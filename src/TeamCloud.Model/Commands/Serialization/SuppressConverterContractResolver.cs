/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace TeamCloud.Model
{
    public class SuppressConverterContractResolver<TConverter> : DefaultContractResolver
        where TConverter : JsonConverter
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            var jsonConverter = base.ResolveContractConverter(objectType);

            if (jsonConverter is TConverter) return null;

            return jsonConverter;
        }
    }
}
