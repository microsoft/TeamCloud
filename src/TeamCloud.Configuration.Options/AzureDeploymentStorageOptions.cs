/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Configuration.Options;

[Options("Azure:DeploymentStorage")]
public class AzureDeploymentStorageOptions
{
    public string ConnectionString { get; set; } = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

    public string BaseUrlOverride { get; set; }
}
