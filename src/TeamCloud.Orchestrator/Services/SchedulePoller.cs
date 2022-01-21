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

namespace TeamCloud.Orchestrator.Services;

public sealed class SchedulePoller
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IProjectRepository projectRepository;
    private readonly IScheduleRepository scheduleRepository;
    private readonly IAzureSessionService azureSessionService;
    private readonly IUserRepository userRepository;

    public SchedulePoller(IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IScheduleRepository scheduleRepository, IAzureSessionService azureSessionService, IUserRepository userRepository)
    {
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
        this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [FunctionName(nameof(SchedulePoller))]
    public async Task Run(
        [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timer,
        [Queue(CommandHandler.ProcessorQueue)] IAsyncCollector<ICommand> commandQueue,
        ILogger log)
    {
        if (timer is null)
            throw new ArgumentNullException(nameof(timer));

        var utcNow = DateTime.UtcNow;

        if (timer.IsPastDue)
        {
            log.LogInformation("Timer is running late!");
        }

        log.LogInformation($"Schedule Poller Timer trigger function executed at: {DateTime.Now}");


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


        var scheduleTasks = projects.SelectMany(p =>
            p.Select(pp => GetSchedules(pp.Id, (DayOfWeek)utcNow.DayOfWeek, utcNow.Hour, utcNow.Minute, 5))
        );

        var schedules = await Task.WhenAll(scheduleTasks)
            .ConfigureAwait(false);


        var userIds = schedules.SelectMany(st =>
            st.Select(sst => (org: sst.Organization, user: sst.Creator))
        ).Distinct();

        var usersTasks = userIds.Select(u => userRepository.GetAsync(u.org, u.user));

        var users = await Task.WhenAll(usersTasks)
            .ConfigureAwait(false);

        var commandQueueTasks = schedules.SelectMany(st =>
            st.Select(sst => commandQueue.AddAsync(new ScheduleRunCommand(users.FirstOrDefault(u => u.Id == sst.Creator), sst)))
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

    private async Task<List<Schedule>> GetSchedules(string project, DayOfWeek day, int hour, int minute, int interval)
    {
        var tasks = await scheduleRepository
            .ListAsync(project, day, hour, minute, interval)
            .ToListAsync()
            .ConfigureAwait(false);

        return tasks;
    }
}
