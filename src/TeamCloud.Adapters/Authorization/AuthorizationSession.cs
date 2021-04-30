/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Globalization;
using System.Linq;

namespace TeamCloud.Adapters.Authorization
{
    public abstract class AuthorizationSession<TAdapter> : AuthorizationSession
        where TAdapter : Adapter
    {
        protected AuthorizationSession(Guid? authId = null, TimeSpan? sessionTTL = null) : base(typeof(TAdapter), authId, sessionTTL)
        { }
    }

    public abstract class AuthorizationSession : AuthorizationEntity
    {
        public static TimeSpan DefaultTTL => TimeSpan.FromMinutes(5);

        private readonly Type adapter;
        private Guid sessionId;
        private TimeSpan sessionTTL;

        internal AuthorizationSession(Type adapter, Guid? authId = null, TimeSpan? sessionTTL = null)
        {
            Entity.RowKey = authId.GetValueOrDefault(Guid.NewGuid()).ToString();
            Entity.PartitionKey = string.Join(',', this.GetType().AssemblyQualifiedName.Split(',').Take(2));

            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.sessionId = Guid.NewGuid();
            this.sessionTTL = sessionTTL.GetValueOrDefault(DefaultTTL);
        }

        public string AuthId
            => Entity.RowKey;

        public Type Adapter
            => adapter;

        public string SessionId
        {
            get => sessionId.ToString();
            protected set => sessionId = Guid.Parse(value);
        }

        public string SessionTTL
        {
            get => sessionTTL.ToString();
            protected set => sessionTTL = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }

        public bool Active
            => (Entity.Timestamp.Add(sessionTTL) > DateTimeOffset.Now);
    }
}
