using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters
{
    public interface IAdapterIdentity : IAdapter
    {
        Task<AzureServicePrincipal> GetServiceIdentityAsync(DeploymentScope deploymentScope, bool withPassword = false);

        Task<AzureServicePrincipal> GetServiceIdentityAsync(Component component, bool withPassword = false);
    }
}
