using System;
using System.Threading.Tasks;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders
{
    public sealed class ProjectIdentityExpander : IDocumentExpander<ProjectIdentity>
    {
        private readonly IAzureDirectoryService azureDirectoryService;

        public ProjectIdentityExpander(IAzureDirectoryService azureDirectoryService)
        {
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
        }

        public bool CanExpand(ProjectIdentity document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            return document.RedirectUrls == null;
        }

        public async Task<ProjectIdentity> ExpandAsync(ProjectIdentity document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            document.RedirectUrls = await azureDirectoryService
                .GetServicePrincipalRedirectUrlsAsync(document.ObjectId.ToString())
                .ConfigureAwait(false);

            return document;
        }
    }
}
