/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TeamCloud.API.Routing;

public class ProviderIdentifierRouteConstraint : IRouteConstraint
{
    private static readonly Regex validProviderOrProjectTypeId = new Regex(@"^(?:[a-z][a-z0-9]+(?:\.?[a-z0-9]+)+)$");

    public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (httpContext is null)
            throw new ArgumentNullException(nameof(httpContext));

        if (route is null)
            throw new ArgumentNullException(nameof(route));

        if (routeKey is null)
            throw new ArgumentNullException(nameof(routeKey));

        if (values is null)
            throw new ArgumentNullException(nameof(values));

        if (values.TryGetValue(routeKey, out var routeValue))
        {
            var routeValueString = Convert.ToString(routeValue, CultureInfo.InvariantCulture);
            return !string.IsNullOrEmpty(routeValueString)
                && routeValueString.Length > 4
                && routeValueString.Length < 255
                && validProviderOrProjectTypeId.IsMatch(routeValueString);
        }

        return false;
    }
}
