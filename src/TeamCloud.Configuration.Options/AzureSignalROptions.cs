/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Configuration.Options;

[Options("Azure:SignalR")]
public class AzureSignalROptions
{
    public string ConnectionString { get; set; }
}
