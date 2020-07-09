/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;

namespace TeamCloud.Azure.Deployment.Providers
{
    public sealed class NoneStorageArtifactsProvider : AzureDeploymentArtifactsProvider
    {
        public static readonly IAzureDeploymentArtifactsProvider Instance = new NoneStorageArtifactsProvider();

        public override Task<string> DownloadArtifactAsync(Guid deploymentId, string artifactName)
            => Task.FromResult(default(string));

        public override Task<IAzureDeploymentArtifactsContainer> UploadArtifactsAsync(Guid deploymentId, AzureDeploymentTemplate azureDeploymentTemplate)
            => (azureDeploymentTemplate?.LinkedTemplates.Any() ?? false)
            ? Task.FromException<IAzureDeploymentArtifactsContainer>(new NotSupportedException($"{nameof(NoneStorageArtifactsProvider)} doesn't support linked templates or artifacts"))
            : Task.FromResult<IAzureDeploymentArtifactsContainer>(Container.Instance);

        internal sealed class Container : IAzureDeploymentArtifactsContainer
        {
            public static readonly IAzureDeploymentArtifactsContainer Instance = new Container();

            public string Location => default;

            public string Token => default;
        }
    }
}
