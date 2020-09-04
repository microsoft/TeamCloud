/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Audit
{
    public static class GlobalExtensions
    {
        public static IServiceCollection AddTeamCloudAudit(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .TryAddSingleton<ICommandAuditWriter, CommandAuditWriter>();

            return serviceCollection;
        }
    }
}
