/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization
{
    public interface IAuthorizationSessionClient
    {
        Task<TAuthorizationSession> GetAsync<TAuthorizationSession>(DeploymentScope deploymentScope)
            where TAuthorizationSession : AuthorizationSession;

        Task<TAuthorizationSession> SetAsync<TAuthorizationSession>(TAuthorizationSession authorizationSession)
            where TAuthorizationSession : AuthorizationSession;
    }
}
