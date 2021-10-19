using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization
{
    public interface IAuthorizationEndpointsResolver
    {
        Task<IAuthorizationEndpoints> GetAuthorizationEndpointsAsync(DeploymentScope deploymentScope);
    }
}
