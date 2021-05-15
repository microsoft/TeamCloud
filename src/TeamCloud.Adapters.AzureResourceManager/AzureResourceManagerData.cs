/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using TeamCloud.Model.Common;
using TeamCloud.Serialization.Forms;

namespace TeamCloud.Adapters.AzureResourceManager
{
    [TeamCloudFormOrder(nameof(ManagementGroupId), nameof(SubscriptionIds))]
    public sealed class AzureResourceManagerData : IValidatable
    {
        //[TeamCloudFormField("SubscriptionField")]
        //[TeamCloudFormTemplate(TeamCloudFormTemplateType.ArrayFieldTemplate, "SubscriptionTemplate")]
        [TeamCloudFormTitle("Subscriptions")]
        [TeamCloudFormDescription("Azure Subscriptions to use as a deployment target.")]
        public IList<string> SubscriptionIds { get; set; }

        //[TeamCloudFormWidget("ManagementGroupWidget")]
        [TeamCloudFormTitle("Management Group")]
        [TeamCloudFormDescription("Azure Management Group to use as a deployment target.")]
        public string ManagementGroupId { get; set; }
    }
}
