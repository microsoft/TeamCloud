/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Adapters.Authorization;

namespace TeamCloud.Adapters.GitHub
{
    public sealed class GitHubAuthorizationSession : AuthorizationSession<GitHubAdapter>
    {
        public string TeamCloudOrganization { get; internal set; }
        public string TeamCloudDeploymentScope { get; internal set; }
    }
}
