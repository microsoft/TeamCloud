/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Serialization.Resolver;

namespace TeamCloud.Model.Data.Serialization;

internal class ReferenceLinksContainerContractResolver : SuppressConverterContractResolver<ReferenceLinksContainerConverter>
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);

        // disable read/write for this property if the property this is not equal to ReferenceLink
        prop.Readable = prop.Writable = (member is PropertyInfo propertyInfo && propertyInfo.PropertyType == typeof(ReferenceLink));

        if (prop.Readable)
        {
            // only serialize ReferenceLink instances where the HRef returns a value other than NULL and EMPTY
            prop.ShouldSerialize = (instance)
                => prop.ValueProvider.GetValue(instance) is ReferenceLink referenceLink && !string.IsNullOrEmpty(referenceLink.HRef);
        }

        return prop;
    }
}
