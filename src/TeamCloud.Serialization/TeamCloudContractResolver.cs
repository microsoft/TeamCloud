/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TeamCloud.Serialization.Compress;
using TeamCloud.Serialization.Converter;
using TeamCloud.Serialization.Encryption;

namespace TeamCloud.Serialization;

public class TeamCloudContractResolver : CamelCasePropertyNamesContractResolver
{
    private readonly IDataProtectionProvider dataProtectionProvider;
    private readonly IEnumerable<JsonConverter> converters;

    public TeamCloudContractResolver(IDataProtectionProvider dataProtectionProvider = null, IEnumerable<JsonConverter> converters = null)
    {
        // prevent changing the case of dictionary keys
        NamingStrategy = new TeamCloudNamingStrategy();

        this.dataProtectionProvider = dataProtectionProvider;
        this.converters = converters ?? Enumerable.Empty<JsonConverter>();
    }


    public override JsonContract ResolveContract(Type objectType)
    {
        var contract = converters.Any()
            ? CreateContract(objectType)
            : base.ResolveContract(objectType);

        return contract;
    }

    protected override JsonConverter ResolveContractConverter(Type objectType)
    {
        if (objectType is null)
            throw new ArgumentNullException(nameof(objectType));

        var converter = converters.FirstOrDefault(c => c.CanConvert(objectType));

        if (converter is not null)
        {
            return converter;
        }
        else if (typeof(Exception).IsAssignableFrom(objectType))
        {
            converter = new ExceptionConverter();
        }
        else if (typeof(NameValueCollection).IsAssignableFrom(objectType))
        {
            converter = new NameValueCollectionConverter();
        }
        else if (objectType.IsEnum)
        {
            converter = new StringEnumConverter();
        }
        else
        {
            converter = base.ResolveContractConverter(objectType);
        }

        return converter;
    }

    protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
    {
        if (member is null)
            throw new ArgumentNullException(nameof(member));

        var valueProvider = base.CreateMemberValueProvider(member);

        if (member.GetCustomAttribute<EncryptedAttribute>() is not null)
            return new EncryptedValueProvider(member, valueProvider, dataProtectionProvider);
        else if (member.GetCustomAttribute<CompressAttribute>() is not null)
            return new CompressValueProvider(member, valueProvider);

        return valueProvider; // we stick with the default value provider
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);

        if (member is PropertyInfo propertyInfo && !prop.Writable)
        {
            // enable private property setter deserialization for types with default constructor
            prop.Writable = propertyInfo.GetSetMethod(true) is not null;
        }

        return prop;
    }

    protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
    {
        return base.CreateDictionaryContract(objectType);
    }
}
