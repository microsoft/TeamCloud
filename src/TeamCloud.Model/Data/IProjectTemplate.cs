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
    public interface IProjectTemplate : IIdentifiable, IDisplayName //, ITags, IProperties
    {
        List<string> Components { get; set; }

        RepositoryReference Repository { get; set; }

        string Description { get; set; }

        bool IsDefault { get; set; }

        // string InputJson { get; set; }

        string InputJsonSchema { get; set; }
    }
}
