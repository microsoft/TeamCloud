/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.Orchestrator.Options
{
    public interface ISignalROptions
    {
        string ConnectionString { get; }
    }

    [Options]
    public class TeamCloudSignalROptions : ISignalROptions
    {
        private readonly AzureSignalROptions azureSignalROptions;

        public TeamCloudSignalROptions(AzureSignalROptions azureSignalROptions)
        {
            this.azureSignalROptions = azureSignalROptions ?? throw new System.ArgumentNullException(nameof(azureSignalROptions));
        }

        string ISignalROptions.ConnectionString => azureSignalROptions.ConnectionString;
    }
}
