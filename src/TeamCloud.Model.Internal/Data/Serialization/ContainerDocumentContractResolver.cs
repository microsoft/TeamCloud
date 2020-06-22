/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Internal.Data.Core;
using TeamCloud.Serialization.Comparer;
using TeamCloud.Serialization.Resolver;

namespace TeamCloud.Model.Internal.Data.Serialization
{
    internal class ContainerDocumentContractResolver : SuppressContractResolver<ContainerDocumentConverter>
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            if (typeof(IContainerDocument).IsAssignableFrom(type))
            {
                var metaProperties = base
                    .CreateProperties(typeof(IContainerDocument), memberSerialization)
                    .Except(properties, JsonPropertyEqualityComparer.ByPropertyName);

                properties = properties
                    .Union(metaProperties)
                    .ToList();
            }

            return properties;
        }
    }
}
