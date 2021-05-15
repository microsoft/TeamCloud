/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.GitHub
{
    public sealed class GitHubSession : AuthorizationSession
    {
        public GitHubSession(DeploymentScope deploymentScope = null) : base(GetEntityId(deploymentScope))
        { }

        public string TeamCloudOrganization { get; internal set; }
        public string TeamCloudDeploymentScope { get; internal set; }
    }
}
