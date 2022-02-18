/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Azure;
using Azure.Data.Tables;

namespace TeamCloud.API.Services;

public class OneTimeTokenServiceEntity : ITableEntity
{
    public static string DefaultPartitionKeyValue => nameof(OneTimeTokenServiceEntity);

    public static string DefaultPartitionKeyFilter => $"PartitionKey eq '{DefaultPartitionKeyValue}'";

    // expression to sanitize a token so it can be used as a rowkey in Azure table storage
    private static readonly Regex DisallowedCharsInRowKeyExpression = new(@"[\\\\#%+/?\u0000-\u001F\u007F-\u009F]");

    public static string CreateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(60);

        return DisallowedCharsInRowKeyExpression.Replace(Convert.ToBase64String(bytes), string.Empty);
    }

    public OneTimeTokenServiceEntity()
    { }

    public OneTimeTokenServiceEntity(Guid organizationId, Guid userId, TimeSpan? ttl = null)
    {
        OrganizationId = organizationId;
        UserId = userId;

        if (ttl.HasValue && ttl.Value > TimeSpan.Zero)
        {
            Expires = DateTimeOffset.UtcNow.Add(ttl.Value);
        }
    }

    public string Token => RowKey;

    public Guid OrganizationId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset Expires { get; set; } = DateTimeOffset.UtcNow.AddMinutes(1);

    public string PartitionKey { get; set; } = DefaultPartitionKeyValue;

    public string RowKey { get; set; } = CreateToken();

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}
