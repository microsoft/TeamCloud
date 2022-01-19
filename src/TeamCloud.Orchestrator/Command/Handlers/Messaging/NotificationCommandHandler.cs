/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Notification;
using TeamCloud.Notification.Smtp;

namespace TeamCloud.Orchestrator.Command.Handlers.Messaging;

public sealed class NotificationCommandHandler : CommandHandler
{
    private readonly INotificationSmtpSender notificationSmtpSender;

    public NotificationCommandHandler(INotificationSmtpSender notificationSmtpSender = null)
    {
        this.notificationSmtpSender = notificationSmtpSender;
    }

    public override bool Orchestration => false;

    public override bool CanHandle(ICommand command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        return command.Payload is INotificationMessage
            && command.GetType().IsGenericType
            && command.GetType().GetGenericTypeDefinition() == typeof(NotificationSendMailCommand<>);
    }

    public override async Task<ICommandResult> HandleAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        var commandResult = command.CreateResult();

        try
        {
            if (CanHandle(command))
            {
                var commandType = command.GetType().GetGenericTypeDefinition();

                switch (true)
                {
                    case var _ when commandType == typeof(NotificationSendMailCommand<>):
                        commandResult = await SendMailAsync(command, commandQueue, orchestrationClient, orchestrationContext, log).ConfigureAwait(false);
                        break;

                    default:
                        commandResult.RuntimeStatus = CommandRuntimeStatus.Completed;
                        break;
                }
            }
            else
            {
                throw new NotImplementedException($"Missing orchestrator command handler implementation ICommandHandler<{command.GetTypeName(prettyPrint: true)}> at {GetType()}");
            }
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }

    private async Task<ICommandResult> SendMailAsync(ICommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
    {
        var commandResult = command.CreateResult();

        try
        {
            if (notificationSmtpSender is null)
            {
                commandResult.Errors.Add(new CommandError()
                {
                    Message = "SMTP notification sender not configured",
                    Severity = CommandErrorSeverity.Warning
                });
            }
            else
            {
                await notificationSmtpSender
                    .SendMessageAsync((INotificationMessage)command.Payload)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception exc)
        {
            commandResult.Errors.Add(exc);
        }

        return commandResult;
    }
}
