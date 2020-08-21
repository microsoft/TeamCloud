using System;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectLinkDocument
        : ContainerDocument, IProjectLink, IEquatable<ProjectLinkDocument>, IPopulate<ProjectLink>
    {
        [PartitionKey]
        public string ProjectId { get; set; }

        [JsonProperty("href")]
        public string HRef { get; set; }

        public string Title { get; set; }

        public ProjectLinkType Type { get; set; }

        public bool Equals(ProjectLinkDocument other)
            => Id.Equals(other?.Id);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProjectLinkDocument);

        public override int GetHashCode()
            => HashCode.Combine(Id, HRef);
    }
}
