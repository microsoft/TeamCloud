/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data
{
    public interface IDataResult : IReturnResult
    {

    }


    public interface IDataResult<T> : IReturnResult
        where T : new()
    {
        T Data { get; }
    }

    public class DataResult<T> : IDataResult<T>, IDataResult
        where T : new()
    {
        public T Data { get; private set; }

        [JsonProperty(Order = int.MinValue)]
        public int Code { get; private set; }

        [JsonProperty(Order = int.MinValue)]
        public string Status { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ResultError> Errors { get; set; }

        private DataResult() { }

        private DataResult(T data)
            => Data = data;

        public static DataResult<T> Ok(T data)
            => new DataResult<T>(data) { Code = 200, Status = "Ok" };
    }

    public static class DataResultExtensions
    {
        public static IActionResult ActionResult(this IDataResult result) => (result.Code) switch
        {
            200 => new OkObjectResult(result),
            _ => throw new NotImplementedException()
        };
    }
}
