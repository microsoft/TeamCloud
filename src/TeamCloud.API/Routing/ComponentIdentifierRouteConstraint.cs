/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TeamCloud.API.Routing
{
    public class ComponentIdentifierRouteConstraint : IRouteConstraint
    {
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
                return routeValueString.IsGuid() || !string.IsNullOrWhiteSpace(routeValueString);
            }

            return false;
        }
    }
}
