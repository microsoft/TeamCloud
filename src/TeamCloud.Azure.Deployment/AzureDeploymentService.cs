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

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeploymentService
    {
        Task<IAzureDeployment> GetAzureDeploymentAsync(string resourceId);

        Task<IAzureDeployment> GetAzureDeploymentAsync(Guid subscriptionId, Guid deploymentId);

        Task<IAzureDeployment> GetAzureDeploymentAsync(Guid subscriptionId, string resourceGroupName, Guid deploymentId);

        Task<IAzureDeployment> DeploySubscriptionTemplateAsync(AzureDeploymentTemplate deploymentTemplate, Guid subscriptionId, string location);

        Task<IAzureDeployment> DeployResourceGroupTemplateAsync(AzureDeploymentTemplate deploymentTemplate, Guid subscriptionId, string resourceGroupName, bool completeMode = false);

        Task<IEnumerable<string>> ValidateSubscriptionTemplateAsync(AzureDeploymentTemplate deploymentTemplate, Guid subscriptionId, string location, bool throwOnError = false);

        Task<IEnumerable<string>> ValidateResourceGroupTemplateAsync(AzureDeploymentTemplate deploymentTemplate, Guid subscriptionId, string resourceGroupName, bool throwOnError = false);
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

        public async Task<IAzureDeployment> GetAzureDeploymentAsync(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentException($"Argument '{nameof(resourceId)}' must not NULL or WHITESPACE", nameof(resourceId));

            if (!resourceId.Contains("/providers/Microsoft.Resources/deployments/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Argument '{nameof(resourceId)}' must be a valid deployment resource Id", nameof(resourceId));

            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var response = await azureSessionService.Environment.ResourceManagerEndpoint
                .AppendPathSegment(resourceId)
                .SetQueryParam("api-version", "2019-05-01")
                .AllowAnyHttpStatus()
                .WithOAuthBearerToken(token)
                .GetAsync(completionOption: System.Net.Http.HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode
                ? new AzureDeployment(resourceId, azureSessionService)
                : null;
        }

        public Task<IAzureDeployment> GetAzureDeploymentAsync(Guid subscriptionId, Guid deploymentId)
            => GetAzureDeploymentAsync($"/subscriptions/{subscriptionId}/providers/Microsoft.Resources/deployments/{deploymentId}");

        public Task<IAzureDeployment> GetAzureDeploymentAsync(Guid subscriptionId, string resourceGroupName, Guid deploymentId)
            => GetAzureDeploymentAsync($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentId}");

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
                var validationResultError = JObject.Parse(validationResultJson).SelectToken("$..error");
                var validationResultResourceErrors = AzureDeploymentException.ResolveResourceErrors(validationResultError);

                throw new AzureDeploymentException($"Invalid deployment template: {string.Join(", ", validationResultResourceErrors)}", deploymentResourceId, validationResultResourceErrors.ToArray());
            }

            return new AzureDeployment(deploymentResourceId, azureSessionService);
        }

        public async Task<IEnumerable<string>> ValidateSubscriptionTemplateAsync(AzureDeploymentTemplate deploymentTemplate, Guid subscriptionId, string location, bool throwOnError = false)
        {
            if (deploymentTemplate is null)
                throw new ArgumentNullException(nameof(deploymentTemplate));

            var deploymentId = Guid.NewGuid();
            var deploymentResourceId = $"/subscriptions/{subscriptionId}/providers/Microsoft.Resources/deployments/{deploymentId}/validate";

            var payload = await GetDeploymentPayloadAsync(deploymentId, deploymentTemplate, location, DeploymentMode.Incremental)
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
                var validationResultError = JObject.Parse(validationResultJson).SelectToken("$..error");
                var validationResultResourceErrors = AzureDeploymentException.ResolveResourceErrors(validationResultError);

                if (throwOnError)
                    throw new AzureDeploymentException($"Invalid deployment template: {string.Join(", ", validationResultResourceErrors)}", deploymentResourceId, validationResultResourceErrors.ToArray());

                return validationResultResourceErrors;
            }
        }

        public async Task<IAzureDeployment> DeployResourceGroupTemplateAsync(AzureDeploymentTemplate deploymentTemplate, Guid subscriptionId, string resourceGroupName, bool completeMode = false)
        {
            if (deploymentTemplate is null)
                throw new ArgumentNullException(nameof(deploymentTemplate));

            var deploymentId = Guid.NewGuid();
            var deploymentResourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentId}";

            var payload = await GetDeploymentPayloadAsync(deploymentId, deploymentTemplate, null, completeMode ? DeploymentMode.Complete : DeploymentMode.Incremental)
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
                var validationResultError = JObject.Parse(validationResultJson).SelectToken("$..error");
                var validationResultResourceErrors = AzureDeploymentException.ResolveResourceErrors(validationResultError);

                throw new AzureDeploymentException($"Invalid deployment template: {string.Join(", ", validationResultResourceErrors)}", deploymentResourceId, validationResultResourceErrors.ToArray());
            }

            return new AzureDeployment(deploymentResourceId, azureSessionService);
        }

        public async Task<IEnumerable<string>> ValidateResourceGroupTemplateAsync(AzureDeploymentTemplate deploymentTemplate, Guid subscriptionId, string resourceGroupName, bool throwOnError = false)
        {
            if (deploymentTemplate is null)
                throw new ArgumentNullException(nameof(deploymentTemplate));

            var deploymentId = Guid.NewGuid();
            var deploymentResourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentId}/validate";

            var payload = await GetDeploymentPayloadAsync(deploymentId, deploymentTemplate, null, DeploymentMode.Incremental)
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
                var validationResultError = JObject.Parse(validationResultJson).SelectToken("$..error");
                var validationResultResourceErrors = AzureDeploymentException.ResolveResourceErrors(validationResultError);

                if (throwOnError)
                    throw new AzureDeploymentException($"Invalid deployment template: {string.Join(", ", validationResultResourceErrors)}", deploymentResourceId, validationResultResourceErrors.ToArray());

                return validationResultResourceErrors;
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
