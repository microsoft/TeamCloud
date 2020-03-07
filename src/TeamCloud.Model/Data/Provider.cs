/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class Provider
    {
        public string Id { get; set; }

        public string Url { get; set; }

        public string AuthCode { get; set; }

        public Guid? PrincipalId { get; set; }

        public ProviderDependencies Dependencies { get; set; } = new ProviderDependencies();

        public IList<string> Events { get; set; } = new List<string>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public DateTime? Registered { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class ProviderDependencies
    {
        public IList<string> Create { get; set; } = new List<string>();

        public IList<string> Init { get; set; } = new List<string>();
    }
}
