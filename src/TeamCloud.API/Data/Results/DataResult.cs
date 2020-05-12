/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.API.Data.Results
{
    public interface IDataResult : ISuccessResult
    { }


    public interface IDataResult<T> : IDataResult
        where T : new()
    {
        T Data { get; }
    }

    public class DataResult<T> : IDataResult<T>
        where T : new()
    {
        public T Data { get; private set; }

        [JsonProperty(Order = int.MinValue)]
        public int Code { get; private set; }

        [JsonProperty(Order = int.MinValue)]
        public string Status { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Location { get; private set; }

        private DataResult() { }

        private DataResult(T data)
            => Data = data;

#pragma warning disable CA1000 // Do not declare static members on generic types

        public static DataResult<T> Ok(T data)
            => new DataResult<T>(data) { Code = StatusCodes.Status200OK, Status = "Ok" };

        public static DataResult<T> Created(T data, string location)
            => new DataResult<T>(data) { Code = StatusCodes.Status201Created, Status = "Created", Location = location };

        public static DataResult<T> NoContent()
            => new DataResult<T> { Code = StatusCodes.Status204NoContent, Status = "NoContent" };

#pragma warning restore CA1000 // Do not declare static members on generic types
    }

    public static class DataResultExtensions
    {
        public static IActionResult ActionResult(this IDataResult result) => result?.Code switch
        {
            StatusCodes.Status200OK => new OkObjectResult(result),
            StatusCodes.Status201Created => new CreatedResult(result.Location, result),
            StatusCodes.Status204NoContent => new NoContentResult(),
            _ => throw new NotImplementedException()
        };
    }
}
