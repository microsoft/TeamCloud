/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Text;
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
    public sealed class ComponentTaskMonitorActivity
    {
        private readonly IComponentTaskRepository componentTaskRepository;
        private readonly IAzureResourceService azureResourceService;

        public ComponentTaskMonitorActivity(IComponentTaskRepository componentTaskRepository, IAzureResourceService azureResourceService)
        {
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentTaskMonitorActivity))]
        [RetryOptions(3)]
        public async Task<ComponentTask> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            try
            {
                var componentTask = context.GetInput<Input>().ComponentTask;

                if (AzureResourceIdentifier.TryParse(componentTask.ResourceId, out var resourceId)
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
                        componentTask.ResourceState = ResourceState.Initializing;
                    }
                    else
                    {
                        //var lines = container.InstanceView.Events
                        //    .Where(e => e.LastTimestamp.HasValue)
                        //    .OrderBy(e => e.LastTimestamp)
                        //    .Select(e => $"{e.LastTimestamp.Value:yyyy-MM-dd hh:mm:ss}\t{e.Name}\t\t{e.Message}");

                        //if (lines.Any())
                        //    lines = lines.Append(string.Empty);

                        //var containerEvents = string.Join(Environment.NewLine, lines);
                        //var containerLog = default(string);

                        //try
                        //{
                        //    containerLog = await runner
                        //        .GetLogContentAsync(container.Name)
                        //        .ConfigureAwait(false);
                        //}
                        //catch
                        //{
                        //    containerLog = string.Empty;
                        //}

                        //if (string.IsNullOrEmpty(containerLog))
                        //{
                        //    containerLog = containerEvents;
                        //}
                        //else if (!(componentTask.Output?.StartsWith(containerEvents, StringComparison.Ordinal) ?? false))
                        //{
                        //    componentTask.Output = containerEvents;
                        //}

                        //componentTask.Output = MergeOutput(componentTask.Output, Regex.Replace(containerLog, @"(?<!\r)\n", Environment.NewLine, RegexOptions.Compiled));

                        if (container.InstanceView.CurrentState != null)
                        {
                            componentTask.ResourceState = ResourceState.Provisioning;
                            componentTask.ExitCode = container.InstanceView.CurrentState.ExitCode;
                            componentTask.Started = container.InstanceView.CurrentState.StartTime;
                            componentTask.Finished = container.InstanceView.CurrentState.FinishTime;

                            if (componentTask.ExitCode.HasValue)
                            {
                                componentTask.ResourceState = componentTask.ExitCode == 0
                                    ? ResourceState.Succeeded   // ExitCode indicates successful provisioning
                                    : ResourceState.Failed;     // ExitCode indicates failed provisioning
                            }
                            else if (container.InstanceView.CurrentState.State?.Equals("Terminated", StringComparison.OrdinalIgnoreCase) ?? false)
                            {
                                // container instance was terminated without exit code
                                componentTask.ResourceState = ResourceState.Failed;
                            }
                            else if (componentTask.Started.GetValueOrDefault(DateTime.UtcNow) < DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(30)))
                            {
                                // container instance needs to be terminated
                                await runner.StopAsync().ConfigureAwait(false);
                            }
                        }
                    }

                    componentTask = await componentTaskRepository
                        .SetAsync(componentTask)
                        .ConfigureAwait(false);

                    if (componentTask.ResourceState.IsFinal())
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

                return componentTask;

                // static string MergeOutput(string source, string append)
                // {
                //     if (string.IsNullOrEmpty(source))
                //         return append;

                //     if (string.IsNullOrEmpty(append))
                //         return source;

                //     if (source == append)
                //         return source;

                //     var builder = new StringBuilder(source);

                //     for (int i = 0; i < builder.Length; i++)
                //     {
                //         if (append.StartsWith(source.Substring(i), StringComparison.Ordinal))
                //             return builder.Remove(i, builder.Length - i).Append(append).ToString();
                //     }

                //     return builder.Append(append).ToString();
                // }
            }
            catch (Exception exc)
            {
                throw exc.AsSerializable();
            }
        }

        internal struct Input
        {
            public ComponentTask ComponentTask { get; set; }
        }
    }
}
