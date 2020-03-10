﻿/**
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

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeploymentService
    {
        Task<IAzureDeployment> DeploySubscriptionTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string location);

        Task<IAzureDeployment> DeployResourceGroupTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName, bool completeMode = false);

        Task<string> ValidateSubscriptionTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string location, bool throwOnError = false);

        Task<string> ValidateResourceGroupTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName, bool throwOnError = false);
    }

    public class AzureDeploymentService : IAzureDeploymentService
    {
        private readonly IAzureDeploymentOptions azureDeploymentOptions;
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureDeploymentArtifactsProvider azureDeploymentArtifactsStorage;

        public AzureDeploymentService(IAzureDeploymentOptions azureDeploymentOptions, IAzureSessionService azureSessionService, IAzureDeploymentArtifactsProvider azureDeploymentArtifactsStorage)
        {
            this.azureDeploymentOptions = azureDeploymentOptions ?? AzureDeploymentOptions.Default;
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureDeploymentArtifactsStorage = azureDeploymentArtifactsStorage ?? throw new ArgumentNullException(nameof(azureDeploymentArtifactsStorage));
        }

        public async Task<IAzureDeployment> DeploySubscriptionTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string location)
        {
            if (template is null)
                throw new ArgumentNullException(nameof(template));

            if (location is null)
                throw new ArgumentNullException(nameof(location));

            var deploymentId = Guid.NewGuid();
            var deploymentResourceId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Resources/deployments/{deploymentId}";

            var payload = await GetDeploymentPayloadAsync(deploymentId, template, location, DeploymentMode.Incremental)
                .ConfigureAwait(false);

            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            try
            {
                _ = await azureSessionService.Environment.ResourceManagerEndpoint
                    .AppendPathSegment(deploymentResourceId)
                    .SetQueryParam("api-version", "2019-05-01")
                    .WithOAuthBearerToken(token)
                    .PutJsonAsync(payload)
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

        public async Task<string> ValidateSubscriptionTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string location, bool throwOnError = false)
        {
            var deploymentId = Guid.NewGuid();
            var deploymentResourceId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Resources/deployments/{deploymentId}/validate";

            var payload = await GetDeploymentPayloadAsync(deploymentId, template, location, DeploymentMode.Incremental)
                .ConfigureAwait(false);

            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            try
            {
                _ = await azureSessionService.Environment.ResourceManagerEndpoint
                    .AppendPathSegment(deploymentResourceId)
                    .SetQueryParam("api-version", "2019-10-01")
                    .WithOAuthBearerToken(token)
                    .PostJsonAsync(payload)
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

        public async Task<IAzureDeployment> DeployResourceGroupTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName, bool completeMode = false)
        {
            var deploymentId = Guid.NewGuid();
            var deploymentResourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentId}";

            var payload = await GetDeploymentPayloadAsync(deploymentId, template, null, completeMode ? DeploymentMode.Complete : DeploymentMode.Incremental)
                .ConfigureAwait(false);

            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            try
            {
                _ = await azureSessionService.Environment.ResourceManagerEndpoint
                    .AppendPathSegment(deploymentResourceId)
                    .SetQueryParam("api-version", "2019-05-01")
                    .WithOAuthBearerToken(token)
                    .PutJsonAsync(payload)
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

        public async Task<string> ValidateResourceGroupTemplateAsync(AzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName, bool throwOnError = false)
        {
            var deploymentId = Guid.NewGuid();
            var deploymentResourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentId}/validate";

            var payload = await GetDeploymentPayloadAsync(deploymentId, template, null, DeploymentMode.Incremental)
                .ConfigureAwait(false);

            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            try
            {
                _ = await azureSessionService.Environment.ResourceManagerEndpoint
                    .AppendPathSegment(deploymentResourceId)
                    .SetQueryParam("api-version", "2019-10-01")
                    .WithOAuthBearerToken(token)
                    .PostJsonAsync(payload)
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

        private async Task<object> GetDeploymentPayloadAsync(Guid deploymentId, AzureDeploymentTemplate template, string location, DeploymentMode deploymentMode)
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

            var properties = new DeploymentProperties()
            {
                Mode = deploymentMode,
                Template = JObject.Parse(template.Template),
                Parameters = deploymentParameters is null ? new JObject() : JObject.FromObject(deploymentParameters)
            };

            if (string.IsNullOrEmpty(location))
                return new { properties };

            return new { location, properties };
        }
    }
}