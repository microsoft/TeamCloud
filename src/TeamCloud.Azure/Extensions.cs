/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace TeamCloud.Azure
{
    public static class Extensions
    {
        public static IServiceCollection AddAzure(this IServiceCollection services)
            => services
            .AddSingleton<IAzureSessionFactory, AzureSessionFactory>()
            .AddSingleton<IAzureDirectoryService, AzureDirectoryService>();

        internal static bool IsGuid(this string value)
            => Guid.TryParse(value, out var _);

        internal static bool IsEMail(this string value)
            => new EmailAddressAttribute().IsValid(value);

        internal static async Task<JObject> GetJObjectAsync(this IFlurlRequest request)
        {
            var json = await request.GetJsonAsync().ConfigureAwait(false);

            return json is null ? null : JObject.FromObject(json);
        }
    }
}
