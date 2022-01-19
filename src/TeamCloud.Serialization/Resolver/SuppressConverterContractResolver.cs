/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace TeamCloud.Serialization.Resolver;

public class SuppressConverterContractResolver<TConverter> : TeamCloudContractResolver
    where TConverter : JsonConverter
{
    protected override JsonConverter ResolveContractConverter(Type objectType)
    {
        var jsonConverter = base.ResolveContractConverter(objectType);

        if (jsonConverter is TConverter)
        {
            Debug.WriteLine($"Suppressing JsonContractResolver of type {jsonConverter.GetType()} for object of type {objectType}");

            return null;
        }

        return jsonConverter;
    }
}
