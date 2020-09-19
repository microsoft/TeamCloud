using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Serialization
{
    public sealed class TeamCloudSerializerSettings : JsonSerializerSettings
    {
        public static TeamCloudSerializerSettings Default => Create(new TeamCloudContractResolver());

        public static TeamCloudSerializerSettings Create(IContractResolver contractResolver) => new TeamCloudSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = contractResolver ?? throw new ArgumentNullException(nameof(contractResolver))
        };
    }
}
