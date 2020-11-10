// /**
//  *  Copyright (c) Microsoft Corporation.
//  *  Licensed under the MIT License.
//  */

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.DurableTask;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Logging.Abstractions;
// using Newtonsoft.Json;
// using TeamCloud.Model;
// using TeamCloud.Model.Commands;
// using TeamCloud.Model.Commands.Core;
// using TeamCloud.Model.Common;
// using TeamCloud.Model.Data;
// using TeamCloud.Model.Internal;
// using TeamCloud.Orchestration;
// using TeamCloud.Orchestrator.Activities;
// using TeamCloud.Orchestrator.Entities;
// using TeamCloud.Serialization;

// namespace TeamCloud.Orchestrator.Orchestrations.Utilities
// {
//     public static class ProviderSendOrchestration
//     {
//         [FunctionName(nameof(ProviderSendOrchestration))]
//         public static async Task<ICommandResult> RunOrchestrator(
//             [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
//             ILogger log)
//         {
//             if (orchestrationContext is null)
//                 throw new ArgumentNullException(nameof(orchestrationContext));

//             var functionContext = orchestrationContext.GetInput<Input>();

//             var commandResult = functionContext.Command.CreateResult();
//             var commandLog = orchestrationContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

//             using (log.BeginCommandScope(functionContext.Command, functionContext.Provider))
//             using (orchestrationContext.TrackCommandMetrics(functionContext.Command))
//             {
//                 // if the provider to use only supports the simple command mode
//                 // we can not send commands like add, update, or delete project users.
//                 // instead we need to send a full project update to process the command!

//                 var isNotProjectCommandWithProjectId = functionContext.Provider.CommandMode == ProviderCommandMode.Simple
//                                                     && !string.IsNullOrEmpty(functionContext.Command.ProjectId)
//                                                     && (functionContext.Command is IProviderCommand<User>);

//                 commandResult = isNotProjectCommandWithProjectId
//                     ? await SwitchCommandAsync(orchestrationContext, functionContext, commandResult, commandLog).ConfigureAwait(true)
//                     : await ProcessCommandAsync(orchestrationContext, functionContext, commandResult, commandLog).ConfigureAwait(true);
//             }

//             return commandResult;
//         }

//         private static async Task<IProviderCommand> AugmentCommandAsync(IDurableOrchestrationContext orchestrationContext, Input functionContext)
//         {
//             if (!string.IsNullOrEmpty(functionContext.Command.ProjectId))
//             {
//                 var project = await orchestrationContext
//                     .GetProjectAsync(functionContext.Command.ProjectId, allowUnsafe: true)
//                     .ConfigureAwait(true);

//                 var providerReference = project.Type.Providers
//                     .Single(pr => pr.Id == functionContext.Provider.Id);

//                 functionContext.Command.Properties = functionContext.Provider.Properties.Resolve(project)
//                     .Override(project.Type.Properties.Resolve(project))
//                     .Override(project.Properties.Resolve(project))
//                     .Override(functionContext.Provider.Properties.Resolve(project))
//                     .Override(providerReference.Properties.Resolve(project));
//             }
//             else
//             {
//                 functionContext.Command.Properties = functionContext.Provider.Properties.Resolve();
//             }

//             if (functionContext.Command.Payload is IProperties payloadWithProperties)
//             {
//                 payloadWithProperties.Properties = payloadWithProperties.Properties.Resolve(functionContext.Command.Payload);
//             }

//             return functionContext.Command;
//         }


//         private static async Task<ICommandResult> ProcessCommandAsync(IDurableOrchestrationContext orchestrationContext, Input functionContext, ICommandResult commandResult, ILogger log)
//         {
//             var commandMessage = default(ICommandMessage);
//             var commandCallback = default(string);

//             try
//             {
//                 orchestrationContext.SetCustomStatus($"Acquire callback url", log);

//                 commandCallback = await orchestrationContext
//                     .CallActivityWithRetryAsync<string>(nameof(CallbackUrlGetActivity), new CallbackUrlGetActivity.Input() { InstanceId = orchestrationContext.InstanceId, Command = functionContext.Command })
//                     .ConfigureAwait(true);

//                 commandMessage = new ProviderCommandMessage(functionContext.Command, commandCallback);

//                 if (!(functionContext.Command is ProviderRegisterCommand) && !functionContext.Provider.Registered.HasValue)
//                 {
//                     log.LogInformation($"Register provider {functionContext.Provider.Id} for command {functionContext.Command.CommandId}");

//                     await orchestrationContext
//                         .RegisterProviderAsync(functionContext.Provider, true)
//                         .ConfigureAwait(true);
//                 }

//                 if (!string.IsNullOrEmpty(functionContext.Command.ProjectId) && functionContext.Provider.PrincipalId.HasValue)
//                 {
//                     log.LogInformation($"Enable provider {functionContext.Provider.Id} for command {functionContext.Command.CommandId}");

//                     await orchestrationContext
//                         .CallActivityWithRetryAsync(nameof(ProjectResourcesAccessActivity), new ProjectResourcesAccessActivity.Input() { ProjectId = functionContext.Command.ProjectId, PrincipalId = functionContext.Provider.PrincipalId.Value })
//                         .ConfigureAwait(true);
//                 }

//                 orchestrationContext.SetCustomStatus($"Augmenting command", log);

//                 functionContext.Command = await AugmentCommandAsync(orchestrationContext, functionContext)
//                     .ConfigureAwait(true);

//                 await orchestrationContext
//                     .AuditAsync(functionContext.Command, commandResult, functionContext.Provider)
//                     .ConfigureAwait(true);

//                 try
//                 {
//                     orchestrationContext.SetCustomStatus($"Sending command", log);

//                     commandResult = await orchestrationContext
//                         .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandSendActivity), new CommandSendActivity.Input() { Provider = functionContext.Provider, CommandMessage = commandMessage })
//                         .ConfigureAwait(true);
//                 }
//                 catch (RetryCanceledException)
//                 {
//                     commandResult = await orchestrationContext
//                         .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultFetchActivity), new CommandResultFetchActivity.Input() { Provider = functionContext.Provider, CommandMessage = commandMessage })
//                         .ConfigureAwait(true);
//                 }
//                 finally
//                 {
//                     await orchestrationContext
//                         .AuditAsync(functionContext.Command, commandResult, functionContext.Provider)
//                         .ConfigureAwait(true);
//                 }

//                 if (!commandResult.RuntimeStatus.IsFinal())
//                 {
//                     var commandTimeout = (commandResult.Timeout > TimeSpan.Zero && commandResult.Timeout < CommandResult.MaximumTimeout)
//                         ? commandResult.Timeout         // use the timeout reported back by the provider
//                         : CommandResult.MaximumTimeout; // use the defined maximum timeout

//                     orchestrationContext.SetCustomStatus($"Waiting for command result", log);

//                     commandResult = await orchestrationContext
//                         .WaitForExternalEvent<ICommandResult>(functionContext.Command.CommandId.ToString(), commandTimeout, null)
//                         .ConfigureAwait(true);

//                     if (commandResult is null)
//                     {
//                         // provider ran into a timeout
//                         // lets give our provider a last
//                         // chance to return a command result

//                         commandResult = await orchestrationContext
//                             .CallActivityWithRetryAsync<ICommandResult>(nameof(CommandResultFetchActivity), new CommandResultFetchActivity.Input() { Provider = functionContext.Provider, CommandMessage = commandMessage })
//                             .ConfigureAwait(true);

//                         if (commandResult.RuntimeStatus.IsActive())
//                         {
//                             // the last change result still doesn't report a final runtime status
//                             // escalate the timeout by throwing an appropriate exception

//                             throw new TimeoutException($"Provider '{functionContext.Provider.Id}' ran into timeout ({commandTimeout})");
//                         }
//                     }
//                 }
//             }
//             catch (Exception exc)
//             {
//                 orchestrationContext.SetCustomStatus($"Sending command failed: {exc.Message}", log, exc);

//                 commandResult ??= functionContext.Command.CreateResult();
//                 commandResult.Errors.Add(exc.AsSerializable());
//             }
//             finally
//             {
//                 await orchestrationContext
//                     .AuditAsync(functionContext.Command, commandResult, functionContext.Provider)
//                     .ConfigureAwait(true);

//                 await ProcessOutputAsync(orchestrationContext, functionContext.Provider, functionContext.Command, commandResult)
//                     .ConfigureAwait(true);
//             }

//             return commandResult;
//         }


//         private static async Task<ICommandResult> SwitchCommandAsync(IDurableOrchestrationContext orchestrationContext, Input functionContext, ICommandResult commandResult, ILogger log)
//         {
//             try
//             {
//                 await orchestrationContext
//                     .AuditAsync(functionContext.Command, commandResult, functionContext.Provider)
//                     .ConfigureAwait(true);

//                 orchestrationContext.SetCustomStatus($"Switching command", log);

//                 var project = await orchestrationContext
//                     .GetProjectAsync(functionContext.Command.ProjectId, allowUnsafe: true)
//                     .ConfigureAwait(true);

//                 functionContext.Command = new ProviderProjectUpdateCommand(functionContext.Command.User as User, project.PopulateExternalModel(), functionContext.Command.CommandId);

//                 orchestrationContext.ContinueAsNew(functionContext);
//             }
//             catch (Exception exc)
//             {
//                 orchestrationContext.SetCustomStatus($"Switching command failed: {exc.Message}", log, exc);

//                 commandResult ??= functionContext.Command.CreateResult();
//                 commandResult.Errors.Add(exc);
//             }

//             return commandResult;
//         }


//         private static async Task ProcessOutputAsync(IDurableOrchestrationContext orchestrationContext, ProviderDocument provider, IProviderCommand command, ICommandResult commandResult)
//         {
//             if (!string.IsNullOrEmpty(command.ProjectId) && commandResult is ICommandResult<ProviderOutput> providerOutputResult)
//             {
//                 using (await orchestrationContext.LockAsync<Project>(command.ProjectId).ConfigureAwait(true))
//                 {
//                     var project = await orchestrationContext
//                         .GetProjectAsync(command.ProjectId)
//                         .ConfigureAwait(true);

//                     var providerReference = project?.Type.Providers
//                         .SingleOrDefault(pr => pr.Id == provider.Id);

//                     if (providerReference != null)
//                     {
//                         var commandType = command.GetType().Name;
//                         var resultProperties = providerOutputResult?.Result?.Properties ?? new Dictionary<string, string>();

//                         if (!providerReference.Metadata.TryAdd(commandType, resultProperties))
//                         {
//                             providerReference.Metadata[commandType] =
//                                 (providerReference.Metadata[commandType] ?? new Dictionary<string, string>()).Override(resultProperties);
//                         }

//                         project = await orchestrationContext
//                             .SetProjectAsync(project)
//                             .ConfigureAwait(true);
//                     }
//                 }
//             }
//         }

//         internal struct Input
//         {
//             public ProviderDocument Provider { get; set; }

//             public IProviderCommand Command { get; set; }
//         }
//     }

//     internal static class ProviderSendExtensions
//     {
//         internal static async Task<string> SendProviderCommandAsync<TCommand>(this IDurableClient durableClient, TCommand command, ProviderDocument provider)
//             where TCommand : IProviderCommand
//         {
//             if (command is null)
//                 throw new ArgumentNullException(nameof(command));

//             if (provider is null)
//                 throw new ArgumentNullException(nameof(provider));

//             var commandJson = JsonConvert.SerializeObject(command);
//             var commandCopy = JsonConvert.DeserializeObject<TCommand>(commandJson);

//             // ensure our commmand copy contains
//             // the right provider id information
//             commandCopy.ProviderId = provider.Id;


//             // we define the orchestration instance id upfront
//             // so we can check if the orchestration start fails
//             // if this is because of a duplicate command operation
//             var instanceId = $"{command.CommandId}@{provider.Id}";

//             try
//             {
//                 return await durableClient
//                     .StartNewAsync(nameof(ProviderSendOrchestration), instanceId, new ProviderSendOrchestration.Input() { Command = command, Provider = provider })
//                     .ConfigureAwait(false);
//             }
//             catch
//             {
//                 if ((await durableClient.GetStatusAsync(instanceId).ConfigureAwait(false)) is null) throw;

//                 throw new NotSupportedException($"Command {command.CommandId} can only sent once to provider {provider.Id}");
//             }
//         }

//         internal static async Task<TCommandResult> SendProviderCommandAsync<TCommand, TCommandResult>(this IDurableOrchestrationContext orchestrationContext, TCommand command)
//             where TCommand : IProviderCommand
//             where TCommandResult : ICommandResult
//         {
//             if (command is null)
//                 throw new ArgumentNullException(nameof(command));

//             var providers = await orchestrationContext
//                 .ListProvidersAsync()
//                 .ConfigureAwait(true);

//             var providerTasks = providers.Select(p => orchestrationContext.SendProviderCommandAsync<TCommand, TCommandResult>(command, p));

//             var providerResults = await Task
//                 .WhenAll(providerTasks)
//                 .ConfigureAwait(true);

//             var providerResult = (TCommandResult)command.CreateResult();

//             providerResults.SelectMany(r => r.Errors).ToList().ForEach(e => providerResult.Errors.Add(e));

//             return providerResult;
//         }


//         internal static async Task<TCommandResult> SendProviderCommandAsync<TCommand, TCommandResult>(this IDurableOrchestrationContext orchestrationContext, TCommand command, ProviderDocument provider)
//             where TCommand : IProviderCommand
//             where TCommandResult : ICommandResult
//         {
//             if (command is null)
//                 throw new ArgumentNullException(nameof(command));

//             if (provider is null)
//                 throw new ArgumentNullException(nameof(provider));

//             command.ProviderId = provider.Id;

//             var providerResult = (TCommandResult)await orchestrationContext
//                 .CallSubOrchestratorWithRetryAsync<ICommandResult>(nameof(ProviderSendOrchestration), new ProviderSendOrchestration.Input() { Command = command, Provider = provider })
//                 .ConfigureAwait(true);

//             if (providerResult is null)
//             {
//                 providerResult = (TCommandResult)command.CreateResult();
//                 providerResult.Errors.Add(new NullReferenceException($"Provider '{provider.Id}' returned no result for command '{command.CommandId}'"));
//             }

//             return providerResult;
//         }


//         internal static Task<IDictionary<string, ICommandResult>> SendProviderCommandAsync<TCommand>(this IDurableOrchestrationContext orchestrationContext, TCommand command, Project project, bool failFast = false)
//             where TCommand : IProviderCommand
//             => orchestrationContext.SendProviderCommandAsync<TCommand, ICommandResult>(command, project, failFast);


//         internal static async Task<IDictionary<string, TCommandResult>> SendProviderCommandAsync<TCommand, TCommandResult>(this IDurableOrchestrationContext orchestrationContext, TCommand command, Project project, bool failFast = false)
//             where TCommand : IProviderCommand
//             where TCommandResult : ICommandResult
//         {
//             if (command is null)
//                 throw new ArgumentNullException(nameof(command));

//             if (project is null && command is IProviderCommand<Project>)
//                 throw new InvalidOperationException("Must pass original Project (internal) for ProviderCommands with a payload of type Project (external).");

//             if (project is null && !string.IsNullOrEmpty(command.ProjectId))
//             {
//                 project = await orchestrationContext
//                     .GetProjectAsync(command.ProjectId, allowUnsafe: true)
//                     .ConfigureAwait(true);
//             }

//             var providerBatches = await orchestrationContext
//                 .CallActivityWithRetryAsync<IEnumerable<IEnumerable<ProviderDocument>>>(nameof(CommandProviderActivity), project)
//                 .ConfigureAwait(true);

//             var commandResults = Enumerable.Empty<KeyValuePair<string, TCommandResult>>();

//             foreach (var providerBatch in providerBatches)
//             {
//                 foreach (var commandResult in commandResults.Where(cr => cr.Value is ICommandResult<ProviderOutput>))
//                 {
//                     var commandResultOutput = commandResult.Value as ICommandResult<ProviderOutput>;

//                     command.Results.TryAdd(commandResult.Key, commandResultOutput?.Result?.Properties ?? new Dictionary<string, string>());
//                 }

//                 var providerTasks = providerBatch.Select(async provider =>
//                 {
//                     var providerResult = await orchestrationContext
//                         .SendProviderCommandAsync<TCommand, TCommandResult>(command, provider)
//                         .ConfigureAwait(true);

//                     return new KeyValuePair<string, TCommandResult>(provider.Id, providerResult);
//                 });

//                 commandResults = commandResults.Concat(await Task
//                     .WhenAll(providerTasks)
//                     .ConfigureAwait(true));

//                 if (failFast && commandResults.Any(cr => cr.Value.Errors.Any()))
//                     break;
//             }

//             return new Dictionary<string, TCommandResult>(commandResults);
//         }
//     }
// }
