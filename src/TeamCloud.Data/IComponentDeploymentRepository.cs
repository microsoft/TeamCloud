using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IComponentDeploymentRepository
    {
        Task<ComponentDeployment> AddAsync(ComponentDeployment deployment);

        Task<ComponentDeployment> GetAsync(string componentId, string id);

        IAsyncEnumerable<ComponentDeployment> ListAsync(string componentId);

        Task<ComponentDeployment> SetAsync(ComponentDeployment deployment);

        Task<ComponentDeployment> RemoveAsync(ComponentDeployment deployment);

        Task RemoveAllAsync(string componentId);

        Task RemoveAsync(string componentId, string id);
    }
}
