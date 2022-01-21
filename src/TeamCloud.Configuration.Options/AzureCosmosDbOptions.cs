/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Configuration.Options;

[Options("Azure:CosmosDb")]
public class AzureCosmosDbOptions
{
    public string DatabaseName { get; set; } = "TeamCloud";

    public string ConnectionString { get; set; }
}
