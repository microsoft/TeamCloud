/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace TeamCloud.API.Data.Results
{
    public static class StatusResultExtensions
    {
        public static IActionResult ToActionResult(this IStatusResult result) => (result?.Code) switch
        {
            StatusCodes.Status200OK
                => new StatusObjectResult(StatusCodes.Status200OK, result),

            StatusCodes.Status202Accepted
                => new StatusObjectResult(StatusCodes.Status202Accepted, result, result.Location),

            StatusCodes.Status302Found
                => new StatusObjectResult(StatusCodes.Status302Found, result, result.Location),

            _ => throw new NotImplementedException()
        };

        private sealed class StatusObjectResult : ObjectResult
        {
            public StatusObjectResult(int statusCode, object value, string location = null) : base(value)
            {
                StatusCode = statusCode;
                Location = location;
            }

            public string Location { get; }

            public override void OnFormatting(ActionContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                base.OnFormatting(context);

                if (!string.IsNullOrEmpty(Location))
                {
                    context.HttpContext.Response.Headers[HeaderNames.Location] = Location;
                }
            }
        }
    }
}
