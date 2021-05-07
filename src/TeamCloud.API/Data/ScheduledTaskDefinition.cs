/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ScheduledTaskDefinition : IValidatable
    {
        public bool Enabled { get; set; }

        public bool Recurring { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }

        public int UtcHour { get; set; }

        public int UtcMinute { get; set; }

        public List<ComponentTaskReference> ComponentTasks { get; set; }
    }
}
