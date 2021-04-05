/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Options
{
    public interface IAzureSignalROptions
    {
        string ConnectionString { get; }
    }

    [Options]
    public class TeamCloudSignalROptions : IAzureSignalROptions
    {
        private readonly AzureSignalROptions azureSignalROptions;

        public TeamCloudSignalROptions(AzureSignalROptions azureSignalROptions)
        {
            this.azureSignalROptions = azureSignalROptions ?? throw new System.ArgumentNullException(nameof(azureSignalROptions));
        }

        string IAzureSignalROptions.ConnectionString => azureSignalROptions.ConnectionString;
    }
}
