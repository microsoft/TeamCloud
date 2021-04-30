/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;

namespace TeamCloud.Adapters.Authorization
{
    public interface IAuthorizationTokenClient
    {
        Task<AuthorizationToken> GetAsync(string authId);

        public async Task<TAuthorizationToken> GetAsync<TAuthorizationToken>(string authId)
            where TAuthorizationToken : AuthorizationToken
        {
            return (await GetAsync(authId).ConfigureAwait(false)) as TAuthorizationToken;
        }

        Task<AuthorizationToken> SetAsync(AuthorizationToken authorizationToken);

        public async Task<TAuthorizationToken> SetAsync<TAuthorizationToken>(TAuthorizationToken authorizationToken)
            where TAuthorizationToken : AuthorizationToken
        {
            return (await SetAsync((AuthorizationToken)authorizationToken).ConfigureAwait(false)) as TAuthorizationToken;
        }
    }
}
