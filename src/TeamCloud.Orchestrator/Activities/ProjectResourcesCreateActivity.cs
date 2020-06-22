/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Azure.Deployment;
using TeamCloud.Data;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Templates;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectResourcesCreateActivity
    {
        private readonly IAzureDeploymentService azureDeploymentService;
        private readonly IAzureSessionService azureSessionService;
        private readonly IProvidersRepository providersRepository;

        public ProjectResourcesCreateActivity(IAzureDeploymentService azureDeploymentService, IAzureSessionService azureSessionService, IProvidersRepository providersRepository)
        {
            this.azureDeploymentService = azureDeploymentService ?? throw new ArgumentNullException(nameof(azureDeploymentService));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
        }

        private async Task<string> GetOrchestratorIdentityAsync()
        {
            var identity = await azureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            return identity.ObjectId.ToString();
        }

        private async Task<string[]> GetProviderIdentitiesAsync(Project project)
        {
            var providers = await providersRepository
                .ListAsync(project.Type.Providers.Select(p => p.Id))
                .ToListAsync()
                .ConfigureAwait(false);

            return project.Type.Providers
                .Select(pr => providers.Single(p => p.Id.Equals(pr.Id, StringComparison.Ordinal)))
                .Where(p => p.PrincipalId.HasValue && p.Registered.HasValue)
                .Select(p => p.PrincipalId.Value.ToString())
                .Distinct().ToArray();
        }

        [FunctionName(nameof(ProjectResourcesCreateActivity))]
        [RetryOptions(3)]
        public async Task<string> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (project, subscriptionId) = functionContext.GetInput<(Project, Guid)>();

            // if the provided project instance is already assigned
            // to a subscription we use this one instead of the provided
            // one to make our activity idempotent (we always go to the
            // same subscription). the same is valid for the projects
            // resource group name and location (passed as templated params).

            subscriptionId = project.ResourceGroup?.SubscriptionId ?? subscriptionId;

            var template = new CreateProjectTemplate();

            template.Parameters["projectId"] = project.Id;
            template.Parameters["projectName"] = project.Name;
            template.Parameters["projectPrefix"] = project.Type.ResourceGroupNamePrefix; // if null - the template uses its default value
            template.Parameters["resourceGroupName"] = project.ResourceGroup?.Name; // if null - the template generates a unique name
            template.Parameters["resourceGroupLocation"] = project.ResourceGroup?.Region ?? project.Type.Region;
            template.Parameters["orchestratorIdentity"] = await GetOrchestratorIdentityAsync().ConfigureAwait(false);
            template.Parameters["providerIdentities"] = await GetProviderIdentitiesAsync(project).ConfigureAwait(false);

            //template.Parameters["eventGridLocation"] = location;
            //template.Parameters["eventGridEndpoint"] = await EventTrigger.GetUrlAsync().ConfigureAwait(false);

            try
            {
                var deployment = await azureDeploymentService
                    .DeploySubscriptionTemplateAsync(template, subscriptionId, project.Type.Region)
                    .ConfigureAwait(false);

                return deployment.ResourceId;
            }
            catch (Exception exc) when (!exc.IsSerializable(out var serializableException))
            {
                log.LogError(exc, $"Activity '{nameof(ProjectResourcesCreateActivity)} failed: {exc.Message}");

                throw serializableException;
            }
        }
    }
}
