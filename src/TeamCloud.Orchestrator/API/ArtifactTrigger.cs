/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json.Linq;
using TeamCloud.Azure.Deployment;

namespace TeamCloud.Orchestrator.API
{
    public class ArtifactTrigger
    {
        private readonly IAzureDeploymentArtifactsProvider azureDeploymentArtifactsProvider;

        public ArtifactTrigger(IAzureDeploymentArtifactsProvider azureDeploymentArtifactsProvider = null)
        {
            this.azureDeploymentArtifactsProvider = azureDeploymentArtifactsProvider;
        }

        [FunctionName(nameof(ArtifactTrigger))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "artifacts/{deploymentId:guid}/{artifactName}")] HttpRequest httpRequest,
            string deploymentId,
            string artifactName
            /* ILogger log */)
        {
            if (azureDeploymentArtifactsProvider is null)
                return new NotFoundResult();

            var artifact = await azureDeploymentArtifactsProvider
                .DownloadArtifactAsync(Guid.Parse(deploymentId), artifactName)
                .ConfigureAwait(false);

            if (artifact is null)
                return new NotFoundResult();

            return new JsonResult(JObject.Parse(artifact));
        }
    }
}
