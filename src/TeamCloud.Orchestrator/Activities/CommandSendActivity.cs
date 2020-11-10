// /**
//  *  Copyright (c) Microsoft Corporation.
//  *  Licensed under the MIT License.
//  */

// using System;
// using System.Net;
// using System.Threading.Tasks;
// using Flurl;
// using Flurl.Http;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.DurableTask;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
// using TeamCloud.Http;
// using TeamCloud.Model.Commands;
// using TeamCloud.Model.Commands.Core;
// using TeamCloud.Model.Data;
// using TeamCloud.Orchestration;
// using TeamCloud.Orchestrator.Services;
// using TeamCloud.Serialization;

// namespace TeamCloud.Orchestrator.Activities
// {
//     public sealed class CommandSendActivity
//     {
//         private readonly IApiOptions apiOptions;

//         public CommandSendActivity(IApiOptions apiOptions)
//         {
//             this.apiOptions = apiOptions ?? throw new ArgumentNullException(nameof(apiOptions));
//         }

//         [FunctionName(nameof(CommandSendActivity)), RetryOptions(3)]
//         public async Task<ICommandResult> RunActivity(
//             [ActivityTrigger] IDurableActivityContext activityContext,
//             ILogger log)
//         {
//             if (activityContext is null)
//                 throw new ArgumentNullException(nameof(activityContext));

//             var functionInput = activityContext.GetInput<Input>();

//             try
//             {
//                 ReferenceLink.BaseUrl = apiOptions.Url ?? ReferenceLink.BaseUrl
//                     ?? throw new NotSupportedException("Missing API base URL in configuration.");

//                 var providerUrl = new Url(functionInput.Provider.Url?.Trim())
//                     .SetQueryParam("providerId", functionInput.Provider.Id);

//                 log.LogInformation(string.Join(", ",
//                     $"Sending command {functionInput.CommandMessage.CommandId} ({functionInput.CommandMessage.CommandType}) to {providerUrl}",
//                     JsonConvert.SerializeObject(functionInput.CommandMessage)));

//                 var response = await providerUrl
//                     .WithHeader("x-functions-key", functionInput.Provider.AuthCode)
//                     .WithHeader("x-functions-callback", (functionInput.CommandMessage as ProviderCommandMessage)?.CallbackUrl)
//                     .WithHeader("x-teamcloud-provider", functionInput.Provider.Id)
//                     .PostJsonAsync(functionInput.CommandMessage)
//                     .ConfigureAwait(false);

//                 return await response.Content
//                     .ReadAsJsonAsync<ICommandResult>()
//                     .ConfigureAwait(false);
//             }
//             catch (FlurlHttpException postException) when (postException.Call.HttpStatus == HttpStatusCode.Conflict)
//             {
//                 // there is no need to retry sending the command - the provider reported a conflict, so the command was already sent
//                 throw new RetryCanceledException($"Provider '{functionInput.Provider.Id}' failed with status code {postException.Call.HttpStatus}", postException);
//             }
//             catch (FlurlHttpException postException) when (postException.Call.HttpStatus == HttpStatusCode.BadRequest)
//             {
//                 // there is no need to retry sending the command - the provider reported a bad message payload
//                 throw new RetryCanceledException($"Provider '{functionInput.Provider.Id}' failed with status code {postException.Call.HttpStatus}", postException);
//             }
//             catch (FlurlHttpException postException) when (postException.Call.HttpStatus == HttpStatusCode.Unauthorized)
//             {
//                 // there is no need to retry sending the command - seems like our auth code became invalid
//                 throw new RetryCanceledException($"Provider '{functionInput.Provider.Id}' failed with status code {postException.Call.HttpStatus}", postException);
//             }
//             catch (Exception exc) when (!exc.IsSerializable(out var serializableExc))
//             {
//                 throw serializableExc;
//             }
//         }

//         internal struct Input
//         {
//             public ProviderDocument Provider { get; set; }

//             public ICommandMessage CommandMessage { get; set; }
//         }

//     }
// }
