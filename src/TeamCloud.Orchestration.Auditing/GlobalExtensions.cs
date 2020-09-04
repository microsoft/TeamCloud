using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Orchestration.Auditing
{
    public static class GlobalExtensions
    {
        public static IServiceCollection AddTeamCloudAuditing(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .TryAddSingleton<ICommandAuditWriter, CommandAuditWriter>();

            return serviceCollection;
        }
    }
}
