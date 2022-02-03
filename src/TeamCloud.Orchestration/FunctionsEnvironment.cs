/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using TeamCloud.Azure;
using TeamCloud.Http;

namespace TeamCloud.Orchestration;

public static class FunctionsEnvironment
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
        => TryGetFunctionMethod(functionName, out var _);

    public static bool IsAzureEnvironment
        => !IsLocalEnvironment;

    public static bool IsLocalEnvironment
        => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

    public static async Task<string> GetFunctionUrlAsync(string functionName, IFunctionsHost functionsHost = null, bool withCode = false, Func<string, string> replaceToken = null)
    {
        var functionJson = await GetFunctionJsonAsync(functionName)
            .ConfigureAwait(false);

        var functionUrl = functionJson?
            .SelectToken("$.invoke_url_template")?
            .ToString();

        if (string.IsNullOrEmpty(functionUrl))
            return null;

        var hostUrl = (functionsHost ?? FunctionsHost.Default).HostUrl;

        if (!functionUrl.StartsWith(hostUrl, StringComparison.OrdinalIgnoreCase))
        {
            functionUrl = hostUrl
                .AppendPathSegment(new Uri(functionUrl).AbsolutePath)
                .ToString();
        }

        if (withCode)
        {
            var code = await GetFunctionKeyAsync(functionName)
                .ConfigureAwait(false);

            functionUrl = functionUrl
                .SetQueryParam("code", code);
        }

        return Regex.Replace(HttpUtility.UrlDecode(functionUrl), "{(\\w+)}", (match) =>
        {
            var value = replaceToken?.Invoke(match.Groups[1].Value);

            return value ?? match.Value;
        });
    }

    public static async Task<string> GetFunctionKeyAsync(string functionName)
    {
        var masterKey = await GetAdminKeyAsync()
            .ConfigureAwait(false);

        var json = await FunctionsHost.Default.HostUrl
            .AppendPathSegment("admin/functions")
            .AppendPathSegment(functionName)
            .AppendPathSegment("keys")
            .SetQueryParam("code", masterKey)
            .GetJObjectAsync()
            .ConfigureAwait(false);

        var tokens = json
            .SelectTokens($"$.keys[?(@.name != 'default')].value")
            .Select(token => token.ToString())
            .ToArray();

        if (tokens.Length == 1)
        {
            return tokens[0];
        }
        else if (tokens.Length > 1)
        {
            return tokens[new Random().Next(0, tokens.Length - 1)];
        }

        return json
            .SelectToken($"$.keys[?(@.name == 'default')].value")?
            .ToString();
    }

    private static string GetResourceId(string token)
    {
        var jwtToken = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        if (jwtToken.Payload.TryGetValue("xms_mirid", out var value))
            return value.ToString();

        throw new NotSupportedException($"The acquired token does not contain any resource id information.");
    }

    private static async Task<JObject> GetFunctionJsonAsync(string functionName)
    {
        if (!TryGetFunctionMethod(functionName, out var functionMethod))
            return null;

        if (!functionMethod.GetParameters().Any(p => p.GetCustomAttribute<HttpTriggerAttribute>() is not null))
            return null;

        var adminKey = await GetAdminKeyAsync()
            .ConfigureAwait(false);

        return await FunctionsHost.Default.HostUrl
            .AppendPathSegment("/admin/functions")
            .AppendPathSegment(functionName)
            .SetQueryParam("code", adminKey)
            .GetJObjectAsync()
            .ConfigureAwait(false);
    }

    private static async Task<JObject> GetKeyJsonAsync()
    {
        if (IsLocalEnvironment)
            return JObject.Parse("{}");

        var token = await AzureSessionService
            .AcquireTokenAsync()
            .ConfigureAwait(false);

        var response = await "https://management.azure.com"
            .AppendPathSegment(GetResourceId(token))
            .AppendPathSegment("/host/default/listKeys")
            .SetQueryParam("api-version", "2018-11-01")
            .WithOAuthBearerToken(token)
            .PostAsync(null)
            .ConfigureAwait(false);

        return await response
            .GetJsonAsync<JObject>()
            .ConfigureAwait(false);
    }

    public static async Task<string> GetAdminKeyAsync()
    {
        var json = await GetKeyJsonAsync()
            .ConfigureAwait(false);

        return json
            .SelectToken("$.masterKey")?
            .ToString();
    }

    public static async Task<string> GetHostKeyAsync(string keyName = default)
    {
        var json = await GetKeyJsonAsync()
            .ConfigureAwait(false);

        return json
            .SelectToken($"$.functionKeys['{keyName ?? "default"}']")?
            .ToString();
    }
}
