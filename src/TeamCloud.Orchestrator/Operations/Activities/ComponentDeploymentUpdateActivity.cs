using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentDeploymentUpdateActivity
    {
        private readonly IComponentDeploymentRepository componentDeploymentRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentDeploymentUpdateActivity(IComponentDeploymentRepository componentDeploymentRepository, IAzureResourceService azureResourceService)
        {
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentDeploymentUpdateActivity))]
        public async Task<ComponentDeployment> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var componentDeployment = context.GetInput<Input>().ComponentDeployment;

            if (AzureResourceIdentifier.TryParse(componentDeployment.ResourceId, out var resourceId)
                && await azureResourceService.ExistsResourceAsync(resourceId.ToString()).ConfigureAwait(false))
            {
                var session = await azureResourceService.AzureSessionService
                    .CreateSessionAsync(resourceId.SubscriptionId)
                    .ConfigureAwait(false);

                var runner = await session.ContainerGroups
                    .GetByIdAsync(resourceId.ToString())
                    .ConfigureAwait(false);

                // there must be only one runner container
                var container = runner.Containers.SingleOrDefault().Value;

                if (container is null)
                {
                    componentDeployment.ResourceState = ResourceState.Initializing;

                    return await componentDeploymentRepository
                        .SetAsync(componentDeployment)
                        .ConfigureAwait(false); ;
                }

                var containerLog = default(string);

                try
                {
                    containerLog = await runner
                        .GetLogContentAsync(container.Name)
                        .ConfigureAwait(false);
                }
                catch
                {
                    // swallow 
                }

                if (string.IsNullOrEmpty(containerLog))
                {
                    componentDeployment.ResourceState = ResourceState.Provisioning;

                    return await componentDeploymentRepository
                        .SetAsync(componentDeployment)
                        .ConfigureAwait(false); ;
                }

                componentDeployment.Output = MergeOutput(componentDeployment.Output, Regex.Replace(containerLog, @"(?<!\r)\n", Environment.NewLine, RegexOptions.Compiled));
                componentDeployment.ExitCode = container.InstanceView.CurrentState.ExitCode;
                componentDeployment.Started = container.InstanceView.CurrentState.StartTime;
                componentDeployment.Finished = container.InstanceView.CurrentState.FinishTime;

                if (componentDeployment.ExitCode.HasValue)
                {
                    componentDeployment.ResourceState = componentDeployment.ExitCode == 0
                        ? ResourceState.Succeeded   // ExitCode indicates successful provisioning
                        : ResourceState.Failed;     // ExitCode indicates failed provisioning

                    componentDeployment.ResourceId = null;

                    await session.ContainerGroups
                        .DeleteByIdAsync(resourceId.ToString())
                        .ConfigureAwait(false);
                }
                else
                {
                    componentDeployment.ResourceState = ResourceState.Provisioning;
                }

                componentDeployment = await componentDeploymentRepository
                    .SetAsync(componentDeployment)
                    .ConfigureAwait(false);
            }

            return componentDeployment;

            static string MergeOutput(string source, string append)
            {
                if (string.IsNullOrEmpty(source))
                    return append;

                if (string.IsNullOrEmpty(append))
                    return source;

                if (source == append)
                    return source;

                var builder = new StringBuilder(source);

                for (int i = 0; i < builder.Length; i++)
                {
                    if (append.StartsWith(source.Substring(i), StringComparison.Ordinal))
                        return builder.Remove(i, builder.Length - i).Append(append).ToString();
                }

                return builder.Append(append).ToString();
            }

        }

        internal struct Input
        {
            public ComponentDeployment ComponentDeployment { get; set; }
        }
    }
}
