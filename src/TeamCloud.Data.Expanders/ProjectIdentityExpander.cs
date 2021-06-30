/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders
{
    public sealed class ProjectIdentityExpander : DocumentExpander,
        IDocumentExpander<ProjectIdentity>
    {
        private readonly IAzureDirectoryService azureDirectoryService;

        public ProjectIdentityExpander(IAzureDirectoryService azureDirectoryService) : base(true)
        {
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
        }

        public async Task<ProjectIdentity> ExpandAsync(ProjectIdentity document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (!(document.RedirectUrls?.Any() ?? false))
            {
                document.RedirectUrls = await azureDirectoryService
                    .GetServicePrincipalRedirectUrlsAsync(document.ObjectId.ToString())
                    .ConfigureAwait(false);
            }

            return document;
        }
    }
}
