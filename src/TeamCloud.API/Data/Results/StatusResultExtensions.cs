/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TeamCloud.API.Data.Results
{
    public static class StatusResultExtensions
    {
        public static IActionResult ActionResult(this IStatusResult result) => (result?.Code) switch
        {
            StatusCodes.Status200OK => new OkObjectResult(result),
            StatusCodes.Status202Accepted => new AcceptedResult(result.Location, result),
            StatusCodes.Status302Found => new JsonResult(result) { StatusCode = StatusCodes.Status302Found },
            _ => throw new NotImplementedException()
        };
    }
}
