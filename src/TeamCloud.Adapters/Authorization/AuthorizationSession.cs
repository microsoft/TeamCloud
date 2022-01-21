/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Globalization;
using System.Linq;

namespace TeamCloud.Adapters.Authorization;

public abstract class AuthorizationSession : AuthorizationEntity
{
    public static TimeSpan DefaultTTL
        => TimeSpan.FromMinutes(5);

    private TimeSpan sessionTTL;
    private Guid sessionId;

    protected AuthorizationSession(string entityId = null, TimeSpan? sessionTTL = null)
    {
        Entity.RowKey = string.IsNullOrWhiteSpace(entityId) ? Guid.Empty.ToString() : entityId;
        Entity.PartitionKey = string.Join(',', this.GetType().AssemblyQualifiedName.Split(',').Take(2));

        this.sessionTTL = sessionTTL.GetValueOrDefault(DefaultTTL);
        this.sessionId = Guid.NewGuid();
    }

    public string SessionId
        => Entity.RowKey;

    public string SessionTTL
    {
        get => sessionTTL.ToString();
        private set => sessionTTL = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
    }

    public string SessionState
    {
        get => sessionId.ToString();
        private set => sessionId = Guid.Parse(value);
    }

    public bool Active
        => (Entity.Timestamp.Add(sessionTTL) > DateTimeOffset.Now);
}
