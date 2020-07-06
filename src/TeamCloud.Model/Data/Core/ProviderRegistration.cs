/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class ProviderRegistration
    {
        public Guid? PrincipalId { get; set; }

        public ProviderCommandMode CommandMode { get; set; } = ProviderCommandMode.Simple;

        public IList<string> ResourceProviders { get; set; } = new List<string>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
