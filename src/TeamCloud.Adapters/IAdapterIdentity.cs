using System.Threading.Tasks;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters
{
    public interface IAdapterIdentity
    {
        DeploymentScopeType Type { get; }

        Task<AzureServicePrincipal> GetIdentityAsync(DeploymentScope deploymentScope, Project project, bool withPassword = false);
    }
}
