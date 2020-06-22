/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public class AzureKeyVault : IEquatable<AzureKeyVault>
    {
        public string VaultId { get; set; }

        public string VaultName { get; set; }

        public string VaultUrl { get; set; }


        public bool Equals(AzureKeyVault other)
            => VaultId?.Equals(other?.VaultId, StringComparison.OrdinalIgnoreCase) ?? false;

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as AzureKeyVault);

        public override int GetHashCode()
            => VaultId?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? base.GetHashCode();
    }
}
