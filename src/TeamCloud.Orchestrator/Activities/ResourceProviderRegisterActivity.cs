/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Activities
{
    public class ResourceProviderRegisterActivity
    {
        private readonly IProvidersRepository providersRepository;
        private readonly IAzureResourceService azureResourceService;

        public ResourceProviderRegisterActivity(IProvidersRepository providersRepository, IAzureResourceService azureResourceService)
        {
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ResourceProviderRegisterActivity)), RetryOptions(3)]
        public async Task RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext,
            ILogger log)
        {
            if (functionContext is null)
                throw new ArgumentNullException(nameof(functionContext));

            var project = functionContext.GetInput<Project>();

            try
            {
                var providers = await Task.WhenAll(project.Type.Providers
                        .Select(p => providersRepository.GetAsync(p.Id)))
                    .ConfigureAwait(false);

                var resourceProviderNamespaces = providers
                    .Where(p => p.ResourceProviders?.Any() ?? false)
                    .SelectMany(p => p.ResourceProviders)
                    .Distinct();

                if (resourceProviderNamespaces.Any())
                    await azureResourceService
                        .RegisterProvidersAsync(project.ResourceGroup.SubscriptionId, resourceProviderNamespaces)
                        .ConfigureAwait(false);

            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Activity '{nameof(ResourceProviderRegisterActivity)} failed: {exc.Message}");

                throw exc.AsSerializable();
            }
        }
    }

    internal static class ResourceProviderRegisterExtension
    {
        public static Task RegisterResourceProvidersAsync(this IDurableOrchestrationContext functionContext, Project project)
            => functionContext.CallActivityWithRetryAsync(nameof(ResourceProviderRegisterActivity), project);
    }
}
