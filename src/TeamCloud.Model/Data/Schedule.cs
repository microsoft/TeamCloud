/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class Schedule : ContainerDocument, IProjectContext, IEquatable<Schedule>, IValidatable
    {
        [JsonProperty(Required = Required.Always)]
        public string Organization { get; set; }

        [PartitionKey]
        [JsonProperty(Required = Required.Always)]
        public string ProjectId { get; set; }

        public bool Enabled { get; set; }

        public bool Recurring { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }

        public int UtcHour { get; set; }

        public int UtcMinute { get; set; }

        public string Creator { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? LastRun { get; set; }

        public List<ComponentTaskReference> ComponentTasks { get; set; }


        public bool Equals(Schedule other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Schedule);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DayOfWeek
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }
}
