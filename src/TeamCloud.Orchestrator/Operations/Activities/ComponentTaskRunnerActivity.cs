using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Azure;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentTaskRunnerActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IComponentTaskRepository componentTaskRepository;
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;

        public ComponentTaskRunnerActivity(IOrganizationRepository organizationRepository,
                                       IProjectRepository projectRepository,
                                       IComponentRepository componentRepository,
                                       IComponentTemplateRepository componentTemplateRepository,
                                       IComponentTaskRepository componentTaskRepository,
                                       IAzureSessionService azureSessionService,
                                       IAzureResourceService azureResourceService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        }

        [FunctionName(nameof(ComponentTaskRunnerActivity))]
        [RetryOptions(3)]
        public async Task<ComponentTask> Run(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var componentTask = context.GetInput<Input>().ComponentTask;

            try
            {
                var component = await componentRepository
                    .GetAsync(componentTask.ProjectId, componentTask.ComponentId)
                    .ConfigureAwait(false);

                var componentResourceId = AzureResourceIdentifier
                    .Parse(component.ResourceId);

                var componentTemplate = await componentTemplateRepository
                    .GetAsync(component.Organization, component.ProjectId, component.TemplateId)
                    .ConfigureAwait(false);

                var componentLocation = await GetLocationAsync(component)
                    .ConfigureAwait(false);

                var storageId = componentTask.StorageId ?? component.StorageId;

                if (string.IsNullOrEmpty(storageId))
                    throw new NullReferenceException($"Missing storage id for component {component.Id}");

                var componentStorage = await azureResourceService
                    .GetResourceAsync<AzureStorageAccountResource>(storageId, true)
                    .ConfigureAwait(false);

                var componentStorageKeys = await componentStorage
                    .GetKeysAsync()
                    .ConfigureAwait(false);

                var componentShare = await componentStorage
                    .CreateShareClientAsync(component.Id)
                    .ConfigureAwait(false);

                await componentShare
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);

                var componentRunnerHost = $"{componentTask.Id}.{componentLocation}.azurecontainer.io";

                var componentRunnerDefinition = new
                {
                    location = componentLocation,
                    identity = new
                    {
                        type = "UserAssigned",
                        userAssignedIdentities = new Dictionary<string, object>()
                    {
                        { component.IdentityId, new { } }
                    }
                    },
                    properties = new
                    {
                        containers = new[]
                        {
                        new
                        {
                            name = "runner",
                            properties = new
                            {
                                image = componentTemplate.Provider,
                                ports = new []
                                {
                                    new { port = 80 },
                                    new { port = 443 }
                                },
                                resources = new
                                {
                                    requests = new
                                    {
                                        cpu = 1,
                                        memoryInGB = 1
                                    }
                                },
                                environmentVariables = new []
                                {
                                    new {
                                        name = "DeploymentId",
                                        value = componentTask.Id
                                    },
                                    new {
                                        name = "DeploymentHost",
                                        value = componentRunnerHost
                                    },
                                    new {
                                        name = "DeploymentType",
                                        value = componentTask.TypeName ?? componentTask.Type.ToString()
                                    },
                                    new {
                                        name = "EnvironmentResourceId",
                                        value = component.ResourceId
                                    },
                                    new {
                                        name = "EnvironmentLocation",
                                        value = componentLocation
                                    },
                                    new
                                    {
                                        name = "EnvironmentTemplateBaseUrl",
                                        value = $"http://{componentRunnerHost}/{componentTemplate.Folder.Trim().TrimStart('/')}"
                                    },
                                    new
                                    {
                                        name = "EnvironmentTemplateToken",
                                        value = "SECRET"
                                    },
                                    new
                                    {
                                        name = "EnvironmentTemplateFolder",
                                        value = $"file:///mnt/templates/root/{componentTemplate.Folder.Trim().TrimStart('/')}"
                                    },
                                    new
                                    {
                                        name = "EnvironmentTemplateParameters",
                                        value = component.InputJson
                                    },
                                    new
                                    {
                                        name = "EnvironmentResourceGroup",
                                        value = componentResourceId.ResourceGroup
                                    },
                                    new
                                    {
                                        name = "EnvironmentSubscription",
                                        value = componentResourceId.SubscriptionId.ToString()
                                    }
                                },
                                volumeMounts = new []
                                {
                                    new {
                                        name = "templates",
                                        mountPath = "/mnt/templates",
                                        readOnly = false
                                    },
                                    new {
                                        name = "storage",
                                        mountPath = "/mnt/storage",
                                        readOnly = false
                                    }
                                }
                            }
                        }
                    },
                        osType = "Linux",
                        restartPolicy = "Never",
                        ipAddress = new
                        {
                            type = "Public",
                            dnsNameLabel = componentTask.Id,
                            ports = new[]
                            {
                            new {
                                protocol = "tcp",
                                port = 80
                            },
                            new {
                                protocol = "tcp",
                                port = 443
                            }
                        }
                        },
                        volumes = new object[]
                        {
                        new {
                            name = "templates",
                            gitRepo = new
                            {
                                directory = "root",
                                repository = componentTemplate.Repository.Url,
                                revision = componentTemplate.Repository.Ref
                            }
                        },
                        new {
                            name = "storage",
                            azureFile = new
                            {
                                  shareName = componentShare.Name,
                                  storageAccountName = componentShare.AccountName,
                                  storageAccountKey = componentStorageKeys.First()
                            }
                        }
                        }
                    }
                };

                var project = await projectRepository
                    .GetAsync(component.Organization, component.ProjectId)
                    .ConfigureAwait(false);

                var projectResourceId = AzureResourceIdentifier
                    .Parse(project.ResourceId);

                var token = await azureSessionService
                    .AcquireTokenAsync()
                    .ConfigureAwait(false);

                var response = await projectResourceId.GetApiUrl(azureSessionService.Environment)
                    .AppendPathSegment($"/providers/Microsoft.ContainerInstance/containerGroups/{componentTask.Id}")
                    .SetQueryParam("api-version", "2019-12-01")
                    .WithOAuthBearerToken(token)
                    .PutJsonAsync(componentRunnerDefinition)
                    .ConfigureAwait(false);

                var responseJson = await response.Content
                    .ReadAsJsonAsync()
                    .ConfigureAwait(false);

                componentTask.ResourceId = responseJson.SelectToken("$.id").ToString();

                componentTask = await componentTaskRepository
                    .SetAsync(componentTask)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                log.LogError(exc, $"Failed to create runner for component deployment {componentTask}: {exc.Message}");

                throw exc.AsSerializable();
            }

            return componentTask;
        }

        private async Task<string> GetLocationAsync(Component component)
        {
            var tenantId = (await azureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            return organization.Location;
        }


        internal struct Input
        {
            public ComponentTask ComponentTask { get; set; }
        }
    }
}
