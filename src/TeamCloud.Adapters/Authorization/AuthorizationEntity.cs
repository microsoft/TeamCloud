/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Data.Tables;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization;

public abstract class AuthorizationEntity : ITableEntity
{
    public static string GetEntityId(DeploymentScope deploymentScope)
    {
        var entityId = Guid.Empty;

        if (deploymentScope is not null)
        {
            var result = Merge(Guid.Parse(deploymentScope.Organization), Guid.Parse(deploymentScope.Id));

            entityId = new Guid(result.ToArray());
        }

        return entityId.ToString();

        static IEnumerable<byte> Merge(Guid guid1, Guid guid2)
        {
            var buffer1 = guid1.ToByteArray();
            var buffer2 = guid2.ToByteArray();

            for (int i = 0; i < buffer1.Length; i++)
                yield return (byte)(buffer1[i] ^ buffer2[i]);
        }
    }

    internal AuthorizationEntity()
    { }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
