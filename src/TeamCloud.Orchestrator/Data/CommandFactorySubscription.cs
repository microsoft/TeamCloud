/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using TeamCloud.Data;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestrator.Command;
using TeamCloud.Orchestrator.Utilities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Data
{
    public abstract class CommandFactorySubscription : DocumentSubscription
    {
        private static async Task<CloudQueue> GetCommandQueueAsync()
        {
            if (CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), out var storageAccount))
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

        protected CommandFactorySubscription()
        {
            commandQueueInstance = new AsyncLazy<CloudQueue>(() => GetCommandQueueAsync(), LazyThreadSafetyMode.PublicationOnly);
        }

        protected async Task EnqueueCommandAsync(ICommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            try
            {
                var commandQueue = await commandQueueInstance
                    .Value
                    .ConfigureAwait(false);

                var commandMessage = new CloudQueueMessage(TeamCloudSerialize.SerializeObject(command));

                await commandQueue
                    .AddMessageAsync(commandMessage)
                    .ConfigureAwait(false);
            }
            catch
            {
                commandQueueInstance.Reset();

                throw;
            }
        }
    }
}
