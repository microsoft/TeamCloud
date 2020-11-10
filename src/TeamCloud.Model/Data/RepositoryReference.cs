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
        // public string Id => $"{Organization}.{Project}.{Repository}.{Version}".Replace("..", ".", StringComparison.Ordinal);

        public string Url { get; set; }

        public string Token { get; set; }

        public string Version { get; set; }

        public string BaselUrl { get; set; }

        public string Ref { get; set; }

        public RepositoryProvider Provider { get; set; }

        public RepositoryReferenceType Type { get; set; }

        public string Organization { get; set; }

        public string Repository { get; set; }

        public string Project { get; set; }
    }
}
