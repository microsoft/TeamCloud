/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Deployments
{
    public enum AzureDeploymentState
    {
        Accepted,

        Cancelled,

        Failed,

        Succeeded,

        Running,

        Deleting,

        Unknown
    }
}
