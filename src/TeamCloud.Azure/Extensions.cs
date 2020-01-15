/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using TeamCloud.Azure.Deployments;

namespace TeamCloud.Azure
{
    public static class Extensions
    {
        public static IServiceCollection AddAzure(this IServiceCollection services)
            => services
            .AddSingleton<IAzureSessionService, AzureSessionService>()
            .AddSingleton<IAzureDirectoryService, AzureDirectoryService>()
            .AddSingleton<IAzureDeploymentService, AzureDeploymentService>();

        internal static bool IsGuid(this string value)
            => Guid.TryParse(value, out var _);

        internal static bool IsEMail(this string value)
            => new EmailAddressAttribute().IsValid(value);

        internal static async Task<JObject> GetJObjectAsync(this IFlurlRequest request)
        {
            var json = await request.GetJsonAsync().ConfigureAwait(false);

            return json is null ? null : JObject.FromObject(json);
        }

        internal static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection)
            => new Dictionary<TKey, TValue>(collection);

        internal static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
            => new ReadOnlyDictionary<TKey, TValue>(dictionary);

    }
}
