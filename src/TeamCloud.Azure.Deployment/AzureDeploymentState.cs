/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Deployment;

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
