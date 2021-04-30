/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;

namespace TeamCloud.Adapters.Authorization
{
    public interface IAuthorizationSessionClient
    {
        Task<AuthorizationSession> GetAsync(string authId);

        public async Task<AuthorizationSession> GetAsync<TAuthorizationSession>(string authId)
             where TAuthorizationSession : AuthorizationSession
        {
            return (await GetAsync(authId).ConfigureAwait(false)) as TAuthorizationSession;
        }

        Task<AuthorizationSession> SetAsync(AuthorizationSession authorizationSession);

        public async Task<TAuthorizationSession> SetAsync<TAuthorizationSession>(TAuthorizationSession authorizationSession)
            where TAuthorizationSession : AuthorizationSession
        {
            return (await SetAsync((AuthorizationSession)authorizationSession).ConfigureAwait(false)) as TAuthorizationSession;
        }
    }
}
