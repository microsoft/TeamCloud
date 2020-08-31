/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProviderDocument : ContainerDocument, IProvider, IEquatable<ProviderDocument>, IPopulate<Model.Data.Provider>
    {
        [PartitionKey]
        public string Tenant { get; set; }

        public string Url { get; set; }

        public string AuthCode { get; set; }

        public Guid? PrincipalId { get; set; }

        public string Version { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public IList<string> Events { get; set; }
            = new List<string>();

        public IList<ProviderEventSubscription> EventSubscriptions { get; set; }
            = new List<ProviderEventSubscription>();

        public IDictionary<string, string> Properties { get; set; }
            = new Dictionary<string, string>();

        public DateTime? Registered { get; set; }

        public ProviderCommandMode CommandMode { get; set; }
            = ProviderCommandMode.Simple;

        public IList<string> ResourceProviders { get; set; }
            = new List<string>();

        public bool Equals(ProviderDocument other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProviderDocument);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
