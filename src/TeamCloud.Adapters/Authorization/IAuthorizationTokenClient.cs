/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization
{
    public interface IAuthorizationTokenClient
    {
        Task<TAuthorizationToken> GetAsync<TAuthorizationToken>(DeploymentScope deploymentScope)
            where TAuthorizationToken : AuthorizationToken;

        Task<TAuthorizationToken> SetAsync<TAuthorizationToken>(TAuthorizationToken authorizationToken)
            where TAuthorizationToken : AuthorizationToken;
    }
}
