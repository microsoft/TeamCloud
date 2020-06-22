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
    public interface IProvider : IProperties
    {
        string Id { get; set; }

        string AuthCode { get; set; }

        Guid? PrincipalId { get; set; }

        AzureResourceGroup ResourceGroup { get; set; }

        IList<string> Events { get; set; }

        DateTime? Registered { get; set; }

        ProviderCommandMode CommandMode { get; set; }
    }
}
