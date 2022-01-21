/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using TeamCloud.Data;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Orchestrator.Command;
using TeamCloud.Orchestrator.Utilities;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Data;

public abstract class CommandFactorySubscription : DocumentSubscription
{
    private static async Task<QueueClient> GetCommandQueueAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        if (connectionString is not null)
        {
            try
            {
                var queue = new QueueClient(connectionString, CommandHandler.ProcessorQueue);

                await queue.CreateIfNotExistsAsync()
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

    private readonly AsyncLazy<QueueClient> commandQueueInstance;

    protected CommandFactorySubscription()
    {
        commandQueueInstance = new AsyncLazy<QueueClient>(() => GetCommandQueueAsync(), LazyThreadSafetyMode.PublicationOnly);
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

            var commandMessage = TeamCloudSerialize.SerializeObject(command);

            await commandQueue
                .SendMessageAsync(commandMessage)
                .ConfigureAwait(false);
        }
        catch
        {
            commandQueueInstance.Reset();

            throw;
        }
    }
}
