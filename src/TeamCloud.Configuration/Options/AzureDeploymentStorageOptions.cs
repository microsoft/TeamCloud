/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Configuration.Options
{
    [Options("Azure:DeploymentStorage")]
    public class AzureDeploymentStorageOptions
    {
        public string ConnectionString { get; set; }

        public string BaseUrl { get; set; }
    }
}
