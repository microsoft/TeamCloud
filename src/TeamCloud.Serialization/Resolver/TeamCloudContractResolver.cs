/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TeamCloud.Serialization.Converter;

namespace TeamCloud.Serialization.Resolver
{
    public class TeamCloudContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType is null)
                throw new ArgumentNullException(nameof(objectType));

            if (typeof(Exception).IsAssignableFrom(objectType))
                return new ExceptionConverter();

            if (objectType.IsEnum)
                return new StringEnumConverter();

            return base.ResolveContractConverter(objectType);
        }

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
                    bool shouldSerializeEnumerable(object obj)
                    {
                        var enumerable = prop.ValueProvider.GetValue(obj) as IEnumerable;
                        return enumerable?.GetEnumerator().MoveNext() ?? false;
                    }

                    prop.ShouldSerialize = prop.ShouldSerialize == null
                        ? (Predicate<object>)shouldSerializeEnumerable
                        : obj => prop.ShouldSerialize(obj) && shouldSerializeEnumerable(obj);
                }
            }

            return prop;
        }
    }
}
