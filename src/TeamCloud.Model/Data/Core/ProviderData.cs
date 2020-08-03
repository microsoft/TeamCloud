/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public interface IProviderData
    {
        // string ProviderId { get; set; }

        // string ProjectId { get; set; }

        string Id { get; set; }

        string Name { get; set; }

        object Value { get; set; }

        string Location { get; set; }

        bool IsSecret { get; set; }

        bool IsShared { get; set; }

        ProviderDataScope Scope { get; set; }

        ProviderDataType DataType { get; set; }
    }

    public enum ProviderDataType
    {
        Property,
        Service
    }

    public enum ProviderDataScope
    {
        System,
        Project
    }
}
