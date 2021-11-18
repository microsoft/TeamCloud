/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Orchestrator.Data;

namespace TeamCloud.Orchestrator.Command.Data
{
    public sealed class DocumentNotificationSubscription : CommandFactorySubscription
    {
        private readonly IAzureSessionService azureSessionService;

        public DocumentNotificationSubscription(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        private async Task<User> GetCommandUserAsync(Guid organizationId)
        {
            var identity = await azureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            return new User()
            {
                Id = identity.ObjectId.ToString(),
                Organization = organizationId.ToString()
            };
        }

        public override bool CanHandle(IContainerDocument containerDocument)
        {
            return true;
        }

        public override async Task HandleAsync(IContainerDocument containerDocument, DocumentSubscriptionEvent subscriptionEvent)
        {
            if (containerDocument is null)
                throw new ArgumentNullException(nameof(containerDocument));

            var broadcastCommandType = (subscriptionEvent switch
            {
                DocumentSubscriptionEvent.Create => typeof(BroadcastDocumentCreateCommand<>),
                DocumentSubscriptionEvent.Update => typeof(BroadcastDocumentUpdateCommand<>),
                DocumentSubscriptionEvent.Delete => typeof(BroadcastDocumentDeleteCommand<>),
                _ => null

            })?.MakeGenericType(containerDocument.GetType());

            if (broadcastCommandType is not null)
            {
                var commandUser = Guid.TryParse((containerDocument as IOrganizationContext)?.Organization, out var organizationId)
                    ? await GetCommandUserAsync(organizationId).ConfigureAwait(false)
                    : await GetCommandUserAsync(Guid.Empty).ConfigureAwait(false);

                var command = (ICommand)Activator
                    .CreateInstance(broadcastCommandType, new object[] { commandUser, containerDocument });

                await EnqueueCommandAsync(command)
                    .ConfigureAwait(false);
            }
        }
    }
}
