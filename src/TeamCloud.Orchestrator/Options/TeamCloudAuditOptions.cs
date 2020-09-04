using Microsoft.Extensions.Configuration;
using TeamCloud.Configuration;
using TeamCloud.Orchestration.Auditing;

namespace TeamCloud.Orchestrator.Options
{
    [Options]
    public sealed class TeamCloudAuditOptions : ICommandAuditWriterOptions
    {
        private readonly IConfiguration configuration;

        public TeamCloudAuditOptions(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        string ICommandAuditWriterOptions.ConnectionString
            => CommandAuditWriter.DefaultOptions.ConnectionString;

        string ICommandAuditWriterOptions.StoragePrefix
            => configuration.GetValue<string>("AzureFunctionsJobHost:extensions:durableTask:hubName");
    }
}
