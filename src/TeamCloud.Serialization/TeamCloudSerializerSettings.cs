/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization;

public sealed class TeamCloudSerializerSettings : JsonSerializerSettings
{
    public static readonly TeamCloudSerializerSettings Default = new();

    public static TeamCloudSerializerSettings Create<TContractResolver>()
        where TContractResolver : class, IContractResolver, new()
        => new(Activator.CreateInstance<TContractResolver>());

    public TeamCloudSerializerSettings(IContractResolver contractResolver) : this()
    {
        ContractResolver = contractResolver ?? throw new ArgumentNullException(nameof(contractResolver));
    }

    public TeamCloudSerializerSettings(JsonConverter converter, params JsonConverter[] additionalConverters) : this()
    {
        Converters = additionalConverters.Prepend(converter ?? throw new ArgumentNullException(nameof(converter))).ToList();
        ContractResolver = new TeamCloudContractResolver(converters: Converters);
    }

    public TeamCloudSerializerSettings()
    {
        TraceWriter = new TeamCloudSerializerTraceWriter();
        TypeNameHandling = TypeNameHandling.Auto;
        NullValueHandling = NullValueHandling.Ignore;
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        ContractResolver = new TeamCloudContractResolver();
    }
}
