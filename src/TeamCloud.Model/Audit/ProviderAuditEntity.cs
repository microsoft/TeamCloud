using Microsoft.WindowsAzure.Storage.Table;

namespace TeamCloud.Model.Audit
{
    public sealed class ProviderAuditEntity : CommandAuditEntity
    {
        [IgnoreProperty]
        public override string ProjectId
        {
            get => this.TableEntity.PartitionKey;
            set => this.TableEntity.PartitionKey = value;
        }

        [IgnoreProperty]
        public override string CommandId
        {
            get => this.TableEntity.RowKey;
            set => this.TableEntity.RowKey = value;
        }
    }
}
