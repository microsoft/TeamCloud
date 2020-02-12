/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Newtonsoft.Json.Linq;
using TeamCloud.Http;

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeploymentService
    {
        Task<IAzureDeployment> DeployTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName = null, bool completeMode = false);

        Task<string> ValidateTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName = null, bool throwOnError = false);
    }

    public class AzureDeploymentService : IAzureDeploymentService
    {
        private readonly IAzureDeploymentOptions azureDeploymentOptions;
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureDeploymentArtifactsProvider azureDeploymentArtifactsStorage;

        public AzureDeploymentService(IAzureDeploymentOptions azureDeploymentOptions, IAzureSessionService azureSessionService, IAzureDeploymentArtifactsProvider azureDeploymentArtifactsStorage)
        {
            this.azureDeploymentOptions = azureDeploymentOptions ?? throw new ArgumentNullException(nameof(azureDeploymentOptions));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureDeploymentArtifactsStorage = azureDeploymentArtifactsStorage ?? throw new ArgumentNullException(nameof(azureDeploymentArtifactsStorage));
        }

        public async Task<IAzureDeployment> DeployTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName = null, bool completeMode = false)
        {
            var deploymentId = Guid.NewGuid();

            var deploymentPayload = await GetDeploymentPayloadAsync(deploymentId, template, subscriptionId, resourceGroupName, completeMode ? DeploymentMode.Complete : DeploymentMode.Incremental)
                .ConfigureAwait(false);

            var deploymentResourceId = string.IsNullOrEmpty(resourceGroupName)
                ? $"/subscriptions/{subscriptionId}/providers/Microsoft.Resources/deployments/{deploymentId}"
                : $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentId}";

            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            try
            {
                _ = await azureSessionService.Environment.ResourceManagerEndpoint
                    .AppendPathSegment(deploymentResourceId)
                    .SetQueryParam("api-version", "2019-05-01")
                    .WithOAuthBearerToken(token)
                    .PutJsonAsync(deploymentPayload)
                    .ConfigureAwait(false);
            }
            catch (FlurlHttpException exc) when (exc.Call.HttpStatus == System.Net.HttpStatusCode.BadRequest)
            {
                var validationResultJson = await exc.Call.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var validationResultMessage = JObject.Parse(validationResultJson).SelectToken("$..message")?.ToString();

                throw new AzureDeploymentException($"Invalid deployment template: {validationResultMessage}", deploymentResourceId, validationResultMessage);
            }

            return new AzureDeployment(deploymentResourceId, azureSessionService);
        }

        public async Task<string> ValidateTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName = null, bool throwOnError = false)
        {
            var deploymentId = Guid.NewGuid();

            var deploymentPayload = await GetDeploymentPayloadAsync(deploymentId, template, subscriptionId, resourceGroupName, DeploymentMode.Incremental)
                .ConfigureAwait(false);

            var deploymentResourceId = string.IsNullOrEmpty(resourceGroupName)
                ? $"/subscriptions/{subscriptionId}/providers/Microsoft.Resources/deployments/{deploymentId}/validate"
                : $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentId}/validate";

            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            try
            {
                _ = await azureSessionService.Environment.ResourceManagerEndpoint
                    .AppendPathSegment(deploymentResourceId)
                    .SetQueryParam("api-version", "2019-10-01")
                    .WithOAuthBearerToken(token)
                    .PostJsonAsync(deploymentPayload)
                    .ConfigureAwait(false);

                return null;
            }
            catch (FlurlHttpException exc) when (exc.Call.HttpStatus == System.Net.HttpStatusCode.BadRequest)
            {
                var validationResultJson = await exc.Call.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var validationResultMessage = JObject.Parse(validationResultJson).SelectToken("$..message")?.ToString();

                if (throwOnError)
                    throw new AzureDeploymentException($"Invalid deployment template: {validationResultMessage}", deploymentResourceId, validationResultMessage);

                return validationResultMessage;
            }
        }

        private async Task<object> GetDeploymentPayloadAsync(Guid deploymentId, AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName, DeploymentMode deploymentMode)
        {
            if (string.IsNullOrEmpty(template.Template))
                throw new ArgumentException("Unable to create deployment payload by an empty template.", nameof(template));

            if (template.LinkedTemplates?.Any() ?? false)
            {
                var deploymentContainer = await azureDeploymentArtifactsStorage
                    .UploadArtifactsAsync(deploymentId, template)
                    .ConfigureAwait(false);

                template.Parameters[IAzureDeploymentTemplate.ArtifactsLocationParameterName] = deploymentContainer.Location;
                template.Parameters[IAzureDeploymentTemplate.ArtifactsLocationSasTokenParameterName] = deploymentContainer.Token;
            }

            IDictionary<string, object> deploymentParameters = null;

            if (template.Parameters?.Any() ?? false)
            {
                deploymentParameters = template.Parameters
                    .Where(param => param.Value != null)
                    .Aggregate(new ExpandoObject() as IDictionary<string, object>, (a, kv) => { a.Add(kv.Key, new { value = kv.Value }); return a; });
            }

            var deploymentLocation = string.IsNullOrEmpty(resourceGroupName)
                ? azureDeploymentOptions.Region
                : await GetResourceGroupLocationAsync(subscriptionId, resourceGroupName).ConfigureAwait(false);

            return new
            {
                location = deploymentLocation,
                properties = new DeploymentProperties()
                {
                    Mode = deploymentMode,
                    Template = JObject.Parse(template.Template),
                    Parameters = JObject.FromObject(deploymentParameters)
                }
            };
        }

        private async Task<string> GetResourceGroupLocationAsync(Guid subscriptionId, string resourceGroupName)
        {
            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var json = await azureSessionService.Environment.ResourceManagerEndpoint
                .AppendPathSegment($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}")
                .SetQueryParam("api-version", "2014-04-01")
                .WithOAuthBearerToken(token)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            return json.SelectToken("$.location")?.ToString();
        }
    }
}
