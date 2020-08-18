using System.Threading.Tasks;

namespace TeamCloud.API.Initialization
{
    public interface IHostInitializer
    {
        Task InitializeAsync();
    }
}
