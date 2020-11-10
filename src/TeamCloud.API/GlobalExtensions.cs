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
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Initialization;
using TeamCloud.API.Services;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.API
{
    internal static class GlobalExtensions
    {
        internal static async IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IEnumerable<Task<TSource>> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var tasks = source.ToList();

            while (tasks.Any())
            {
                var result = await Task
                    .WhenAny(tasks)
                    .ConfigureAwait(false);

                tasks.Remove(result);

                yield return await result.ConfigureAwait(false);
            }
        }

        internal static async Task<T> InitializeAsync<T>(this T host)
            where T : IHost
        {
            using var scope = host.Services.CreateScope();

            var tasks = scope.ServiceProvider
                .GetServices<IHostInitializer>()
                .Select(initializer => initializer.InitializeAsync());

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);

            return host;
        }

        public static Uri GetApplicationBaseUrl(this HttpContext httpContext, bool enforceHttps = false)
        {
            var uriBuilder = new UriBuilder()
            {
                Scheme = httpContext.Request.Scheme,
                Host = httpContext.Request.Host.Host,
                Port = httpContext.Request.Host.Port ?? -1,
                Path = httpContext.Request.PathBase
            };

            return enforceHttps ? uriBuilder.Uri.EnforceHttps() : uriBuilder.Uri;
        }

        public static Uri EnforceHttps(this Uri url, int? port = null)
        {
            if (url is null)
            {
                throw new ArgumentNullException(nameof(url));
            }
            else if (!url.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                  && !url.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Urls with scheme '{url.Scheme}' are not supported.", nameof(url));
            }

            var uriBuilder = new UriBuilder(url);

            if (uriBuilder.Uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                uriBuilder.Port = port.GetValueOrDefault(uriBuilder.Port);
            }
            else
            {
                uriBuilder.Scheme = Uri.UriSchemeHttps;
                uriBuilder.Port = port.GetValueOrDefault(-1);
            }

            return uriBuilder.Uri;
        }

        public static T GetValueOrDefault<T>(this IReadOnlyDictionary<string, T> dictionary, string key, StringComparison comparsion)
         => dictionary.GetValueOrDefault(key, default, comparsion);

        public static T GetValueOrDefault<T>(this IReadOnlyDictionary<string, T> dictionary, string key, T defaultValue, StringComparison comparsion)
        {
            var result = dictionary.SingleOrDefault(kvp => kvp.Key.Equals(key, comparsion));

            return result.Key is null ? defaultValue : result.Value;
        }

        public static bool RequestPathStartsWithSegments(this HttpContext httpContext, PathString other, bool ignoreCase = true)
            => httpContext.Request.Path.StartsWithSegments(other, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        public static bool RequestPathEndsWith(this HttpContext httpContext, string value, bool ignoreCase = true)
            => httpContext.Request.Path.Value.EndsWith(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        public static string RouteValueOrDefault(this HttpContext httpContext, string key, bool ignoreCase = true)
            => httpContext.GetRouteData().ValueOrDefault(key, ignoreCase);

        public static string ValueOrDefault(this RouteData routeData, string key, bool ignoreCase = true)
            => routeData.Values.GetValueOrDefault(key, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)?.ToString();

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
            => !string.IsNullOrWhiteSpace(identifier);

        public static Task<IActionResult> InvokeAndReturnActionResultAsync<TData>(this Orchestrator orchestrator, IOrchestratorCommand<TData> command, HttpRequest httpRequest)
            where TData : class, new()
            => InvokeAndReturnActionResultAsync(orchestrator, command, new HttpMethod(httpRequest?.Method ?? throw new ArgumentNullException(nameof(httpRequest))));

        public static async Task<IActionResult> InvokeAndReturnActionResultAsync<TData>(this Orchestrator orchestrator, IOrchestratorCommand<TData> command, HttpMethod httpMethod)
            where TData : class, new()
        {
            if (orchestrator is null)
                throw new ArgumentNullException(nameof(orchestrator));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (httpMethod is null)
                throw new ArgumentNullException(nameof(httpMethod));

            var commandResult = (ICommandResult<TData>)await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            return commandResult.ToActionResult<TData>(httpMethod);
        }

        public static IActionResult ToActionResult<TData>(this ICommandResult<TData> commandResult, HttpRequest httpRequest)
            where TData : class, new()
            => ToActionResult(commandResult, new HttpMethod(httpRequest?.Method ?? throw new ArgumentNullException(nameof(httpRequest))));

        public static IActionResult ToActionResult<TData>(this ICommandResult<TData> commandResult, HttpMethod httpMethod)
            where TData : class, new()
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (httpMethod is null)
                throw new ArgumentNullException(nameof(httpMethod));

            if (commandResult.RuntimeStatus.IsActive())
            {
                return commandResult.ToAcceptedResult();
            }
            else if (commandResult.RuntimeStatus == CommandRuntimeStatus.Completed)
            {
                return commandResult.ToDataResult(httpMethod);
            }
            else
            {
                return ErrorResult.ServerError(commandResult.Errors).ToActionResult();
            }
        }

        public static IActionResult ToDataResult<TData>(this ICommandResult<TData> commandResult, HttpRequest httpRequest)
            where TData : class, new()
            => ToDataResult(commandResult, new HttpMethod(httpRequest?.Method ?? throw new ArgumentNullException(nameof(httpRequest))));

        public static IActionResult ToDataResult<TData>(this ICommandResult<TData> commandResult, HttpMethod httpMethod)
            where TData : class, new()
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (httpMethod == HttpMethod.Delete)
            {
                return DataResult<TData>.NoContent().ToActionResult();
            }

            if (httpMethod == HttpMethod.Post)
            {
                if (!commandResult.Links.TryGetValue("location", out var location))
                    throw new NotSupportedException("Missing location link in command result.");

                return DataResult<TData>.Created(commandResult.Result, location).ToActionResult();
            }
            else if (httpMethod == HttpMethod.Put)
            {
                return DataResult<TData>.Ok(commandResult.Result).ToActionResult();
            }
            else
            {
                throw new NotSupportedException($"HTTP verb {httpMethod} is not supported.");
            }
        }

        public static IActionResult ToAcceptedResult(this ICommandResult commandResult)
        {
            if (commandResult is null)
                throw new ArgumentNullException(nameof(commandResult));

            if (commandResult.RuntimeStatus.IsActive())
            {
                if (commandResult.Links.TryGetValue("status", out var url) || commandResult.Links.TryGetValue("location", out url))
                {
                    return StatusResult
                        .Accepted(commandResult.CommandId.ToString(), url, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                        .ToActionResult();
                }

                throw new ArgumentException($"Command result of type '{commandResult.GetType().Name}' does not provide a status or location url.", nameof(commandResult));
            }

            throw new NotSupportedException("None active runtime states are not supported");
        }

        public static bool RequiresAdminUserSet(this HttpRequest httpRequest)
        {
            if (httpRequest.IsAdminUserPost() || httpRequest.IsSwaggerGet() || httpRequest.IsAdminTeamCloudInstancePost())
                return false;

            return true;
        }

        public static bool IsSwaggerGet(this HttpRequest httpRequest)
            => httpRequest.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase) && HttpMethods.IsGet(httpRequest.Method);

        public static bool IsAdminUserPost(this HttpRequest httpRequest)
            => httpRequest.Path.StartsWithSegments("/api/admin/users", StringComparison.OrdinalIgnoreCase) && HttpMethods.IsPost(httpRequest.Method);

        public static bool IsAdminTeamCloudInstancePost(this HttpRequest httpRequest)
            => httpRequest.Path.StartsWithSegments("/api/admin/teamCloudInstance", StringComparison.OrdinalIgnoreCase) && HttpMethods.IsPost(httpRequest.Method);
    }
}
