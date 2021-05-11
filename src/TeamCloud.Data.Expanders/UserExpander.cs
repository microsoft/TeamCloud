/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders
{
    public sealed class UserExpander : DocumentExpander,
        IDocumentExpander<User>
    {
        private readonly IAzureDirectoryService azureDirectoryService;

        public UserExpander(IAzureDirectoryService azureDirectoryService) : base(true)
        {
            this.azureDirectoryService = azureDirectoryService ?? throw new System.ArgumentNullException(nameof(azureDirectoryService));
        }

        public async Task<User> ExpandAsync(User document)
        {
            if (document is null)
                throw new System.ArgumentNullException(nameof(document));

            document.DisplayName ??= await azureDirectoryService
                .GetDisplayNameAsync(document.Id)
                .ConfigureAwait(false);

            document.LoginName ??= await azureDirectoryService
                .GetLoginNameAsync(document.Id)
                .ConfigureAwait(false);

            document.MailAddress ??= await azureDirectoryService
                .GetMailAddressAsync(document.Id)
                .ConfigureAwait(false);

            return document;
        }
    }
}
