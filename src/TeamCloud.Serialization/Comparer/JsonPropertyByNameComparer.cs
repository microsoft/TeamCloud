/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization.Comparer
{
    public sealed class JsonPropertyEqualityComparer : EqualityComparer<JsonProperty>
    {
        public static readonly JsonPropertyEqualityComparer ByPropertyName = new JsonPropertyEqualityComparer(JsonPropertyCompareBy.PropertyName);

        private readonly JsonPropertyCompareBy compareBy;

        private JsonPropertyEqualityComparer(JsonPropertyCompareBy compareBy)
        {
            this.compareBy = compareBy;
        }

        public override bool Equals(JsonProperty x, JsonProperty y)
            => compareBy switch
            {
                JsonPropertyCompareBy.PropertyName => StringComparer.Ordinal.Compare(x?.PropertyName, y?.PropertyName) == 0,
                _ => throw new NotImplementedException()
            };

        public override int GetHashCode(JsonProperty obj)
            => compareBy switch
            {
                JsonPropertyCompareBy.PropertyName => obj.PropertyName.GetHashCode(),
                _ => throw new NotImplementedException()
            };

        private enum JsonPropertyCompareBy
        {
            PropertyName
        }
    }
}
