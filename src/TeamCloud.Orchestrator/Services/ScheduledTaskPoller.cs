/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestrator.Command;
using DayOfWeek = TeamCloud.Model.Data.DayOfWeek;

namespace TeamCloud.Orchestrator.Services
{
    public sealed class ScheduledTaskPoller
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IScheduledTaskRepository scheduledTaskRepository;
        private readonly IAzureSessionService azureSessionService;
        private readonly IUserRepository userRepository;

        public ScheduledTaskPoller(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IScheduledTaskRepository scheduledTaskRepository, IAzureSessionService azureSessionService, IUserRepository userRepository)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.scheduledTaskRepository = scheduledTaskRepository ?? throw new ArgumentNullException(nameof(scheduledTaskRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [FunctionName(nameof(ScheduledTaskPoller))]
        public async Task Run([
            TimerTrigger("0 */5 * * * *")] TimerInfo timer,
            [Queue(ICommandHandler.ProcessorQueue)] IAsyncCollector<ICommand> commandQueue,
            ILogger log)
        {
            if (timer is null)
                throw new ArgumentNullException(nameof(timer));

            var utcNow = DateTime.UtcNow;

            if (timer.IsPastDue)
            {
                log.LogInformation("Timer is running late!");
            }

            log.LogInformation($"Scheduled Task Poller Timer trigger function executed at: {DateTime.Now}");


            var tenantId = await azureSessionService
                .GetTenantIdAsync()
                .ConfigureAwait(false);


            var orgs = await organizationRepository
                .ListAsync(tenantId.ToString())
                .ToListAsync()
                .ConfigureAwait(false);

            var projectsTasks = orgs.Select(o => GetProjects(o.Id));

            var projects = await Task.WhenAll(projectsTasks)
                .ConfigureAwait(false);


            var scheduledTasksTasks = projects.SelectMany(p =>
                p.Select(pp => GetScheduledTasks(pp.Id, (DayOfWeek)utcNow.DayOfWeek, utcNow.Hour, utcNow.Minute, 5))
            );

            var scheduledTasks = await Task.WhenAll(scheduledTasksTasks)
                .ConfigureAwait(false);


            var userIds = scheduledTasks.SelectMany(st =>
                st.Select(sst => (org: sst.Organization, user: sst.Creator))
            ).Distinct();

            var usersTasks = userIds.Select(u => userRepository.GetAsync(u.org, u.user));

            var users = await Task.WhenAll(usersTasks)
                .ConfigureAwait(false);

            var commandQueueTasks = scheduledTasks.SelectMany(st =>
                st.Select(sst => commandQueue.AddAsync(new ScheduledTaskRunCommand(users.FirstOrDefault(u => u.Id == sst.Creator), sst)))
            );

            await Task.WhenAll(commandQueueTasks)
                .ConfigureAwait(false);
        }

        private async Task<List<Project>> GetProjects(string org)
        {
            var projects = await projectRepository
                .ListAsync(org)
                .ToListAsync()
                .ConfigureAwait(false);

            return projects;
        }

        private async Task<List<ScheduledTask>> GetScheduledTasks(string project, DayOfWeek day, int hour, int minute, int interval)
        {
            var tasks = await scheduledTaskRepository
                .ListAsync(project, day, hour, minute, interval)
                .ToListAsync()
                .ConfigureAwait(false);

            return tasks;
        }
    }
}
