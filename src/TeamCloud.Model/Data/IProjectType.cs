/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Common;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public interface IProjectType : ITags, IProperties
    {
        string Id { get; set; }

        bool IsDefault { get; set; }

        string Region { get; set; }

        int SubscriptionCapacity { get; set; }

        string ResourceGroupNamePrefix { get; set; }

        IList<ProviderReference> Providers { get; set; }
    }
}
