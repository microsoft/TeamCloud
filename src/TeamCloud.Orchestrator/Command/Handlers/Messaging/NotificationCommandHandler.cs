/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestrator.Options;

namespace TeamCloud.Orchestrator.Command.Handlers.Messaging
{
    public sealed class NotificationCommandHandler : CommandHandler,
        ICommandHandler<NotificationSendMailCommand>
    {
        private readonly ISmtpOptions smtpOptions;

        public NotificationCommandHandler(ISmtpOptions smtpOptions)
        {
            this.smtpOptions = smtpOptions;
        }

        public Task<ICommandResult> HandleAsync(NotificationSendMailCommand command, IAsyncCollector<ICommand> commandQueue, IDurableClient orchestrationClient, IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var commandResult = command.CreateResult();

            try
            {
                using var smtpClient = new SmtpClient(smtpOptions.Host, smtpOptions.Port)
                {
                    EnableSsl = smtpOptions.SSL,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpOptions.Username, smtpOptions.Password)
                };

                var mailMessage = new MailMessage();
                
            }
            catch (Exception exc)
            {
                commandResult.Errors.Add(exc);
            }

            return Task.FromResult<ICommandResult>(commandResult);
        }
    }
}
