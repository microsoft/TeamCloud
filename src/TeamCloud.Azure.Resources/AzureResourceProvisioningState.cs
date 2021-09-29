using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Azure.Resources
{
    public enum AzureResourceProvisioningState
    {
        Canceled,
        Deleting,
        Failed,
        InProgress,
        Succeeded,
        Unknown
    }
}
