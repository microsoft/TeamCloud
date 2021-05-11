using System;

namespace TeamCloud.Configuration.Options
{
    [Options("Adapter:Session:Storage")]
    public sealed class AdapterSessionStorageOptions
    {
        public string ConnectionString { get; set; } = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    }
}
