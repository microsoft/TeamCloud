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
    public interface IIdentifiable
    {
        Guid Id { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public interface IProperties
    {
        IDictionary<string, string> Properties { get; set; }
    }


    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public interface ITags
    {
        IDictionary<string, string> Tags { get; set; }
    }



}
