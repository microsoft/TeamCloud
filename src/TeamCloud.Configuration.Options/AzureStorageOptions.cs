/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Configuration.Options;

[Options("Azure:Storage")]
public sealed class AzureStorageOptions
{
    public string ConnectionString { get; set; } = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
}
