using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Configuration.Options
{
    [Options("Azure:Storage")]
    public sealed class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    }
}
