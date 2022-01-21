/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TeamCloud.API.Data.Results;

namespace TeamCloud.API.Middleware;

public class ClientErrorFactory : IClientErrorFactory
{
    public IActionResult GetClientError(ActionContext actionContext, IClientErrorActionResult clientError)
    {
        if (clientError is null)
            throw new System.ArgumentNullException(nameof(clientError));

        if (clientError.StatusCode.HasValue)
        {
            switch (clientError.StatusCode.Value)
            {
                case StatusCodes.Status400BadRequest:
                    return ErrorResult.BadRequest().ToActionResult();
                case StatusCodes.Status401Unauthorized:
                    return ErrorResult.Unauthorized().ToActionResult();
                case StatusCodes.Status403Forbidden:
                    return ErrorResult.Forbidden().ToActionResult();
                case StatusCodes.Status404NotFound:
                    return ErrorResult.NotFound("Not Found").ToActionResult();
                case StatusCodes.Status409Conflict:
                    return ErrorResult.Conflict("Conflict").ToActionResult();
                case StatusCodes.Status500InternalServerError:
                    return ErrorResult.ServerError().ToActionResult();
            }
        }

        return ErrorResult.Unknown(clientError.StatusCode).ToActionResult();
    }
}
