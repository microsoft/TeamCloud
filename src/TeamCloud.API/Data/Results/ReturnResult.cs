/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data.Results
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public interface IReturnResult
    {
        int Code { get; }

        string Status { get; }
    }

    public interface ISuccessResult : IReturnResult
    {
        string Location { get; }
    }

    public interface IFailureResult : IReturnResult
    {
        IList<ResultError> Errors { get; }
    }
}
