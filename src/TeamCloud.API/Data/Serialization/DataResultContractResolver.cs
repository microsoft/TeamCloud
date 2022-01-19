/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Serialization.Encryption;
using TeamCloud.Serialization.Resolver;

namespace TeamCloud.API.Data.Serialization;

public class DataResultContractResolver : SuppressConverterContractResolver<DataResultConverter>
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        foreach (var property in properties.Where(p => p.ValueProvider is EncryptedValueProvider))
        {
            property.ShouldSerialize = (obj) =>
            {
                return false;
            };
        }

        return properties;
    }
}
