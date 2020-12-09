using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
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
            var componentDeploymentUpdated = false;

            if (AzureResourceIdentifier.TryParse(componentDeployment.ResourceId, out var resourceId))
            {
                IAzure session = null;

                if (await azureResourceService.ExistsResourceAsync(resourceId.ToString()).ConfigureAwait(false))
                {
                    session ??= await azureResourceService.AzureSessionService
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
                    componentDeploymentUpdated = true;
                }

                if (componentDeployment.ExitCode.HasValue)
                {
                    componentDeployment.ResourceState = componentDeployment.ExitCode == 0
                        ? ResourceState.Succeeded   // ExitCode indicates successful provisioning
                        : ResourceState.Failed;     // ExitCode indicates failed provisioning
                }
                else if (componentDeployment.Finished.HasValue)
                {
                    // the container instance finished but didn't return an exit code
                    // we assume something went wrong and set a failed resource state
                    componentDeployment.ResourceState = ResourceState.Failed;
                }
                else if (componentDeploymentUpdated)
                {
                    // the component deployment was update with the status information
                    // provided by our runner - so we must be in a provisioning state
                    componentDeployment.ResourceState = ResourceState.Provisioning;
                }
                else
                {
                    // the component deployment wasn't updated - this usually indicates
                    // that our runner was deleted by some other process or users
                    componentDeployment.ResourceState = ResourceState.Failed;
                }

                if (componentDeployment.ResourceState != ResourceState.Provisioning)
                {
                    // we left the provisioning state - so its time to clean up

                    session ??= await azureResourceService.AzureSessionService
                        .CreateSessionAsync(resourceId.SubscriptionId)
                        .ConfigureAwait(false);

                    await session.ContainerGroups
                        .DeleteByIdAsync(resourceId.ToString())
                        .ConfigureAwait(false);
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
