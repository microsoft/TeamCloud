/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using System;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data;

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
[ContainerPath("/orgs/{Organization}/projects/{ProjectId}/components/{ComponentId}/tasks/{Id}")]
public sealed class ComponentTask : ContainerDocument, IEquatable<ComponentTask>, IResourceReference, IComponentContext
{
    private string typeName;

    [JsonProperty(Required = Required.Always)]
    public string Organization { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string OrganizationName { get; set; }

    [PartitionKey]
    [JsonProperty(Required = Required.Always)]
    public string ComponentId { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string ComponentName { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string ProjectId { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string ProjectName { get; set; }

    public string RequestedBy { get; set; }

    public string ScheduleId { get; set; }

    public ComponentTaskType Type { get; set; } = ComponentTaskType.Create;

    public string TypeName
    {
        get => Type == ComponentTaskType.Custom ? typeName : default;
        set => typeName = value;
    }

    public DateTime Created { get; set; } = DateTime.UtcNow;

    public DateTime? Started { get; set; }

    public DateTime? Finished { get; set; }

    public string InputJson { get; set; }

    [DatabaseIgnore]
    public string Output { get; set; }

    public string ResourceId { get; set; }

    public TaskState TaskState { get; set; } = TaskState.Pending;

    public int? ExitCode { get; set; }

    public override object Clone(bool reset)
    {
        var clone = (ComponentTask)base.Clone(reset);

        if (reset)
        {
            clone.TaskState = TaskState.Pending;
            clone.Created = DateTime.UtcNow;
            clone.Started = null;
            clone.Finished = null;
            clone.ExitCode = null;
        }

        return clone;
    }
    public bool Equals(ComponentTask other)
        => Id.Equals(other?.Id, StringComparison.Ordinal);

    public override bool Equals(object obj)
        => base.Equals(obj) || Equals(obj as ComponentTask);

    public override int GetHashCode()
        => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
}
