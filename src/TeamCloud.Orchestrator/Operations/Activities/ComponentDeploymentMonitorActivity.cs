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
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentDeploymentMonitorActivity
    {
        private readonly IComponentDeploymentRepository componentDeploymentRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentDeploymentMonitorActivity(IComponentDeploymentRepository componentDeploymentRepository, IAzureResourceService azureResourceService)
        {
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentDeploymentMonitorActivity))]
        [RetryOptions(3)]
        public async Task<ComponentDeployment> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            try
            {
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

                    var container = runner.Containers
                        .SingleOrDefault()
                        .Value;

                    if (container?.InstanceView is null)
                    {
                        componentDeployment.ResourceState = ResourceState.Initializing;
                    }
                    else
                    {
                        var lines = container.InstanceView.Events
                            .Where(e => e.LastTimestamp.HasValue)
                            .OrderBy(e => e.LastTimestamp)
                            .Select(e => $"{e.LastTimestamp.Value:yyyy-MM-dd hh:mm:ss}\t{e.Name}\t\t{e.Message}");

                        if (lines.Any())
                            lines = lines.Append(string.Empty);

                        var containerEvents = string.Join(Environment.NewLine, lines);
                        var containerLog = default(string);

                        try
                        {
                            containerLog = await runner
                                .GetLogContentAsync(container.Name)
                                .ConfigureAwait(false);
                        }
                        catch
                        {
                            containerLog = string.Empty;
                        }

                        if (string.IsNullOrEmpty(containerLog))
                        {
                            containerLog = containerEvents;
                        }
                        else if (!(componentDeployment.Output?.StartsWith(containerEvents, StringComparison.Ordinal) ?? false))
                        {
                            componentDeployment.Output = containerEvents;
                        }

                        componentDeployment.Output = MergeOutput(componentDeployment.Output, Regex.Replace(containerLog, @"(?<!\r)\n", Environment.NewLine, RegexOptions.Compiled));

                        if (container.InstanceView.CurrentState != null)
                        {
                            componentDeployment.ExitCode = container.InstanceView.CurrentState.ExitCode;
                            componentDeployment.Started = container.InstanceView.CurrentState.StartTime;
                            componentDeployment.Finished = container.InstanceView.CurrentState.FinishTime;

                            if (componentDeployment.ExitCode.HasValue)
                            {
                                componentDeployment.ResourceState = componentDeployment.ExitCode == 0
                                    ? ResourceState.Succeeded   // ExitCode indicates successful provisioning
                                    : ResourceState.Failed;     // ExitCode indicates failed provisioning
                            }
                            else if (container.InstanceView.CurrentState.State?.Equals("Terminated", StringComparison.OrdinalIgnoreCase) ?? false)
                            {
                                // container instance was terminated without exit code
                                componentDeployment.ResourceState = ResourceState.Failed;
                            }
                            else if (componentDeployment.Started.GetValueOrDefault(DateTime.UtcNow) < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30)))
                            {
                                // container instance needs to be terminated
                                await runner.StopAsync().ConfigureAwait(false);
                            }
                        }
                    }

                    componentDeployment = await componentDeploymentRepository
                        .SetAsync(componentDeployment)
                        .ConfigureAwait(false);

                    if (componentDeployment.ResourceState.IsFinal())
                    {
                        // we left the provisioning state - so its time to clean up

                        session ??= await azureResourceService.AzureSessionService
                            .CreateSessionAsync(resourceId.SubscriptionId)
                            .ConfigureAwait(false);

                        await session.ContainerGroups
                            .DeleteByIdAsync(resourceId.ToString())
                            .ConfigureAwait(false);
                    }
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
            catch (Exception exc)
            {
                throw exc.AsSerializable();
            }
        }

        internal struct Input
        {
            public ComponentDeployment ComponentDeployment { get; set; }
        }
    }
}
