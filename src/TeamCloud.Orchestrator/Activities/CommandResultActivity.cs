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
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public static class CommandResultActivity
    {
        [FunctionName(nameof(CommandResultActivity))]
        [RetryOptions(3)]
        public static async Task<ICommandResult> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (provider, message) = functionContext.GetInput<(Provider, ProviderCommandMessage)>();

            var providerUrl = new Url(provider.Url?.Trim());

            if (!providerUrl.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                providerUrl = providerUrl.AppendPathSegment("api/command");

            try
            {
                ICommandResult commandResult;

                try
                {
                    commandResult = await providerUrl
                        .AppendPathSegment(message.CommandId)
                        .WithHeader("x-functions-key", provider.AuthCode)
                        .GetJsonAsync<ICommandResult>()
                        .ConfigureAwait(false);
                }
                catch (FlurlHttpException postException) when (postException.Call.HttpStatus == HttpStatusCode.Unauthorized)
                {
                    throw new RetryCanceledException($"Provider '{provider.Id}' failed: {postException.Message}", postException);
                }

                return commandResult;
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableExc))
            {
                throw serializableExc;
            }
        }
    }
}

