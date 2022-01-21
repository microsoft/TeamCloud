/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Serialization.Forms;

namespace TeamCloud.Adapters.AzureDevOps;

public sealed class AzureDevOpsData
{
    private string organization;

    [TeamCloudFormTitle("Organization")]
    [TeamCloudFormDescription("Azure DevOps Organization name or base URL.")]
    public string Organization
    {
        get => string.IsNullOrWhiteSpace(organization) ? null : AzureDevOpsToken.FormatOrganizationUrl(organization);
        set => organization = value;
    }
}
