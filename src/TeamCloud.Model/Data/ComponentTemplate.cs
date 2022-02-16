/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data;

public abstract class ComponentTemplate<TConfiguration> : ComponentTemplate
    where TConfiguration : class, new()
{
    internal ComponentTemplate()
    {
        Configuration = Activator.CreateInstance<TConfiguration>();
    }

    public new TConfiguration Configuration
    {
        get => base.Configuration as TConfiguration;
        set => base.Configuration = value;
    }
}

[JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
public class ComponentTemplate : ContainerDocument, IOrganizationContext, IRepositoryReference
{
    /// <summary>
    /// Gets or sets the organization this component template belongs to.
    /// </summary>
    [PartitionKey]
    [JsonProperty(Required = Required.Always)]
    public string Organization { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string OrganizationName { get; set; }

    /// <summary>
    /// Gets or sets the parent identifier.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string ParentId { get; set; }

    /// <summary>
    /// Gets or sets the component template's display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the component template's description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the component template's repository reference.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public RepositoryReference Repository { get; set; }

    /// <summary>
    /// Gets or sets the permissions dictionary to be applied to instances of this component.
    /// </summary>
    public Dictionary<ProjectUserRole, IEnumerable<string>> Permissions { get; set; }

    /// <summary>
    /// Gets or sets the component template's input json schema.
    /// </summary>
    public string InputJsonSchema { get; set; }

    /// <summary>
    /// Gets or sets the component template's tasks.
    /// </summary>
    /// <value></value>
    public List<ComponentTaskTemplate> Tasks { get; set; }

    /// <summary>
    /// Gets or sets the runner to execute a task.
    /// </summary>
    public ComponentTaskRunner TaskRunner { get; set; }

    /// <summary>
    /// Gets or sets the component template's type.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public ComponentType Type { get; set; }

    /// <summary>
    /// Gets or sets the folder of the component in the repo
    /// </summary>
    public string Folder { get; set; }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public object Configuration { get; set; }

    /// <summary>
    /// Equalses the specified other.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <returns></returns>
    public bool Equals(ComponentTemplate other)
        => Id.Equals(other?.Id, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
        => base.Equals(obj) || Equals(obj as ComponentTemplate);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode()
        => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
}
