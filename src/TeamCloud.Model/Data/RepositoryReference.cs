/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class RepositoryReference
    {
        public string Url { get; set; }

        public string Token { get; set; }

        public string Version { get; set; }

        public string Ref { get; set; }
    }
}
