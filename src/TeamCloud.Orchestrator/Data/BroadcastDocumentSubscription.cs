using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Orchestrator.Utilities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command
{
    public sealed class BroadcastDocumentSubscription : DocumentSubscription
    {
        private static string ConnectionString => Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        private static async Task<CloudQueue> GetCommandQueueAsync()
        {
            if (CloudStorageAccount.TryParse(ConnectionString, out var storageAccount))
            {
                try
                {
                    var queue = storageAccount
                        .CreateCloudQueueClient()
                        .GetQueueReference(ICommandHandler.ProcessorQueue);

                    _ = await queue
                        .CreateIfNotExistsAsync()
                        .ConfigureAwait(false);

                    return queue;
                }
                catch
                {
                    throw;
                }
            }

            return null;
        }

        private readonly AsyncLazy<CloudQueue> commandQueueInstance;
        private readonly IAzureSessionService azureSessionService;

        public BroadcastDocumentSubscription(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));

            commandQueueInstance = new AsyncLazy<CloudQueue>(() => GetCommandQueueAsync(), LazyThreadSafetyMode.PublicationOnly);
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

            try
            {
                var commandQueue = await commandQueueInstance
                    .Value
                    .ConfigureAwait(false);

                var commandUser = Guid.TryParse((containerDocument as IOrganizationContext)?.Organization, out var organizationId)
                    ? await GetCommandUserAsync(organizationId).ConfigureAwait(false)
                    : await GetCommandUserAsync(Guid.Empty).ConfigureAwait(false);


                if (commandQueue != null)
                {
                    var broadcastCommandType = (subscriptionEvent switch
                    {
                        DocumentSubscriptionEvent.Create => typeof(BroadcastDocumentCreateCommand<>),
                        DocumentSubscriptionEvent.Update => typeof(BroadcastDocumentUpdateCommand<>),
                        DocumentSubscriptionEvent.Delete => typeof(BroadcastDocumentDeleteCommand<>),
                        _ => null

                    })?.MakeGenericType(containerDocument.GetType());

                    if (broadcastCommandType != null)
                    {
                        var broadcastCommand = Activator.CreateInstance(broadcastCommandType, new object[] { commandUser, containerDocument });
                        var broadcastMessage = new CloudQueueMessage(TeamCloudSerialize.SerializeObject(broadcastCommand));

                        await commandQueue
                            .AddMessageAsync(broadcastMessage)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                commandQueueInstance.Reset();
            }
        }
    }
}
