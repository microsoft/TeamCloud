using System.Threading;
using System.Threading.Tasks;

namespace TeamCloud.Git.Caching
{
    public interface IRepositoryCache
    {
        bool InMemory { get; }

        Task<string> GetAsync(string endpoint, CancellationToken cancellationToken = default);

        Task SetAsync(string endpoint, string value, CancellationToken cancellationToken = default);
    }
}
