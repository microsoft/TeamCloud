// /**
//  *  Copyright (c) Microsoft Corporation.
//  *  Licensed under the MIT License.
//  */

// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.DurableTask;
// using Microsoft.Extensions.Logging;
// using TeamCloud.Model.Commands;
// using TeamCloud.Model.Commands.Core;
// using TeamCloud.Model.Data;
// using TeamCloud.Orchestration;
// using TeamCloud.Orchestrator.Activities;
// using TeamCloud.Orchestrator.Entities;

// namespace TeamCloud.Orchestrator.Orchestrations.Utilities
// {
//     public static class ProviderRegisterOrchestration
//     {
//         [FunctionName(nameof(ProviderRegisterOrchestration) + "-Trigger")]
//         public static async Task RunTrigger(
//             [TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo timerInfo,
//             [DurableClient] IDurableClient durableClient)
//         {
//             if (timerInfo is null)
//                 throw new ArgumentNullException(nameof(timerInfo));

//             if (durableClient is null)
//                 throw new ArgumentNullException(nameof(durableClient));

//             _ = await durableClient
//                 .StartNewAsync(nameof(ProviderRegisterOrchestration), Guid.NewGuid().ToString(), new Input())
//                 .ConfigureAwait(false);
//         }

//         [FunctionName(nameof(ProviderRegisterOrchestration))]
//         public static async Task RunOrchestration(
//             [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
//             ILogger log)
//         {
//             if (orchestrationContext is null)
//                 throw new ArgumentNullException(nameof(orchestrationContext));

//             var functionInput = orchestrationContext.GetInput<Input>();

//             try
//             {
//                 if (functionInput.Command is null)
//                 {
//                     // no command was given !!!
//                     // restart the orchestration
//                     // with a new command instance.

//                     orchestrationContext.SetCustomStatus("Creating command", log);

//                     var systemUser = await orchestrationContext
//                         .CallActivityWithRetryAsync<User>(nameof(TeamCloudSystemUserActivity), null)
//                         .ConfigureAwait(true);

//                     functionInput.Command = new ProviderRegisterCommand
//                     (
//                         systemUser.PopulateExternalModel(),
//                         new ProviderConfiguration()
//                     );

//                     orchestrationContext
//                         .ContinueAsNew(functionInput);
//                 }
//                 else if (functionInput.Provider is null)
//                 {
//                     // no provider was given !!!
//                     // fan out registration with
//                     // one orchestration per provider.

//                     var providers = await orchestrationContext
//                         .ListProvidersAsync()
//                         .ConfigureAwait(true);

//                     if (providers.Any())
//                     {
//                         orchestrationContext.SetCustomStatus($"Register provider/s", log);

//                         var tasks = providers
//                             .Select(provider => orchestrationContext.CallSubOrchestratorWithRetryAsync(nameof(ProviderRegisterOrchestration), new Input() { Provider = provider, Command = functionInput.Command }));

//                         await Task
//                             .WhenAll(tasks)
//                             .ConfigureAwait(true);
//                     }
//                 }
//                 else
//                 {
//                     functionInput.Command.Payload
//                         .TeamCloudApplicationInsightsKey = await orchestrationContext
//                         .GetInstrumentationKeyAsync()
//                         .ConfigureAwait(true);

//                     functionInput.Command.Payload
//                         .Properties = functionInput.Provider.Properties;

//                     orchestrationContext.SetCustomStatus($"Sending command", log);

//                     var commandResult = await orchestrationContext
//                         .SendProviderCommandAsync<ProviderRegisterCommand, ProviderRegisterCommandResult>(functionInput.Command, functionInput.Provider)
//                         .ConfigureAwait(true);

//                     if (commandResult.RuntimeStatus == CommandRuntimeStatus.Completed)
//                     {
//                         using (await orchestrationContext.LockContainerDocumentAsync(functionInput.Provider).ConfigureAwait(true))
//                         {
//                             functionInput.Provider = await orchestrationContext
//                                 .GetProviderAsync(functionInput.Provider.Id)
//                                 .ConfigureAwait(true);

//                             if (functionInput.Provider is null)
//                             {
//                                 log.LogWarning($"Provider registration skipped - provider no longer exists");
//                             }
//                             else
//                             {
//                                 functionInput.Provider.PrincipalId = commandResult.Result.PrincipalId;
//                                 functionInput.Provider.CommandMode = commandResult.Result.CommandMode;
//                                 functionInput.Provider.Registered = orchestrationContext.CurrentUtcDateTime;
//                                 functionInput.Provider.EventSubscriptions = commandResult.Result.EventSubscriptions;
//                                 functionInput.Provider.ResourceProviders = commandResult.Result.ResourceProviders;
//                                 functionInput.Provider.Properties = functionInput.Provider.Properties.Override(commandResult.Result.Properties);

//                                 orchestrationContext.SetCustomStatus($"Updating provider", log);

//                                 functionInput.Provider = await orchestrationContext
//                                     .SetProviderAsync(functionInput.Provider)
//                                     .ConfigureAwait(true);

//                                 if (functionInput.Provider.PrincipalId.HasValue)
//                                 {
//                                     orchestrationContext.SetCustomStatus($"Resolving provider identity", log);

//                                     var providerUser = await orchestrationContext
//                                         .GetUserAsync(functionInput.Provider.PrincipalId.Value.ToString(), allowUnsafe: true)
//                                         .ConfigureAwait(true);

//                                     if (providerUser is null)
//                                     {
//                                         providerUser = new User
//                                         {
//                                             Id = functionInput.Provider.PrincipalId.Value.ToString(),
//                                             Role = OrganizationUserRole.Provider,
//                                             UserType = UserType.Provider
//                                         };

//                                         orchestrationContext.SetCustomStatus($"Granting provider access", log);

//                                         _ = await orchestrationContext
//                                             .SetUserTeamCloudInfoAsync(providerUser, allowUnsafe: true)
//                                             .ConfigureAwait(true);
//                                     }
//                                 }
//                             }
//                         }

//                         orchestrationContext.SetCustomStatus($"Provider registered", log);
//                     }
//                     else
//                     {
//                         var exception = commandResult.Errors
//                             .ToException();

//                         throw exception
//                             ?? new OperationErrorException($"Provider registration ended in runtime status '{commandResult.RuntimeStatus}'");
//                     }
//                 }
//             }
//             catch (Exception exc)
//             {
//                 if (functionInput.Provider != null)
//                 {
//                     orchestrationContext.SetCustomStatus($"Failed to register provider '{functionInput.Provider.Id}' - {exc.Message}", log, exc);
//                 }
//                 else if (functionInput.Command != null)
//                 {
//                     orchestrationContext.SetCustomStatus($"Failed to register providers - {exc.Message}", log, exc);
//                 }
//                 else
//                 {
//                     orchestrationContext.SetCustomStatus($"Failed to initiate provider registration - {exc.Message}", log, exc);
//                 }
//             }
//         }

//         internal struct Input
//         {
//             public ProviderDocument Provider { get; set; }

//             public ProviderRegisterCommand Command { get; set; }
//         }
//     }

//     internal static class ProviderRegisterExtension
//     {
//         public static Task RegisterProviderAsync(this IDurableOrchestrationContext orchestrationContext, ProviderDocument provider = null, bool wait = true)
//         {
//             if (wait)
//             {
//                 // if the caller request to wait for the provider registration
//                 // we will kick off the corresponding orchestration as a sub
//                 // orchestration instead of completely new one

//                 return orchestrationContext
//                     .CallSubOrchestratorWithRetryAsync(nameof(ProviderRegisterOrchestration), new ProviderRegisterOrchestration.Input() { Provider = provider });
//             }

//             orchestrationContext
//                 .StartNewOrchestration(nameof(ProviderRegisterOrchestration), new ProviderRegisterOrchestration.Input() { Provider = provider });

//             return Task
//                 .CompletedTask;
//         }
//     }

// }
