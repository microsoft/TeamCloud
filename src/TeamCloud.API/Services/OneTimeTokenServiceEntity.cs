using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Azure.Cosmos.Table;

namespace TeamCloud.API.Services
{
    public class OneTimeTokenServiceEntity : ITableEntity
    {
        public static string DefaultPartitionKeyValue => nameof(OneTimeTokenServiceEntity);

        public static string DefaultPartitionKeyFilter => TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DefaultPartitionKeyValue);

        // expression to sanitize a token so it can be used as a rowkey in Azure table storage
        private static readonly Regex DisallowedCharsInRowKeyExpression = new Regex(@"[\\\\#%+/?\u0000-\u001F\u007F-\u009F]");

        public static string CreateToken()
        {
            using var rng = new RNGCryptoServiceProvider();

            var buffer = new byte[60];

            rng.GetNonZeroBytes(buffer);

            return DisallowedCharsInRowKeyExpression.Replace(Convert.ToBase64String(buffer), string.Empty);
        }

        public OneTimeTokenServiceEntity()
        {}

        public OneTimeTokenServiceEntity(Guid organizationId, Guid userId, TimeSpan? ttl = null)
        {
            OrganizationId = organizationId;
            UserId = userId;

            if (ttl.HasValue && ttl.Value > TimeSpan.Zero)
            {
                Expires = DateTimeOffset.UtcNow.Add(ttl.Value);
            }
        }

        public string Token => TableEntity.RowKey;
        
        public Guid OrganizationId { get; set; }

        public Guid UserId { get; set; }

        public DateTimeOffset Expires { get; set; } = DateTimeOffset.UtcNow.AddMinutes(1);

        public ITableEntity TableEntity => this;

        string ITableEntity.PartitionKey { get; set; } = DefaultPartitionKeyValue;
        
        string ITableEntity.RowKey { get; set; } = CreateToken();

        DateTimeOffset ITableEntity.Timestamp { get; set; }
        
        string ITableEntity.ETag { get; set; }

        void ITableEntity.ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
            => Microsoft.Azure.Cosmos.Table.TableEntity.ReadUserObject(this, properties, operationContext);

        IDictionary<string, EntityProperty> ITableEntity.WriteEntity(OperationContext operationContext)
            => Microsoft.Azure.Cosmos.Table.TableEntity.WriteUserObject(this, operationContext);
    }
}
