/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class ProviderRegistration
    {
        public Guid? PrincipalId { get; set; }

        public IEnumerable<ProviderEventSubscription> EventSubscriptions { get; set; }
            = Enumerable.Empty<ProviderEventSubscription>();

        public ProviderCommandMode CommandMode { get; set; }
            = ProviderCommandMode.Simple;

        public IEnumerable<string> ResourceProviders { get; set; }
            = Enumerable.Empty<string>();

        public IDictionary<string, string> Properties { get; set; }
            = new Dictionary<string, string>();
    }
}
