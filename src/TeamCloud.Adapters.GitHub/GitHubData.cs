using TeamCloud.Serialization.Forms;

namespace TeamCloud.Adapters.GitHub
{
    public sealed class GitHubData
    {
        private string organization;

        [TeamCloudFormTitle("Organization")]
        [TeamCloudFormDescription("Azure DevOps Organization name or base URL.")]
        public string Organization
        {
            get => string.IsNullOrWhiteSpace(organization) ? null : GitHubToken.FormatOrganizationUrl(organization);
            set => organization = value;
        }
    }
}
