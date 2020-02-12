/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using TeamCloud.Http;

namespace TeamCloud.Azure.Directory
{
    public interface IAzureDirectoryService
    {
        Task<Guid?> GetUserIdAsync(string identifier);

        Task<Guid?> GetGroupIdAsync(string identifier);
    }

    public class AzureDirectoryService : IAzureDirectoryService
    {
        private readonly IAzureSessionService azureSessionService;

        public AzureDirectoryService(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        private async Task<string> GetDefaultDomainAsync()
        {
            var token = await azureSessionService
                .AcquireTokenAsync(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            var json = await azureSessionService.Environment.GraphEndpoint
                .AppendPathSegment($"{azureSessionService.Options.TenantId}/tenantDetails")
                .SetQueryParam("api-version", "1.6")
                .WithOAuthBearerToken(token)
                .GetJObjectAsync();

            return json.SelectToken("$.value[0].verifiedDomains[?(@.default == true)].name")?.ToString();
        }

        private async Task<IEnumerable<string>> GetVerifiedDomainsAsync()
        {
            var token = await azureSessionService
                .AcquireTokenAsync(AzureEndpoint.GraphEndpoint)
                .ConfigureAwait(false);

            var json = await azureSessionService.Environment.GraphEndpoint
                .AppendPathSegment($"{azureSessionService.Options.TenantId}/tenantDetails")
                .SetQueryParam("api-version", "1.6")
                .WithOAuthBearerToken(token)
                .GetJObjectAsync()
                .ConfigureAwait(false);

            return json.SelectTokens("$.value[0].verifiedDomains[*].name").Select(name => name.ToString());
        }

        public async Task<Guid?> GetUserIdAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            var azureSession = azureSessionService.CreateSession();
            var azureUser = default(IActiveDirectoryUser);

            if (identifier.IsEMail())
            {
                var verifiedDomains = await GetVerifiedDomainsAsync()
                    .ConfigureAwait(false);

                if (!verifiedDomains.Any(domain => identifier.EndsWith($"@{domain}", StringComparison.OrdinalIgnoreCase)))
                {
                    var defaultDomain = await GetDefaultDomainAsync().ConfigureAwait(false);

                    identifier = $"{identifier.Replace("@", "_")}#EXT#@{defaultDomain}";
                }

                azureUser = await azureSession.ActiveDirectoryUsers
                    .GetByNameAsync(identifier)
                    .ConfigureAwait(false);
            }
            else if (identifier.IsGuid())
            {
                azureUser = await azureSession.ActiveDirectoryUsers
                    .GetByIdAsync(identifier)
                    .ConfigureAwait(false);
            }

            if (azureUser is null) return null;

            return Guid.Parse(azureUser.Inner.ObjectId);
        }

        public Task<Guid?> GetGroupIdAsync(string identifier)
        {
            throw new NotImplementedException();
        }
    }
}
