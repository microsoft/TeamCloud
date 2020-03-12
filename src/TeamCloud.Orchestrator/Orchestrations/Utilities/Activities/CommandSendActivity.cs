/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Orchestrations.Utilities.Activities
{
    public static class CommandSendActivity
    {
        [FunctionName(nameof(CommandSendActivity))]
        [RetryOptions(3, typeof(RetryHandler))]
        public static async Task<ICommandResult> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {

            var (provider, message) = functionContext.GetInput<(Provider, ProviderCommandMessage)>();

            var providerUrl = new Url(provider.Url?.Trim());

            if (!providerUrl.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                providerUrl = providerUrl.AppendPathSegment("api/command");
            }

            log.LogInformation($"Sending command {message.CommandId} ({message.CommandType}) to {providerUrl}. Payload:{JsonConvert.SerializeObject(message)}");

            try
            {
                var response = await providerUrl
                    .WithHeader("x-functions-key", provider.AuthCode)
                    .WithHeader("x-functions-callback", message.CallbackUrl)
                    .AllowHttpStatus(HttpStatusCode.Conflict, HttpStatusCode.BadRequest)
                    .PostJsonAsync(message)
                    .ConfigureAwait(false);

                log.LogInformation($"Sending command {message.CommandId} ({message.CommandType}) to {providerUrl} returned status code {response.StatusCode}.");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadRequest:

                        // the provider reported back a bad request
                        // as resending the same payload doesn't
                        // make sense we throw a cancelation exception

                        throw new OperationCanceledException($"Provider '{provider.Id}' reported back a bad message for payload: {JsonConvert.SerializeObject(message)}");

                    case HttpStatusCode.Conflict:

                        // the provider returned a conflict 
                        // this could mean that the sent command
                        // is already in-flight. lets ask the provider
                        // if there is a status available

                        response = await providerUrl
                            .AppendPathSegment(message.CommandId)
                            .WithHeader("x-functions-key", provider.AuthCode)
                            .GetAsync()
                            .ConfigureAwait(false);

                        break;
                }

                var responseJson = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                return JsonConvert.DeserializeObject<ICommandResult>(responseJson);
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableExc))
            {
                throw serializableExc;
            }
        }

        private class RetryHandler : DefaultRetryHandler
        {
            public override bool Handle(Exception exception)
            {
                if (exception is OperationCanceledException)
                {
                    return false;
                }

                return base.Handle(exception);
            }
        }
    }
}
