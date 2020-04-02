/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using TeamCloud.Azure;
using TeamCloud.Http;

namespace TeamCloud.Orchestration
{
    public static class FunctionEnvironment
    {
        private static readonly ConcurrentDictionary<string, MethodInfo> FunctionMethodCache = new ConcurrentDictionary<string, MethodInfo>();

        public static MethodInfo GetFunctionMethod(string functionName) => FunctionMethodCache.GetOrAdd(functionName, (key) =>
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.IsDynamic)
                .SelectMany(asm => asm.GetExportedTypes().Where(type => type.IsClass))
                .SelectMany(type => type.GetMethods())
                .FirstOrDefault(method => method.GetCustomAttribute<FunctionNameAttribute>()?.Name.Equals(functionName, StringComparison.Ordinal) ?? false);

        }) ?? throw new ArgumentOutOfRangeException(nameof(functionName), $"Could not find function by name '{functionName}'");

        public static bool TryGetFunctionMethod(string functionName, out MethodInfo functionMethod)
        {
            try
            {
                functionMethod = GetFunctionMethod(functionName);
                return true;
            }
            catch
            {
                functionMethod = null;
                return false;
            }
        }

        public static bool FunctionExists(string functionName)
        {
            try
            {
                return (GetFunctionMethod(functionName) != null);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public static string HostUrl
        {
            get
            {
                var hostScheme = "http";
                var hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

                if (!hostName.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
                    hostScheme += "s";

                return $"{hostScheme}://{hostName}";
            }
        }

        private static async Task<JObject> GetKeyJsonAsync()
        {
            var token = await AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            if (jwtToken.Payload.TryGetValue("xms_mirid", out var value))
            {
                var response = await "https://management.azure.com"
                    .AppendPathSegment(value)
                    .AppendPathSegment("/host/default/listKeys")
                    .SetQueryParam("api-version", "2018-11-01")
                    .WithOAuthBearerToken(token)
                    .PostAsync(null)
                    .ConfigureAwait(false);

                return await response
                    .ReadAsJsonAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                throw new NotSupportedException($"The acquired token does not contain any resource id information.");
            }
        }

        public static async Task<string> GetMasterKeyAsync()
        {
            var json = await GetKeyJsonAsync().ConfigureAwait(false);

            return json.SelectToken("$.masterKey")?.ToString();
        }

        public static async Task<string> GetHostKeyAsync(string hostKeyName = default)
        {
            var json = await GetKeyJsonAsync().ConfigureAwait(false);

            return json.SelectToken($"$.functionKeys['{hostKeyName ?? "default"}']")?.ToString();
        }

    }
}
