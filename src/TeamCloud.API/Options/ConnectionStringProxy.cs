using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Data.Cosmos;

namespace TeamCloud.API.Options
{
    [Options]
    public class ConnectionStringProxy : ICosmosOptions
    {
        const string AZURE_COSMOSDB_NAME = "TeamCloud";

        private readonly ConnectionStringOptions connectionStringOptions;

        public ConnectionStringProxy(ConnectionStringOptions connectionStringOptions)
        {
            this.connectionStringOptions = connectionStringOptions;
        }

        string ICosmosOptions.AzureCosmosDBName => AZURE_COSMOSDB_NAME;
        string ICosmosOptions.AzureCosmosDBConnection => connectionStringOptions.CosmosDbConnectionString;
    }
}
