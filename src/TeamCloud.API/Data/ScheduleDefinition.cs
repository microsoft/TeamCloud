/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;
using TeamCloud.Validation;

namespace TeamCloud.API.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public sealed class ScheduleDefinition : IValidatable
{
    public bool Enabled { get; set; }

    public bool Recurring { get; set; }

    public List<DayOfWeek> DaysOfWeek { get; set; }

    public int UtcHour { get; set; }

    public int UtcMinute { get; set; }

    public List<ComponentTaskReference> ComponentTasks { get; set; }
}
