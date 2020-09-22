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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public static class CommandResultFetchActivity
    {
        [FunctionName(nameof(CommandResultFetchActivity))]
        [RetryOptions(3)]
        public static async Task<ICommandResult> RunActivity(
            [ActivityTrigger] IDurableActivityContext activityContext)
        {
            if (activityContext is null)
                throw new ArgumentNullException(nameof(activityContext));

            var functionInput = activityContext.GetInput<Input>();
            var providerUrl = new Url(functionInput.Provider.Url?.Trim());

            if (!providerUrl.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                providerUrl = providerUrl.AppendPathSegment("api/command");

            try
            {
                ICommandResult commandResult;

                try
                {
                    commandResult = await providerUrl
                        .AppendPathSegment(functionInput.CommandMessage.CommandId)
                        .WithHeader("x-functions-key", functionInput.Provider.AuthCode)
                        .GetJsonAsync<ICommandResult>()
                        .ConfigureAwait(false);
                }
                catch (FlurlHttpException postException) when (postException.Call.HttpStatus == HttpStatusCode.Unauthorized)
                {
                    throw new RetryCanceledException($"Provider '{functionInput.Provider.Id}' failed: {postException.Message}", postException);
                }

                return commandResult;
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableExc))
            {
                throw serializableExc;
            }
        }

        public struct Input
        {
            public ProviderDocument Provider { get; set; }

            public ICommandMessage CommandMessage { get; set; }
        }
    }
}

