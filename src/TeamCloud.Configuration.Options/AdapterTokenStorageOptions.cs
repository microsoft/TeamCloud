/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Configuration.Options
{
    [Options("Adapter:Token:Storage")]
    public sealed class AdapterTokenStorageOptions
    {
        public string ConnectionString { get; set; } = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    }
}
