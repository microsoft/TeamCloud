using System;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Services
{

    public interface IOneTimeTokenServiceOptions
    {
        public string ConnectionString { get; }
    }

    [Options]
    public sealed class OneTimeTokenServiceOptions : IOneTimeTokenServiceOptions
    {
        private readonly AzureStorageOptions azureStorageOptions;

        public OneTimeTokenServiceOptions(AzureStorageOptions azureStorageOptions)
        {
            this.azureStorageOptions = azureStorageOptions ?? throw new ArgumentNullException(nameof(azureStorageOptions));
        }

        public string ConnectionString => azureStorageOptions.ConnectionString;
    }
}
