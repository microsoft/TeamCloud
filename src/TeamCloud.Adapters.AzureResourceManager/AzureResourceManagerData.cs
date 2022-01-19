/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using TeamCloud.Serialization.Forms;
using TeamCloud.Validation;

namespace TeamCloud.Adapters.AzureResourceManager;

public sealed class AzureResourceManagerData : IValidatable
{
    [TeamCloudFormTitle("Subscription Source")]
    public AzureResourceManagerSubscriptionSource SubscriptionSource { get; set; }
}

[TeamCloudFormOrder(nameof(ManagementGroupId), nameof(SubscriptionIds))]
public sealed class AzureResourceManagerSubscriptionSource
{
    [TeamCloudFormTitle("Subscriptions")]
    [TeamCloudFormDescription("Azure Subscriptions to use as a deployment target.")]
    public IList<string> SubscriptionIds { get; set; }

    [TeamCloudFormTitle("Management Group")]
    [TeamCloudFormDescription("Azure Management Group to use as a deployment target.")]
    public string ManagementGroupId { get; set; }
}
