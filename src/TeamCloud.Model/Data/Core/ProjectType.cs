/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public interface IProjectType : ITags, IProperties
    {
        string Id { get; set; }

        bool Default { get; set; }

        string Region { get; set; }

        int SubscriptionCapacity { get; set; }

        string ResourceGroupNamePrefix { get; set; }

        IList<ProviderReference> Providers { get; set; }
    }
}
