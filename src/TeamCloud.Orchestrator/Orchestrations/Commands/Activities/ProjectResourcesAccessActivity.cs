/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.KeyVault.Fluent.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Orchestrations.Commands.Activities
{
    public class ProjectResourcesAccessActivity
    {
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;
        private readonly IProjectsRepositoryReadOnly projectsRepository;

        public ProjectResourcesAccessActivity(IAzureSessionService azureSessionService, IAzureResourceService azureResourceService, IProjectsRepositoryReadOnly projectsRepository)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        [FunctionName(nameof(ProjectResourcesAccessActivity))]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var (projectId, principalId) = functionContext.GetInput<(Guid, Guid)>();

            var project = await projectsRepository
                .GetAsync(projectId)
                .ConfigureAwait(false);

            if (project.ResourceGroup?.ResourceGroupId is null)
            {
                throw new NullReferenceException($"Project {projectId} missing resource group information.");
            }
            else
            {
                var tasks = new Task[]
                {
                    EnsureResourceGroupAccessAsync(project, principalId),
                    EnsureKeyVaultAccessAsync(project, principalId)
                };

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task EnsureResourceGroupAccessAsync(Project project, Guid principalId)
        {
            var resourceGroup = await azureResourceService
                .GetResourceGroupAsync(project.ResourceGroup.SubscriptionId, project.ResourceGroup.ResourceGroupName, throwIfNotExists: true)
                .ConfigureAwait(false);

            await resourceGroup
                .AddRoleAssignmentAsync(principalId, AzureRoleDefinition.Contributor)
                .ConfigureAwait(false);
        }

        private async Task EnsureKeyVaultAccessAsync(Project project, Guid principalId)
        {
            var keyVault = await azureResourceService
                .GetResourceAsync<AzureKeyVaultResource>(project.KeyVault.VaultId, throwIfNotExists: true)
                .ConfigureAwait(false);

            var systemIdentity = await azureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            var tasks = new List<Task>();

            if (systemIdentity.ObjectId == principalId)
            {
                tasks.Add(keyVault.SetAllCertificatePermissionsAsync(principalId));
                tasks.Add(keyVault.SetAllKeyPermissionsAsync(principalId));
                tasks.Add(keyVault.SetAllSecretPermissionsAsync(principalId));
            }
            else
            {
                tasks.Add(keyVault.SetCertificatePermissionsAsync(principalId, CertificatePermissions.Get, CertificatePermissions.List));
                tasks.Add(keyVault.SetKeyPermissionsAsync(principalId, KeyPermissions.Get, KeyPermissions.List));
                tasks.Add(keyVault.SetSecretPermissionsAsync(principalId, SecretPermissions.Get, SecretPermissions.List));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

    }
}
