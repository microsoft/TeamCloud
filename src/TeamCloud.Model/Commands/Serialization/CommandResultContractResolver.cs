/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Commands.Serialization
{
    class CommandResultContractResolver : SuppressConverterContractResolver<CommandResultConverter>
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo propertyInfo)
            {
                if (!prop.Writable)
                {
                    // enable private property setter deserialization
                    prop.Writable = propertyInfo.GetSetMethod(true) != null;
                }

                // suppress serialization of empty enumerations
                if (propertyInfo.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    Predicate<object> shouldSerializeEnumerable = obj =>
                    {

                        var enumerable = prop.ValueProvider.GetValue(obj) as IEnumerable;
                        var serialize = enumerable?.GetEnumerator().MoveNext() ?? false;

                        if (!serialize) Debug.WriteLine($"Suppress serialization for {prop.PropertyName} of type {prop.PropertyType.Name}");
                        return serialize;
                    };

                    prop.ShouldSerialize = prop.ShouldSerialize == null
                        ? shouldSerializeEnumerable
                        : obj => prop.ShouldSerialize(obj) && shouldSerializeEnumerable(obj);
                }
            }

            return prop;
        }
    }
}
