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
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Model.Internal.Commands;

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

        public static string GetObjectId(this ClaimsPrincipal claimsPrincipal)
        {
            const string ObjectIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            var objectIdenifier = claimsPrincipal.FindFirstValue(ObjectIdentifierClaimType);

            return objectIdenifier;
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

        public static bool IsUserIdentifier(this string identifier)
            => !string.IsNullOrEmpty(identifier)
            && ((Guid.TryParse(identifier, out var outGuid) && !outGuid.Equals(Guid.Empty))
                || new EmailAddressAttribute().IsValid(identifier)
                || new UrlAttribute().IsValid(identifier));

        public static async Task<IActionResult> InvokeAndReturnAccepted(this Orchestrator orchestrator, IOrchestratorCommand command)
        {
            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var url) || commandResult.Links.TryGetValue("location", out url))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), url, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception($"We tried to retrun an Accepted (202) result but the Orchestrator service didn't return a status url or a location url for the Orchestrator command of type '{command.GetType().Name}'. This shouldn't happen, but we need to decide to do when it does.");
        }

        public static bool RequiresAdminUserSet(this HttpRequest httpRequest)
        {
            if (httpRequest.IsAdminUserPost() || httpRequest.IsSwaggerGet())
                return false;

            return true;
        }

        public static bool IsSwaggerGet(this HttpRequest httpRequest)
            => httpRequest.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) && HttpMethods.IsGet(httpRequest.Method);

        public static bool IsAdminUserPost(this HttpRequest httpRequest)
            => httpRequest.Path.StartsWithSegments("/api/admin/users", StringComparison.OrdinalIgnoreCase) && HttpMethods.IsPost(httpRequest.Method);
    }
}
