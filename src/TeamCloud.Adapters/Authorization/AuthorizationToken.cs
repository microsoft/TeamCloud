/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;

namespace TeamCloud.Adapters.Authorization
{
    public abstract class AuthorizationToken : AuthorizationEntity
    {
        protected AuthorizationToken(string authorizationId = null)
        {
            Entity.RowKey = string.IsNullOrWhiteSpace(authorizationId) ? Guid.Empty.ToString() : authorizationId;
            Entity.PartitionKey = string.Join(',', this.GetType().AssemblyQualifiedName.Split(',').Take(2));
        }

        public string TokenId
            => Entity.RowKey;
    }
}
