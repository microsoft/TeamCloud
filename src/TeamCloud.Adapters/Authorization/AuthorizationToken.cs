/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;

namespace TeamCloud.Adapters.Authorization
{
    public abstract class AuthorizationToken<TAdapter> : AuthorizationToken
        where TAdapter : Adapter
    {
        protected AuthorizationToken(Guid? authId = null) : base(typeof(TAdapter), authId)
        { }
    }

    public abstract class AuthorizationToken : AuthorizationEntity
    {
        private readonly Type adapter;

        internal AuthorizationToken(Type adapter, Guid? authId = null)
        {
            Entity.RowKey = authId.GetValueOrDefault(Guid.NewGuid()).ToString();
            Entity.PartitionKey = string.Join(',', this.GetType().AssemblyQualifiedName.Split(',').Take(2));

            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public Type Adapter
            => adapter;
    }
}
