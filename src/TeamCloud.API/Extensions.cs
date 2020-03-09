/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.API
{
    internal static class Extensions
    {
        public static Uri GetApplicationBaseUrl(this HttpContext httpContext)
        {
            var uriBuilder = new UriBuilder()
            {
                Scheme = httpContext.Request.Scheme,
                Host = httpContext.Request.Host.Host,
                Port = httpContext.Request.Host.Port ?? -1,
                Path = httpContext.Request.PathBase
            };

            return uriBuilder.Uri;
        }

        public static T GetValueOrDefault<T>(this IReadOnlyDictionary<string, T> dictionary, string key, StringComparison comparsion)
        {
            return dictionary.GetValueOrDefault(key, default, comparsion);
        }

        public static T GetValueOrDefault<T>(this IReadOnlyDictionary<string, T> dictionary, string key, T defaultValue, StringComparison comparsion)
        {
            var result = dictionary.SingleOrDefault(kvp => kvp.Key.Equals(key, comparsion));

            return result.Key is null ? defaultValue : result.Value;
        }

        public static Guid GetObjectId(this ClaimsPrincipal claimsPrincipal)
        {
            const string ObjectIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            var objectIdenifier = claimsPrincipal.FindFirstValue(ObjectIdentifierClaimType);

            return Guid.Parse(objectIdenifier);
        }

        public static Task<T> ReadAsAsync<T>(this HttpContent httpContent, JsonSerializerSettings serializerSettings = null)
            => httpContent.ReadAsAsync<T>(JsonSerializer.CreateDefault(serializerSettings));

        public static async Task<T> ReadAsAsync<T>(this HttpContent httpContent, JsonSerializer serializer)
        {
            using var stream = await httpContent.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);

            return serializer.Deserialize<T>(jsonReader);
        }

        public static bool IsGuid(this string value)
            => Guid.TryParse(value, out var _);

        public static bool IsEMail(this string value)
            => new EmailAddressAttribute().IsValid(value);

        public static IActionResult ActionResult(this ICommandResult result)
            => result.Links.TryGetValue("status", out var statusUrl)
               ? new AcceptedResult(statusUrl, result)
               : new OkObjectResult(result) as IActionResult;

        public static IActionResult StatusResult(this ICommandResult result)
            => !result.RuntimeStatus.IsFinal() && result.Links.TryGetValue("status", out var statusUrl)
               ? new AcceptedResult(statusUrl, result)
               : new OkObjectResult(result) as IActionResult;
    }
}
