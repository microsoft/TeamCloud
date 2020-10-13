/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ComponentOffer : IComponentOffer, IEquatable<ComponentOffer>, IValidatable
    {
        public string Id { get; set; }

        public string ProviderId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string InputJsonSchema { get; set; }

        public ComponentScope Scope { get; set; }

        public ComponentType Type { get; set; }


        public bool Equals(ComponentOffer other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ComponentOffer);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
