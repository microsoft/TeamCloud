/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using TeamCloud.Model.Common;
using TeamCloud.Serialization.Forms;

namespace TeamCloud.Adapters.AzureResourceManager
{
    [TeamCloudFormTitle("Subscription Source")]
    [TeamCloudFormOrder(nameof(ManagementGroupId), nameof(SubscriptionIds))]
    public sealed class AzureResourceManagerData : IValidatable
    {
        [TeamCloudFormTitle("Subscriptions")]
        [TeamCloudFormDescription("Azure Subscriptions to use as a deployment target.")]
        public IList<string> SubscriptionIds { get; set; }

        [TeamCloudFormTitle("Management Group")]
        [TeamCloudFormDescription("Azure Management Group to use as a deployment target.")]
        public string ManagementGroupId { get; set; }
    }
}
