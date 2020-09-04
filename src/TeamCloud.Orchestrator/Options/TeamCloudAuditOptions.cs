/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Extensions.Configuration;
using TeamCloud.Audit;
using TeamCloud.Configuration;

namespace TeamCloud.Orchestrator.Options
{
    [Options]
    public sealed class TeamCloudAuditOptions : ICommandAuditOptions
    {
        private readonly IConfiguration configuration;

        public TeamCloudAuditOptions(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        string ICommandAuditOptions.ConnectionString
            => CommandAuditOptions.Default.ConnectionString;

        string ICommandAuditOptions.StoragePrefix
            => configuration.GetValue<string>("AzureFunctionsJobHost:extensions:durableTask:hubName");
    }
}
