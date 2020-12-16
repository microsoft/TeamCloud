/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TeamCloud.API.Data.Results
{
    public static class DataResultExtensions
    {
        public static IActionResult ToActionResult(this IDataResult result) => result?.Code switch
        {
            StatusCodes.Status200OK => new OkObjectResult(result),
            StatusCodes.Status201Created => new CreatedResult(result.Location, result),
            StatusCodes.Status204NoContent => new NoContentResult(),
            _ => throw new NotImplementedException()
        };

        public static Task<IActionResult> ToActionResultAsync(this IDataResult result)
            => Task.FromResult(result.ToActionResult());
    }
}
