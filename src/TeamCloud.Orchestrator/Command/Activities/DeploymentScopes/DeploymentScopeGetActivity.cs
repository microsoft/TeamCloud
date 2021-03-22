/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Command.Activities.DeploymentScopes
{
    public sealed class DeploymentScopeGetActivity
    {
        private readonly IDeploymentScopeRepository deploymentScopeRepository;

        public DeploymentScopeGetActivity(IDeploymentScopeRepository deploymentScopeRepository)
        {
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        }

        [FunctionName(nameof(DeploymentScopeGetActivity))]
        public async Task<DeploymentScope> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.GetInput<Input>();

            var deploymentScope = await (string.IsNullOrEmpty(input.Id)
                ? deploymentScopeRepository.GetDefaultAsync(input.Organization).ConfigureAwait(false)
                : deploymentScopeRepository.GetAsync(input.Organization, input.Id).ConfigureAwait(false));

            return deploymentScope;
        }

        internal struct Input
        {
            public string Id { get; set; }

            public string Organization { get; set; }
        }
    }
}
