/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;

namespace TeamCloud.Model.Commands.Serialization
{
    public class SuppressConverterContractResolver<TConverter> : ModelConverterContractResolver
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
