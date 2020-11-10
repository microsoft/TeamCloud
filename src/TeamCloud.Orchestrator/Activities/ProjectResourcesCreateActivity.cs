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
// using TeamCloud.Azure;
// using TeamCloud.Azure.Deployment;
// using TeamCloud.Data;
// using TeamCloud.Model.Data;
// using TeamCloud.Orchestration;
// using TeamCloud.Orchestrator.Templates;
// using TeamCloud.Serialization;

// namespace TeamCloud.Orchestrator.Activities
// {
//     public class ProjectResourcesCreateActivity
//     {
//         private readonly IAzureDeploymentService azureDeploymentService;
//         private readonly IAzureSessionService azureSessionService;
//         private readonly IProviderRepository providerRepository;

//         public ProjectResourcesCreateActivity(IAzureDeploymentService azureDeploymentService, IAzureSessionService azureSessionService, IProviderRepository providerRepository)
//         {
//             this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
//             this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
//             this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
//         }

//         private async Task<string> GetOrchestratorIdentityAsync()
//         {
//             var identity = await azureSessionService
//                 .GetIdentityAsync()
//                 .ConfigureAwait(false);

//             return identity.ObjectId.ToString();
//         }

//         private async Task<string[]> GetProviderIdentitiesAsync(Project project)
//         {
//             var providers = await providerRepository
//                 .ListAsync(project.Type.Providers.Select(p => p.Id))
//                 .ToListAsync()
//                 .ConfigureAwait(false);

//             return project.Type.Providers
//                 .Select(pr => providers.Single(p => p.Id.Equals(pr.Id, StringComparison.Ordinal)))
//                 .Where(p => p.PrincipalId.HasValue && p.Registered.HasValue)
//                 .Select(p => p.PrincipalId.Value.ToString())
//                 .Distinct().ToArray();
//         }

//         [FunctionName(nameof(ProjectResourcesCreateActivity))]
//         [RetryOptions(3)]
//         public async Task<string> RunActivity(
//             [ActivityTrigger] IDurableActivityContext activityContext,
//             ILogger log)
//         {
//             if (activityContext is null)
//                 throw new ArgumentNullException(nameof(activityContext));

//             var functionInput = activityContext.GetInput<Input>();

//             // if the provided project instance is already assigned
//             // to a subscription we use this one instead of the provided
//             // one to make our activity idempotent (we always go to the
//             // same subscription). the same is valid for the projects
//             // resource group name and location (passed as templated params).

//             functionInput.SubscriptionId = functionInput.Project.ResourceGroup?.SubscriptionId ?? functionInput.SubscriptionId;

//             var template = new CreateProjectTemplate();

//             template.Parameters["projectId"] = functionInput.Project.Id;
//             template.Parameters["projectName"] = functionInput.Project.Name;
//             template.Parameters["projectPrefix"] = functionInput.Project.Type.ResourceGroupNamePrefix; // if null - the template uses its default value
//             template.Parameters["resourceGroupName"] = functionInput.Project.ResourceGroup?.Name; // if null - the template generates a unique name
//             template.Parameters["resourceGroupLocation"] = functionInput.Project.ResourceGroup?.Region ?? functionInput.Project.Type.Region;
//             template.Parameters["providerIdentities"] = await GetProviderIdentitiesAsync(functionInput.Project).ConfigureAwait(false);

//             //template.Parameters["eventGridLocation"] = location;
//             //template.Parameters["eventGridEndpoint"] = await EventTrigger.GetUrlAsync().ConfigureAwait(false);

//             try
//             {
//                 var deployment = await azureDeploymentService
//                     .DeploySubscriptionTemplateAsync(template, functionInput.SubscriptionId, functionInput.Project.Type.Region)
//                     .ConfigureAwait(false);

//                 return deployment.ResourceId;
//             }
//             catch (Exception exc) when (!exc.IsSerializable(out var serializableException))
//             {
//                 log.LogError(exc, $"Activity '{nameof(ProjectResourcesCreateActivity)} failed: {exc.Message}");

//                 throw serializableException;
//             }
//         }

//         internal struct Input
//         {
//             public Project Project { get; set; }

//             public Guid SubscriptionId { get; set; }
//         }
//     }
// }
