/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Command.Activities.Adapters
{
    public sealed class AdapterCreateComponentActivity
    {
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IEnumerable<IAdapter> adapters;

        public AdapterCreateComponentActivity(IDeploymentScopeRepository deploymentScopeRepository, IEnumerable<IAdapter> adapters)
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.adapters = adapters ?? Enumerable.Empty<IAdapter>();
        }

        [FunctionName(nameof(AdapterCreateComponentActivity))]
        [RetryOptions(3)]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var component = context.GetInput<Input>().Component;

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId)
                .ConfigureAwait(false);

            var adapter = adapters
                .FirstOrDefault(a => a.Type == deploymentScope.Type);

            if (adapter is null)
                throw new ArgumentException("Adapter for deployment scope not found", nameof(context));

            if (!await adapter.IsAuthorizedAsync(deploymentScope).ConfigureAwait(false))
                throw new ArgumentException("Adapter for deployment scope not authorized", nameof(context));

            return await adapter
                .CreateComponentAsync(component)
                .ConfigureAwait(false);
        }

        internal struct Input
        {
            public Component Component { get; set; }
        }
    }
}
