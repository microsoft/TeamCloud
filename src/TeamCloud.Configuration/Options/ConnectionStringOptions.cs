using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Configuration.Options
{
    [Options("ConnectionStrings")]
    public class ConnectionStringOptions
    {
        public string CosmosDbConnectionString { get; set; }
    }
}
