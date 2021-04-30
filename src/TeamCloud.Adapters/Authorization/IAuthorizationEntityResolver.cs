/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos.Table;

namespace TeamCloud.Adapters.Authorization
{
    public interface IAuthorizationEntityResolver
    {
        EntityResolver<AuthorizationEntity> Resolve { get; }
    }
}
