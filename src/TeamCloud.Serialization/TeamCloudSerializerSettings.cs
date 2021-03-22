/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace TeamCloud.Serialization
{
    public sealed class TeamCloudSerializerSettings : JsonSerializerSettings
    {
        public static readonly TeamCloudSerializerSettings Default = Create();

        public static TeamCloudSerializerSettings Create(IContractResolver contractResolver = null)
            => new TeamCloudSerializerSettings(contractResolver ?? new TeamCloudContractResolver());

        public static TeamCloudSerializerSettings Create<T>()
            where T : class, IContractResolver, new()
            => Create(Activator.CreateInstance<T>());

        private TeamCloudSerializerSettings(IContractResolver contractResolver)
        {
            TraceWriter = new TeamCloudSerializerTraceWriter();
            TypeNameHandling = TypeNameHandling.Auto;
            NullValueHandling = NullValueHandling.Ignore;
            ContractResolver = contractResolver ?? throw new ArgumentNullException(nameof(contractResolver));
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }
    }
}
