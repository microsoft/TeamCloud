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

namespace TeamCloud.Orchestrator.Command.Data;

public sealed class DocumentNotificationSubscription : CommandFactorySubscription
{
    private readonly IAzureSessionService azureSessionService;

    public DocumentNotificationSubscription(IAzureSessionService azureSessionService)
    {
        this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
    }

    private async Task<User> GetCommandUserAsync(Guid organizationId, string organizationName)
    {
        var identity = await azureSessionService
            .GetIdentityAsync()
            .ConfigureAwait(false);

        return new User()
        {
            Id = identity.ObjectId.ToString(),
            Organization = organizationId.ToString(),
            OrganizationName = organizationName
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
            var organizationId = Guid.Empty;
            var organizationName = "none";

            var organizationContext = containerDocument as IOrganizationContext;
            if (organizationContext is not null && Guid.TryParse(organizationContext.Organization, out organizationId))
            {
                organizationName = organizationContext.OrganizationName;
            }
            else
            {
                var organization = containerDocument as Organization;
                if (organization is not null && Guid.TryParse(organization.Id, out organizationId))
                {
                    organizationName = organization.Slug;
                }
            }

            var commandUser = await GetCommandUserAsync(organizationId, organizationName);

            var command = (ICommand)Activator
                .CreateInstance(broadcastCommandType, new object[] { commandUser, containerDocument });

            await EnqueueCommandAsync(command)
                .ConfigureAwait(false);
        }
    }
}
