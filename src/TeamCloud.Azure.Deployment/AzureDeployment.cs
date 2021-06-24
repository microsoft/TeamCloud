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
using Newtonsoft.Json.Linq;
using TeamCloud.Http;

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeployment
    {
        string ResourceId { get; }

        Task<IEnumerable<string>> GetErrorsAsync();

        Task<AzureDeploymentState> GetStateAsync();

        Task<AzureDeploymentState> WaitAsync(bool throwOnError = false, bool cleanUp = false);

        Task<IReadOnlyDictionary<string, object>> GetOutputAsync();

        Task DeleteAsync();
    }

    public sealed class AzureDeployment : IAzureDeployment
    {


        private readonly IAzureSessionService azureSessionService;
        private JObject deploymentJsonFinal;

        internal AzureDeployment(string resourceId, IAzureSessionService azureSessionService)
        {
            ResourceId = resourceId?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(resourceId));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        public string ResourceId { get; }

        private async Task<JObject> GetDeploymentJsonAsync()
        {
            if (deploymentJsonFinal != null)
                return deploymentJsonFinal;

            try
            {
                var token = await azureSessionService
                    .AcquireTokenAsync()
                    .ConfigureAwait(false);

                var json = await azureSessionService.Environment.ResourceManagerEndpoint
                    .AppendPathSegment(ResourceId)
                    .SetQueryParam("api-version", "2019-05-01")
                    .WithOAuthBearerToken(token)
                    .GetJObjectAsync()
                    .ConfigureAwait(false);

                if (GetState(json).IsFinalState())
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

            var url = azureSessionService.Environment.ResourceManagerEndpoint
                .AppendPathSegment(ResourceId.Substring(0, ResourceId.LastIndexOf('/') + 1))
                .ToString();

            while (!string.IsNullOrEmpty(url))
            {
                var token = await azureSessionService
                    .AcquireTokenAsync()
                    .ConfigureAwait(false);

                var json = await url
                    .SetQueryParam("api-version", "2019-05-01")
                    .WithOAuthBearerToken(token)
                    .GetJObjectAsync()
                    .ConfigureAwait(false);

                if (json is null)
                    break;

                deploymentIds.AddRange(json
                    .SelectTokens($"$.value[?(@.properties.correlationId == '{correlationId}')].id")
                    .Select(token => token.ToString()));

                url = json.SelectToken("$.nextLink")?.ToString();
            }

            return deploymentIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static AzureDeploymentState GetState(JObject deploymentJson)
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

        public async Task<IEnumerable<string>> GetErrorsAsync()
        {
            var json = await GetDeploymentJsonAsync()
                .ConfigureAwait(false);

            var errorToken = json.SelectToken("$.properties.error");

            return AzureDeploymentException.ResolveResourceErrors(errorToken);
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
                    if (state.IsProgressState())
                    {
                        await Task.Delay(5000)
                            .ConfigureAwait(false);

                        state = await GetStateAsync()
                            .ConfigureAwait(false);
                    }
                    else if (throwOnError && state.IsErrorState())
                    {
                        var exceptionMessage = $"Deployment '{ResourceId}' ended in state '{state}'";
                        var deploymentErrors = await GetErrorsAsync().ConfigureAwait(false);

                        throw new AzureDeploymentException(exceptionMessage, ResourceId, deploymentErrors?.ToArray());
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

            if (state.IsProgressState())
                throw new InvalidOperationException($"Deployment '{ResourceId}' cannot be deleted while in state '{state}'");

            var correlationId = await GetCorrelationIdAsync()
                .ConfigureAwait(false);

            var deploymentIds = await GetDeploymentIdsByCorrelationIdAsync(correlationId)
                .ConfigureAwait(false);

            var token = await azureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var tasks = deploymentIds.Select(deploymentId => azureSessionService.Environment.ResourceManagerEndpoint
                .AppendPathSegment(deploymentId)
                .SetQueryParam("api-version", "2019-08-01")
                .WithOAuthBearerToken(token)
                .AllowAnyHttpStatus()
                .DeleteAsync());

            await tasks
                .WhenAll()
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyDictionary<string, object>> GetOutputAsync()
        {
            var state = await GetStateAsync()
                .ConfigureAwait(false);

            for (int i = 0; state == AzureDeploymentState.Unknown && i < 10; i++)
            {
                // in some cases Azure doesn't return the state immediately
                // this poor mans retry loop tries to solve this behaviour

                state = await GetStateAsync()
                    .ConfigureAwait(false);
            }

            if (state.IsProgressState() || state == AzureDeploymentState.Unknown)
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

            static object ConvertOutputValue(JProperty token)
            {
                var outputType = token.Value.SelectToken("type")?.ToString();
                var outputValue = token.Value.SelectToken("value");

                if (outputValue is null)
                    return null;

                return ((outputType ?? "string").ToUpperInvariant()) switch
                {
                    // simple type conversion
                    "BOOL" => outputValue.ToObject<bool>(),
                    "INT" => outputValue.ToObject<int>(),
                    "STRING" => outputValue.ToString(),

                    // complex type conversion
                    "ARRAY" => outputValue.ToObject<JArray>(),
                    "OBJECT" => outputValue.ToObject<JObject>(),

                    // unsupported type conversion
                    _ => throw new NotSupportedException($"Output type '{outputType}' is not supported."),
                };
            }

        }
    }
}
