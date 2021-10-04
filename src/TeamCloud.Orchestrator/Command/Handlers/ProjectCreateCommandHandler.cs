/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Messaging;
using TeamCloud.Notification;
using TeamCloud.Orchestrator.Options;

namespace TeamCloud.Orchestrator.Command.Handlers
{
    public sealed class ProjectCreateCommandHandler : CommandHandler<ProjectCreateCommand>
    {
        private readonly IUserRepository userRepository;
        private readonly TeamCloudEndpointOptions endpointOptions;
        private readonly IAzureSessionService azureSessionService;
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;

        public ProjectCreateCommandHandler(IAzureSessionService azureSessionService, IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IUserRepository userRepository, TeamCloudEndpointOptions endpointOptions)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.endpointOptions = endpointOptions ?? throw new ArgumentNullException(nameof(endpointOptions));
        }

        public override bool Orchestration => false;

        public override async Task<ICommandResult> HandleAsync(ProjectCreateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            var commandResult = command.CreateResult();

            try
            {
                commandResult.Result = await projectRepository
                    .AddAsync(command.Payload)
                    .ConfigureAwait(false);

                await userRepository
                    .AddProjectMembershipAsync(command.User, commandResult.Result.Id, ProjectUserRole.Owner, new Dictionary<string, string>())
                    .ConfigureAwait(false);

                await commandQueue
                    .AddAsync(new ProjectDeployCommand(command.User, command.Payload))
                    .ConfigureAwait(false);

                var tenantId = await azureSessionService
                    .GetTenantIdAsync()
                    .ConfigureAwait(false);

                var message = NotificationMessage.Create<WelcomeMessage>(command.User);

                message.Merge(new WelcomeMessageData()
                {
                    Organization = await organizationRepository.GetAsync(tenantId.ToString(), commandResult.Result.Organization, expand: true).ConfigureAwait(false),
                    Project = commandResult.Result,
                    User = await userRepository.ExpandAsync(command.User).ConfigureAwait(false),
                    PortalUrl = endpointOptions.Portal
                });

                await commandQueue
                    .AddAsync(new NotificationSendMailCommand<WelcomeMessage>(command.User, message))
                    .ConfigureAwait(false);

                commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return commandResult;
        }
    }
}
