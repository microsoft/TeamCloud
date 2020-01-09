using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Configuration.Options
{
    [Options("AzureRM")]
    public class AzureRMOptions
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string TenantId { get; set; }
    }
}
