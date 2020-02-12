using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Azure.Resources
{
    public static class Extensions
    {
        public static IAzureConfiguration AddResources(this IAzureConfiguration azureConfiguration)
        {
            azureConfiguration.Services
                .TryAddSingleton<IAzureResourceService, AzureResourceService>();

            return azureConfiguration;
        }

        public static bool IsAzureResourceId(this string resourceId)
        {
            if (resourceId is null)
                throw new ArgumentNullException(nameof(resourceId));

            return AzureResourceIdentifier.TryParse(resourceId, out var _);
        }

        public static Task<IEnumerable<string>> GetApiVersionsAsync(this IAzureResourceService azureResourceService, AzureResourceIdentifier azureResourceIdentifier, bool includePreviewVersions = false)
            => azureResourceService.GetApiVersionsAsync(azureResourceIdentifier.SubscriptionId, azureResourceIdentifier.ResourceNamespace, azureResourceIdentifier.ResourceTypeName, includePreviewVersions);

        internal static T WithAzureResourceException<T>(this T obj, AzureEnvironment environment) where T : IHttpSettingsContainer
        {
            obj.Settings.OnErrorAsync = async (call) =>
            {
                try
                {
                    if (call.Request.RequestUri.ToString().StartsWith(environment.ResourceManagerEndpoint, StringComparison.OrdinalIgnoreCase) && call.Response != null)
                    {
                        await call.Response.Content
                            .LoadIntoBufferAsync()
                            .ConfigureAwait(false);

                        using var jsonStream = await call.Response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        using var jsonReader = new JsonTextReader(new StreamReader(jsonStream));

                        var json = await JObject
                            .ReadFromAsync(jsonReader)
                            .ConfigureAwait(false);

                        var errorMessage = json
                            .SelectToken("$.error.message")?
                            .ToString();

                        if (!string.IsNullOrEmpty(errorMessage))
                            throw new AzureResourceException(errorMessage);
                    }
                }
                catch (Exception exc) when (!(exc is AzureResourceException))
                {
                    // swallow all exceptions other than AzureResourceException
                }
            };

            return obj;
        }

        internal static Guid GetRoleDefinitionId(this RoleAssignmentInner roleAssignment)
        {
            var regex = new Regex(@"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}");
            var match = regex.Matches(roleAssignment.RoleDefinitionId).LastOrDefault()?.Value;

            return string.IsNullOrEmpty(match) ? Guid.Empty : Guid.Parse(match);
        }

    }
}
