/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Serialization.Forms;

namespace TeamCloud.Adapters.GitHub;

public sealed class GitHubData
{
    private string organization;

    [TeamCloudFormTitle("Organization")]
    [TeamCloudFormDescription("GitHub organization's name or base URL.")]
    public string Organization
    {
        get => string.IsNullOrWhiteSpace(organization) ? null : GitHubToken.SanitizeOrganizationUrl(organization);
        set => organization = value;
    }

    [TeamCloudFormTitle("Public Repository")]
    [TeamCloudFormDescription("Determins if repositories should be public accessible or not.")]
    public bool PublicRepository { get; set; }
}
