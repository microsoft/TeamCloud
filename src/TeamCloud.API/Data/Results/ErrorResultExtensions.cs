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
    public static class ErrorResultExtensions
    {
        public static IActionResult ToActionResult(this IErrorResult result) => result?.Code switch
        {
            StatusCodes.Status400BadRequest => new BadRequestObjectResult(result),
            StatusCodes.Status401Unauthorized => new JsonResult(result) { StatusCode = StatusCodes.Status401Unauthorized },
            StatusCodes.Status403Forbidden => new JsonResult(result) { StatusCode = StatusCodes.Status403Forbidden },
            StatusCodes.Status404NotFound => new NotFoundObjectResult(result),
            StatusCodes.Status409Conflict => new ConflictObjectResult(result),
            StatusCodes.Status500InternalServerError => new JsonResult(result) { StatusCode = StatusCodes.Status500InternalServerError },
            _ => throw new NotImplementedException()
        };

        public static Task<IActionResult> ToActionResultAsync(this IErrorResult result)
            => Task.FromResult(result.ToActionResult());
    }
}
