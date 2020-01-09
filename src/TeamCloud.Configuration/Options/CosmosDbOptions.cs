using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Configuration.Options
{
    [Options("CosmosDb")]
    public class CosmosDbOptions
    {
        public string DatabaseName { get; set; }

        public string ConnectionString { get; set; }
    }
}
