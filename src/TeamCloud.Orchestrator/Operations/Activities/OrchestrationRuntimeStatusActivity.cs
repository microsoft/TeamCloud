/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class OrchestrationRuntimeStatusActivity
    {
        [FunctionName(nameof(OrchestrationRuntimeStatusActivity))]
        public async Task<OrchestrationRuntimeStatus?> Run(
            [ActivityTrigger] IDurableActivityContext context,
            [DurableClient] IDurableClient client)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (client is null)
                throw new ArgumentNullException(nameof(client));

            var instanceId = context.GetInput<Input>().InstanceId;

            try
            {
                var status = await client
                    .GetStatusAsync(instanceId, showInput: false)
                    .ConfigureAwait(false);

                return status?.RuntimeStatus;
            }
            catch (Exception exc)
            {
                throw exc.AsSerializable();
            }
        }

        internal struct Input
        {
            public string InstanceId { get; set; }
        }
    }
}
