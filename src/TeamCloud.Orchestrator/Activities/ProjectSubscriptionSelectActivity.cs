/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class ProjectSubscriptionSelectActivity
    {
        private readonly IProjectTypesRepository projectTypesRepository;
        private readonly IAzureResourceService azureResourceService;

        public ProjectSubscriptionSelectActivity(IProjectTypesRepository projectTypesRepository, IAzureResourceService azureResourceService)
        {
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ProjectSubscriptionSelectActivity))]
        public async Task<Guid> RunActivity(
            [ActivityTrigger] ProjectDocument project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            var subscriptionCapacityTasks = project.Type.Subscriptions
                .Select(subscriptionId => GetSubscriptionCapacityAsync(project.Type, subscriptionId));

            IEnumerable<(Guid subscription, int capacity)> subscriptionCapacity = await Task
                .WhenAll(subscriptionCapacityTasks)
                .ConfigureAwait(false);

            var (subscription, capacity) = subscriptionCapacity
                .OrderByDescending((sub) => sub.capacity)
                .First();

            if (capacity == 0)
                throw new NotSupportedException($"All subscriptions reached their maximum capacity of {project.Type.SubscriptionCapacity} project of type {project.Type.Id}.");

            return subscription;
        }

        private async Task<(Guid, int)> GetSubscriptionCapacityAsync(ProjectTypeDocument projectType, Guid subscriptionId)
        {
            var subscription = await azureResourceService
                .GetSubscriptionAsync(subscriptionId)
                .ConfigureAwait(false);

            if (subscription is null)
                return (subscriptionId, 0);

            var identity = await azureResourceService.AzureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            var hasOwnership = await subscription
                .HasRoleAssignmentAsync(identity.ObjectId.ToString(), AzureRoleDefinition.Owner)
                .ConfigureAwait(false);

            if (hasOwnership)
            {
                var instanceCount = await projectTypesRepository
                    .GetInstanceCountAsync(projectType.Id, subscriptionId)
                    .ConfigureAwait(false);

                return (subscriptionId, Math.Max(projectType.SubscriptionCapacity - instanceCount, 0));
            }

            return (subscriptionId, 0);
        }
    }
}
