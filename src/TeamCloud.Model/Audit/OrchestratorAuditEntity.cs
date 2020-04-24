/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.WindowsAzure.Storage.Table;

namespace TeamCloud.Model.Audit
{
    public sealed class OrchestratorAuditEntity : CommandAuditEntity
    {
        [IgnoreProperty]
        public string ProviderId
        {
            get => this.TableEntity.RowKey;
            set => this.TableEntity.RowKey = value;
        }

        [IgnoreProperty]
        public override string CommandId
        {
            get => this.TableEntity.PartitionKey;
            set => this.TableEntity.PartitionKey = value;
        }
    }
}
