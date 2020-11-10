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
// using TeamCloud.Azure.Resources;
// using TeamCloud.Data;
// using TeamCloud.Model.Data;
// using TeamCloud.Orchestration;
// using TeamCloud.Serialization;

// namespace TeamCloud.Orchestrator.Activities
// {
//     public class ResourceProviderRegisterActivity
//     {
//         private readonly IProviderRepository providerRepository;
//         private readonly IAzureResourceService azureResourceService;

//         public ResourceProviderRegisterActivity(IProviderRepository providerRepository, IAzureResourceService azureResourceService)
//         {
//             this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
//             this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
//         }

//         [FunctionName(nameof(ResourceProviderRegisterActivity)), RetryOptions(3)]
//         public async Task RunActivity(
//             [ActivityTrigger] IDurableActivityContext activityContext,
//             ILogger log)
//         {
//             if (activityContext is null)
//                 throw new ArgumentNullException(nameof(activityContext));

//             var project = activityContext.GetInput<Project>();

//             try
//             {
//                 var providers = await Task.WhenAll(project.Type.Providers
//                         .Select(p => providerRepository.GetAsync(p.Id)))
//                     .ConfigureAwait(false);

//                 var resourceProviderNamespaces = providers
//                     .Where(p => p.ResourceProviders?.Any() ?? false)
//                     .SelectMany(p => p.ResourceProviders)
//                     .Distinct();

//                 if (resourceProviderNamespaces.Any())
//                     await azureResourceService
//                         .RegisterProvidersAsync(project.ResourceGroup.SubscriptionId, resourceProviderNamespaces)
//                         .ConfigureAwait(false);

//             }
//             catch (Exception exc)
//             {
//                 log.LogError(exc, $"Activity '{nameof(ResourceProviderRegisterActivity)} failed: {exc.Message}");

//                 throw exc.AsSerializable();
//             }
//         }
//     }

//     internal static class ResourceProviderRegisterExtension
//     {
//         public static Task RegisterResourceProvidersAsync(this IDurableOrchestrationContext orchestrationContext, Project project)
//             => orchestrationContext.CallActivityWithRetryAsync(nameof(ResourceProviderRegisterActivity), project);
//     }
// }
