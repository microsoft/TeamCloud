/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Azure.Deployments
{
    public interface IAzureDeployment
    {
        string ResourceId { get; }

        Task<string> GetErrorAsync();

        Task<AzureDeploymentState> GetStateAsync();

        Task<AzureDeploymentState> WaitAsync(bool throwOnError = false, bool cleanUp = false);

        Task<IReadOnlyDictionary<string, object>> GetOutputAsync();

        Task DeleteAsync();
    }

    public sealed class AzureDeployment : IAzureDeployment
    {
        private static readonly AzureDeploymentState[] ProgressStates = new AzureDeploymentState[]
        {
            AzureDeploymentState.Accepted,
            AzureDeploymentState.Running,
            AzureDeploymentState.Deleting
        };

        private static readonly AzureDeploymentState[] FinalStates = new AzureDeploymentState[]
        {
            AzureDeploymentState.Succeeded,
            AzureDeploymentState.Cancelled,
            AzureDeploymentState.Failed
        };

        private static readonly AzureDeploymentState[] ErrorStates = new AzureDeploymentState[]
        {
            AzureDeploymentState.Cancelled,
            AzureDeploymentState.Failed
        };

        private readonly IAzureSessionService azureSessionFactory;
        private JObject deploymentJsonFinal = null;

        internal AzureDeployment(string resourceId, IAzureSessionService azureSessionFactory)
        {
            ResourceId = resourceId?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(resourceId));
            this.azureSessionFactory = azureSessionFactory ?? throw new ArgumentNullException(nameof(azureSessionFactory));
        }

        public string ResourceId { get; }

        private async Task<JObject> GetDeploymentJsonAsync()
        {
            if (deploymentJsonFinal != null)
                return deploymentJsonFinal;

            try
            {
                var token = await azureSessionFactory
                    .AcquireTokenAsync(AzureAuthorities.AzureResourceManager)
                    .ConfigureAwait(false);

                var json = await AzureAuthorities.AzureResourceManager
                    .AppendPathSegment(ResourceId)
                    .SetQueryParam("api-version", "2019-05-01")
                    .WithOAuthBearerToken(token)
                    .GetJObjectAsync()
                    .ConfigureAwait(false);

                if (FinalStates.Contains(GetState(json)))
                    deploymentJsonFinal = json;

                return json;
            }
            catch (FlurlHttpException exc) when (exc.Call.HttpStatus == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task<string> GetCorrelationIdAsync()
        {
            var json = await GetDeploymentJsonAsync()
                .ConfigureAwait(false);

            return json?.SelectToken("$.properties.correlationId")?.ToString();
        }

        private async Task<IEnumerable<string>> GetDeploymentIdsByCorrelationIdAsync(string correlationId)
        {
            var deploymentIds = new List<string>()
            {
                ResourceId
            };

            var url = AzureAuthorities.AzureResourceManager
                .AppendPathSegment(ResourceId.Substring(0, ResourceId.LastIndexOf('/') + 1))
                .ToString();

            while (!string.IsNullOrEmpty(url))
            {
                var token = await azureSessionFactory
                    .AcquireTokenAsync(AzureAuthorities.AzureResourceManager)
                    .ConfigureAwait(false);

                var json = await url
                    .SetQueryParam("api-version", "2019-05-01")
                    .WithOAuthBearerToken(token)
                    .GetJObjectAsync();

                if (json is null)
                    break;

                deploymentIds.AddRange(json
                    .SelectTokens($"$.value[?(@.properties.correlationId == '{correlationId}')].id")
                    .Select(token => token.ToString()));

                url = json.SelectToken("$.nextLink")?.ToString();
            }

            return deploymentIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private AzureDeploymentState GetState(JObject deploymentJson)
        {
            var jsonState = deploymentJson?.SelectToken("$..provisioningState")?.ToString();

            if (string.IsNullOrEmpty(jsonState))
                return AzureDeploymentState.Unknown;
            else if (Enum.TryParse<AzureDeploymentState>(jsonState, out AzureDeploymentState state))
                return state;
            else
                throw new NotSupportedException($"The deployment status '{jsonState}' is not supported.");
        }

        public async Task<AzureDeploymentState> GetStateAsync()
        {
            var json = await GetDeploymentJsonAsync()
                .ConfigureAwait(false);

            return GetState(json);
        }

        public async Task<string> GetErrorAsync()
        {
            var json = await GetDeploymentJsonAsync()
                .ConfigureAwait(false);

            return json?.SelectToken("")?.ToString();
        }

        public async Task<AzureDeploymentState> WaitAsync(bool throwOnError = false, bool cleanUp = false)
        {
            var state = await GetStateAsync()
                .ConfigureAwait(false);

            if (state == AzureDeploymentState.Unknown)
                return state;

            try
            {
                while (true)
                {
                    if (ProgressStates.Contains(state))
                    {
                        await Task.Delay(5000)
                            .ConfigureAwait(false);

                        state = await GetStateAsync()
                            .ConfigureAwait(false);
                    }
                    else if (throwOnError && ErrorStates.Contains(state))
                    {
                        var exceptionMessage = $"Deployment '{ResourceId}' ended in state '{state}'";
                        var resourceError = await GetErrorAsync().ConfigureAwait(false);

                        throw new AzureDeploymentException(exceptionMessage, ResourceId, resourceError);
                    }
                    else
                    {
                        break; // template successfully processed
                    }
                }
            }
            finally
            {
                if (cleanUp)
                    await DeleteAsync().ConfigureAwait(false);
            }

            return state;
        }

        public async Task DeleteAsync()
        {
            var state = await GetStateAsync()
                .ConfigureAwait(false);

            if (ProgressStates.Contains(state))
                throw new InvalidOperationException($"Deployment '{ResourceId}' cannot be deleted while in state '{state}'");

            var correlationId = await GetCorrelationIdAsync()
                .ConfigureAwait(false);

            var deploymentIds = await GetDeploymentIdsByCorrelationIdAsync(correlationId)
                .ConfigureAwait(false);

            var token = await azureSessionFactory
                .AcquireTokenAsync(AzureAuthorities.AzureResourceManager)
                .ConfigureAwait(false);

            var tasks = deploymentIds.Select(deploymentId => AzureAuthorities.AzureResourceManager
                .AppendPathSegment(deploymentId)
                .SetQueryParam("api-version", "2019-08-01")
                .WithOAuthBearerToken(token)
                .AllowAnyHttpStatus()
                .DeleteAsync());

            Task.WaitAll(tasks.ToArray());
        }

        public async Task<IReadOnlyDictionary<string, object>> GetOutputAsync()
        {
            var state = await GetStateAsync()
                .ConfigureAwait(false);

            if (ProgressStates.Contains(state))
                throw new InvalidOperationException($"Deployment '{ResourceId}' cannot provide output in state '{state}'");

            var json = await GetDeploymentJsonAsync()
                .ConfigureAwait(false);

            var output = json
                .SelectToken("$..outputs")?
                .ToObject<JObject>();

            return output.Children<JProperty>()
                .Select(token => new KeyValuePair<string, object>(token.Name, ConvertOutputValue(token)))
                .ToDictionary()
                .AsReadOnly();

            object ConvertOutputValue(JProperty token)
            {
                var outputType = token.Value.SelectToken("type")?.ToString();
                var outputValue = token.Value.SelectToken("value");

                if (outputValue is null)
                    return null;

                switch ((outputType ?? "string").ToUpperInvariant())
                {
                    // simple type conversion

                    case "BOOL":
                        return outputValue.ToObject<bool>();

                    case "INT":
                        return outputValue.ToObject<int>();

                    case "STRING":
                        return outputValue.ToString();

                    // complex type conversion

                    case "ARRAY":
                        return outputValue.ToObject<JArray>();

                    case "OBJECT":
                        return outputValue.ToObject<JObject>();

                    // unsupported type conversion

                    default:
                        throw new NotSupportedException($"Output type '{outputType}' is not supported.");
                }
            }

        }
    }
}
