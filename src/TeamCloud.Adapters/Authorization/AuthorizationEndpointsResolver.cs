using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization
{
    public sealed class AuthorizationEndpointsResolver : IAuthorizationEndpointsResolver
    {
        private readonly IAuthorizationEndpointsResolverOptions options;

        public AuthorizationEndpointsResolver(IAuthorizationEndpointsResolverOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Task<IAuthorizationEndpoints> GetAuthorizationEndpointsAsync(DeploymentScope deploymentScope)
            => deploymentScope is null ? throw new ArgumentNullException(nameof(deploymentScope)) : Task.FromResult<IAuthorizationEndpoints>(new AuthorizationEndpoints()
            {
                AuthorizationUrl = options.BaseUrl.AppendPathSegments(deploymentScope.ToString(), "authorize").ToString(),
                CallbackUrl = options.BaseUrl.AppendPathSegments(deploymentScope.ToString(), "authorize", "callback").ToString()
            });
    }
}
