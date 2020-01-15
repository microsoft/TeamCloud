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

namespace TeamCloud.Azure.Deployments
{
    public interface IAzureDeploymentService
    {
        Task<IAzureDeployment> DeployTemplateAsync(IAzureDeploymentTemplate template, Guid subscriptionId, string resourceGroupName = null, bool completeMode = false);
    }

    public class AzureDeploymentService: IAzureDeploymentService
    {
        private readonly IAzureSessionService azureSessionFactory;
        private readonly IAzureDeploymentArtifactsStorage azureDeploymentArtifactsStorage;

        public AzureDeploymentService(IAzureSessionService azureSessionFactory, IAzureDeploymentArtifactsStorage azureDeploymentArtifactsStorage)
        {
            this.azureSessionFactory = azureSessionFactory ?? throw new ArgumentNullException(nameof(azureSessionFactory));
            this.azureDeploymentArtifactsStorage = azureDeploymentArtifactsStorage ?? throw new ArgumentNullException(nameof(azureDeploymentArtifactsStorage));
        }

        public async Task<IAzureDeployment> DeployTemplateAsync(IAzureDeploymentTemplate azureDeploymentTemplate, Guid subscriptionId, string resourceGroupName = null, bool completeMode = false)
        {
            var deploymentId = Guid.NewGuid();

            if (azureDeploymentTemplate.LinkedTemplates?.Any() ?? false)
            {
                var deploymentContainer = await azureDeploymentArtifactsStorage
                    .CreateContainerAsync(deploymentId, azureDeploymentTemplate)
                    .ConfigureAwait(false);

                azureDeploymentTemplate.Parameters[IAzureDeploymentTemplate.ArtifactsLocationParameterName] = deploymentContainer.Location;
                azureDeploymentTemplate.Parameters[IAzureDeploymentTemplate.ArtifactsLocationSasTokenParameterName] = deploymentContainer.Token;
            }

            var deploymentParameters = (azureDeploymentTemplate.Parameters?.Any() ?? false)
                ? azureDeploymentTemplate.Parameters.Aggregate(new ExpandoObject() as IDictionary<string, object>, (a, kv) => { a.Add(kv.Key, new { value = kv.Value }); return a; })
                : null;

            var deploymentLocation = string.IsNullOrEmpty(resourceGroupName)
                ? azureSessionFactory.Options.DefaultLocation
                : await GetResourceGroupLocationAsync(subscriptionId, resourceGroupName).ConfigureAwait(false); 

            var deploymentPayload = new
            {
                location = deploymentLocation,
                properties = new DeploymentProperties()
                {
                    Mode = completeMode ? DeploymentMode.Complete : DeploymentMode.Incremental,
                    Template = JObject.Parse(azureDeploymentTemplate.Template),
                    Parameters = JObject.FromObject(deploymentParameters)
                }
            };

            var deploymentResourceId = string.IsNullOrEmpty(resourceGroupName)
                ? $"/subscriptions/{subscriptionId}/providers/Microsoft.Resources/deployments/{deploymentId}"
                : $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Resources/deployments/{deploymentId}";

            var token = await azureSessionFactory
                .AcquireTokenAsync(AzureAuthorities.AzureResourceManager)
                .ConfigureAwait(false);

            _ = await AzureAuthorities.AzureResourceManager
                .AppendPathSegment(deploymentResourceId)
                .SetQueryParam("api-version", "2019-05-01")
                .WithOAuthBearerToken(token)
                .PutJsonAsync(deploymentPayload)
                .ConfigureAwait(false);

            return new AzureDeployment(deploymentResourceId, azureSessionFactory);
        }

        private async Task<string> GetResourceGroupLocationAsync(Guid subscriptionId, string resourceGroupName)
        {
            var token = await azureSessionFactory
                .AcquireTokenAsync(AzureAuthorities.AzureResourceManager)
                .ConfigureAwait(false);

            var json = await AzureAuthorities.AzureResourceManager
                .AppendPathSegment($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}")
                .SetQueryParam("api-version", "2014-04-01")
                .WithOAuthBearerToken(token)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            return json.SelectToken("$.location")?.ToString();
        }
    }
}
