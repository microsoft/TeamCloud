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
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.Adapters
{
    public sealed class AdapterUpdateComponentActivity
    {
        private readonly IComponentRepository componentRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IEnumerable<IAdapter> adapters;

        public AdapterUpdateComponentActivity(IComponentRepository componentRepository, IDeploymentScopeRepository deploymentScopeRepository, IEnumerable<IAdapter> adapters)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.adapters = adapters ?? Enumerable.Empty<IAdapter>();
        }

        [FunctionName(nameof(AdapterUpdateComponentActivity))]
        [RetryOptions(3)]
        public async Task<Component> Run(
            [ActivityTrigger] IDurableActivityContext context,
            [Queue(CommandHandler.ProcessorQueue)] IAsyncCollector<ICommand> commandQueue,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var component = context.GetInput<Input>().Component;

            // ensure we deal with the latest version of the component
            // as adapters have the power to update it - so it could be
            // changed in case this is a retry

            component = await componentRepository
                .GetAsync(component.ProjectId, component.Id)
                .ConfigureAwait(false);

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId)
                .ConfigureAwait(false);

            if (deploymentScope is null)
                throw new ArgumentException("Deployment scope not found", nameof(context));

            if (!adapters.TryGetAdapter(deploymentScope.Type, out var adapter))
                throw new ArgumentException("Adapter for deployment scope not found", nameof(context));

            if (!await adapter.IsAuthorizedAsync(deploymentScope).ConfigureAwait(false))
                throw new ArgumentException("Adapter for deployment scope not authorized", nameof(context));

            try
            {
                component = await adapter
                    .UpdateComponentAsync(component, context.GetInput<Input>().User, new CommandCollector(commandQueue), log)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Adapter '{adapter.GetType().FullName}' failed to execute component {component}: {exc.Message}");

                throw exc.AsSerializable();
            }

            return component;
        }

        internal struct Input
        {
            public Component Component { get; set; }

            public User User { get; set; }
        }
    }
}
