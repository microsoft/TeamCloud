// /**
//  *  Copyright (c) Microsoft Corporation.
//  *  Licensed under the MIT License.
//  */

// using System;
// using System.Threading.Tasks;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.DurableTask;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Logging.Abstractions;
// using TeamCloud.Azure.Resources;
// using TeamCloud.Orchestration;
// using TeamCloud.Orchestration.Deployment;
// using TeamCloud.Orchestrator.Activities;
// using TeamCloud.Orchestrator.Entities;
// using TeamCloud.Orchestrator.Templates;

// namespace TeamCloud.Orchestrator.Orchestrations.Utilities
// {
//     public static class SubscriptionInitializationOrchestration
//     {
//         private static readonly Version TargetSubscriptionVersion = new InitializeSubscriptionTemplate().TemplateVersion;

//         [FunctionName(nameof(SubscriptionInitializationOrchestration))]
//         public static async Task RunOrchestrator(
//             [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
//             ILogger log)
//         {
//             if (orchestrationContext is null)
//                 throw new ArgumentNullException(nameof(orchestrationContext));

//             var functionLog = orchestrationContext.CreateReplaySafeLogger(log ?? NullLogger.Instance);

//             var subscriptionId = orchestrationContext.GetInput<Guid>();
//             var resourceId = new AzureResourceIdentifier(subscriptionId);
//             var initializationTimeout = TimeSpan.FromMinutes(5);

//             try
//             {
//                 var currentSubscriptionVersion = await orchestrationContext
//                     .GetSubscriptionVersionAsync(subscriptionId)
//                     .ConfigureAwait(true);

//                 if (currentSubscriptionVersion != TargetSubscriptionVersion)
//                 {
//                     using (await orchestrationContext.LockAzureResourceAsync(resourceId).ConfigureAwait(true))
//                     {
//                         currentSubscriptionVersion = await orchestrationContext
//                             .GetSubscriptionVersionAsync(subscriptionId)
//                             .ConfigureAwait(true);

//                         if (currentSubscriptionVersion != TargetSubscriptionVersion)
//                         {
//                             // we have to offload the subscription initialization deployment to
//                             // an independant orchestration (StartDeploymentAsync) as we initiate
//                             // the deployment inside a critical section. this doesn't allow us
//                             // to run the deploy as a nested orchestration (CallDeploymentAsync).

//                             var deploymentOutputEventName = subscriptionId.ToString();

//                             _ = await orchestrationContext
//                                 .StartDeploymentAsync(nameof(ProjectSubscriptonInitializeActivity), subscriptionId, deploymentOutputEventName)
//                                 .ConfigureAwait(true);

//                             await orchestrationContext
//                                 .WaitForExternalEvent(deploymentOutputEventName, initializationTimeout)
//                                 .ConfigureAwait(true);

//                             _ = await orchestrationContext
//                                 .SetSubscriptionVersionAsync(subscriptionId, TargetSubscriptionVersion)
//                                 .ConfigureAwait(true);
//                         }
//                     }
//                 }
//             }
//             catch (TimeoutException exc)
//             {
//                 functionLog.LogError(exc, $"Failed to initialize subscription '{subscriptionId}' within a {initializationTimeout} timeout");
//             }
//             catch (Exception exc)
//             {
//                 functionLog.LogError(exc, $"Failed to initialize subscription '{subscriptionId}': {exc.Message}");
//             }
//         }
//     }

//     internal static class SubscriptionInitializationExtensions
//     {
//         private static readonly Version TargetSubscriptionVersion = new InitializeSubscriptionTemplate().TemplateVersion;

//         internal static async Task InitializeSubscriptionAsync(this IDurableOrchestrationContext orchestrationContext, Guid subscriptionId, bool waitFor = false)
//         {
//             var currentSubscriptionVersion = await orchestrationContext
//                 .GetSubscriptionVersionAsync(subscriptionId)
//                 .ConfigureAwait(true);

//             if (currentSubscriptionVersion != TargetSubscriptionVersion)
//             {
//                 if (waitFor)
//                 {
//                     await orchestrationContext
//                         .CallSubOrchestratorWithRetryAsync(nameof(SubscriptionInitializationOrchestration), subscriptionId)
//                         .ConfigureAwait(true);
//                 }
//                 else
//                 {
//                     orchestrationContext.StartNewOrchestration(nameof(SubscriptionInitializationOrchestration), subscriptionId);
//                 }
//             }
//         }
//     }

// }
