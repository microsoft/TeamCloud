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
    public sealed class Component : IComponent, IEquatable<Component>, IValidatable
    {
        public string Id { get; set; }
            = Guid.NewGuid().ToString();

        public string OfferId { get; set; }

        public string ProjectId { get; set; }

        public string ProviderId { get; set; }

        public string RequesterId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public object Input { get; set; }

        public object Value { get; set; }

        public bool Equals(Component other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as Component);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
