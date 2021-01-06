using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IComponentDeploymentRepository : IDocumentRepository<ComponentDeployment>
    {
        Task RemoveAllAsync(string componentId);

        Task RemoveAsync(string componentId, string id);
    }
}
