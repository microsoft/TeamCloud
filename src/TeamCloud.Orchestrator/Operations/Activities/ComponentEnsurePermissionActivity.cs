/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentEnsurePermissionActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IUserRepository userRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentEnsurePermissionActivity(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IUserRepository userRepository, IDeploymentScopeRepository deploymentScopeRepository, IComponentTemplateRepository componentTemplateRepository, IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentEnsurePermissionActivity))]
        [RetryOptions(3)]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var component = context.GetInput<Input>().Component;

            try
            {
                var task = component.Type switch
                {
                    ComponentType.Environment => HandleEnvironmentAsync(component, log),
                    _ => Task.CompletedTask
                };

                await task.ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                throw exc.AsSerializable();
            }

            return component;
        }

        private async Task<Dictionary<string, IEnumerable<Guid>>> GetRoleAssignmentsAsync(Component component, ILogger log)
        {
            var session = await azureResourceService.AzureSessionService
                .CreateSessionAsync()
                .ConfigureAwait(false);

            var template = await componentTemplateRepository
                .GetAsync(component.Organization, component.ProjectId, component.TemplateId)
                .ConfigureAwait(false);

            return await userRepository
                .ListAsync(component.Organization, component.ProjectId)
                .SelectAwait(async user => new KeyValuePair<string, IEnumerable<Guid>>(user.Id, await GetUserRoleDefinitionIdsAsync(user).ConfigureAwait(false)))
                .ToDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value)
                .ConfigureAwait(false);

            async Task<IEnumerable<Guid>> GetUserRoleDefinitionIdsAsync(User user)
            {
                var roleDefinitionIds = new HashSet<Guid>();

                if (template.Permissions?.Any() ?? false)
                {
                    if (user.IsOwner(component.ProjectId) && template.Permissions.TryGetValue(ProjectUserRole.Owner, out var ownerPermissions))
                    {
                        var tasks = ownerPermissions.Select(permission => ResolveRoleDefinitionIdAsync(permission));

                        roleDefinitionIds.UnionWith(await Task.WhenAll(tasks).ConfigureAwait(false));
                    }
                    else if (user.IsAdmin(component.ProjectId) && template.Permissions.TryGetValue(ProjectUserRole.Admin, out var adminPermissions))
                    {
                        var tasks = adminPermissions.Select(permission => ResolveRoleDefinitionIdAsync(permission));

                        roleDefinitionIds.UnionWith(await Task.WhenAll(tasks).ConfigureAwait(false));
                    }
                    else if (user.IsMember(component.ProjectId) && template.Permissions.TryGetValue(ProjectUserRole.Member, out var memberPermissions))
                    {
                        var tasks = memberPermissions.Select(permission => ResolveRoleDefinitionIdAsync(permission));

                        roleDefinitionIds.UnionWith(await Task.WhenAll(tasks).ConfigureAwait(false));
                    }
                }

                // strip out unresolved role defintion ids
                roleDefinitionIds.RemoveWhere(id => id == Guid.Empty);

                if (!roleDefinitionIds.Any())
                {
                    // if no role definition id was resolved use default
                    roleDefinitionIds.Add(AzureRoleDefinition.Reader);
                }

                return roleDefinitionIds;
            }

            async Task<Guid> ResolveRoleDefinitionIdAsync(string permission)
            {
                if (Guid.TryParse(permission, out var roleDefinitionId))
                    return roleDefinitionId;

                try
                {
                    var roleDefinition = await session.RoleDefinitions
                        .GetByScopeAndRoleNameAsync(component.ResourceId, permission)
                        .ConfigureAwait(false);

                    if (roleDefinition is null)
                        return Guid.Empty;

                    return Guid.Parse(roleDefinition.Id.Split('/').LastOrDefault());
                }
                catch (Exception exc)
                {
                    log.LogWarning(exc, $"Failed to resolve role definition by name '{permission}'");

                    return Guid.Empty;
                }
            }
        }

        private async Task HandleEnvironmentAsync(Component component, ILogger log)
        {
            if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                var roleAssignmentMap = await GetRoleAssignmentsAsync(component, log)
                    .ConfigureAwait(false);

                if (AzureResourceIdentifier.TryParse(component.IdentityId, out var identityResourceId))
                {
                    var session = await azureResourceService.AzureSessionService
                        .CreateSessionAsync(identityResourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    var identity = await session.Identities
                        .GetByIdAsync(identityResourceId.ToString())
                        .ConfigureAwait(false);

                    roleAssignmentMap
                        .Add(identity.PrincipalId, Enumerable.Repeat(AzureRoleDefinition.Contributor, 1));
                }

                if (string.IsNullOrEmpty(componentResourceId.ResourceGroup))
                {
                    var subscription = await azureResourceService
                        .GetSubscriptionAsync(componentResourceId.SubscriptionId, throwIfNotExists: true)
                        .ConfigureAwait(false);

                    await subscription
                        .SetRoleAssignmentsAsync(roleAssignmentMap)
                        .ConfigureAwait(false);
                }
                else
                {
                    var resourceGroup = await azureResourceService
                        .GetResourceGroupAsync(componentResourceId.SubscriptionId, componentResourceId.ResourceGroup, throwIfNotExists: true)
                        .ConfigureAwait(false);

                    await resourceGroup
                        .SetRoleAssignmentsAsync(roleAssignmentMap)
                        .ConfigureAwait(false);
                }
            }
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
