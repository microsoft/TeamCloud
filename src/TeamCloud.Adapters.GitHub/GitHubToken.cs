/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.GitHub
{
    public sealed class GitHubToken : AuthorizationToken
    {
        public GitHubToken() : this(null)
        { }

        public GitHubToken(DeploymentScope deployementScope) : base(GetEntityId(deployementScope))
        { }
    }
}
