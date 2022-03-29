//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Orchestrator.Command.Handlers;
internal class OrganizationPortalUpdateCommandHandler : CommandHandler<OrganizationPortalUpdateCommand>
{
    private readonly IAzureResourceService azureResourceService;

    public OrganizationPortalUpdateCommandHandler(IAzureResourceService azureResourceService)
    {
        this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
    }

    public override bool Orchestration => false;

    public override async Task<ICommandResult> HandleAsync(OrganizationPortalUpdateCommand command, IAsyncCollector<ICommand> commandQueue, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        var commandResult = command.CreateResult();

        try
        {
            var appService = await azureResourceService
                .GetResourceAsync<AzureAppServiceResource>(command.Payload.PortalId, throwIfNotExists: true)
                .ConfigureAwait(false);

            await appService
                .UpdateContainerAsync()
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
