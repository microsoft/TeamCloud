using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;

namespace TeamCloud.Orchestrator.Operations.Activities
{
    public sealed class ComponentRunnerActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IComponentDeploymentRepository componentDeploymentRepository;
        private readonly IAzureSessionService azureSessionService;

        public ComponentRunnerActivity(IOrganizationRepository organizationRepository,
                                       IProjectRepository projectRepository,
                                       IComponentTemplateRepository componentTemplateRepository,
                                       IComponentDeploymentRepository componentDeploymentRepository,
                                       IAzureSessionService azureSessionService)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.componentDeploymentRepository = componentDeploymentRepository ?? throw new ArgumentNullException(nameof(componentDeploymentRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(ComponentRunnerActivity))]
        [RetryOptions(3)]
        public async Task<ComponentDeployment> Run(
            [ActivityTrigger] IDurableActivityContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var component = context.GetInput<Input>().Component;

            var componentResourceId = AzureResourceIdentifier
                .Parse(component.ResourceId);

            var componentTemplate = await componentTemplateRepository
                .GetAsync(component.Organization, component.ProjectId, component.TemplateId)
                .ConfigureAwait(false);

            var componentDeployment = await componentDeploymentRepository.AddAsync(new ComponentDeployment()
            {
                ComponentId = component.Id,
                ProjectId = component.ProjectId

            }).ConfigureAwait(false);

            var componentLocation = await GetLocationAsync(component)
                .ConfigureAwait(false);

            var componentRunnerHost = $"{componentDeployment.Id}.{componentLocation}.azurecontainer.io";

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
                                        value = componentDeployment.Id
                                    },
                                    new {
                                        name = "DeploymentHost",
                                        value = componentRunnerHost
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
                        dnsNameLabel = componentDeployment.Id,
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
                    volumes = new[]
                    {
                        new {
                            name = "templates",
                            gitRepo = new
                            {
                                directory = "root",
                                repository = componentTemplate.Repository.Url,
                                revision = componentTemplate.Repository.Ref
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
                .AppendPathSegment($"/providers/Microsoft.ContainerInstance/containerGroups/{componentDeployment.Id}")
                .SetQueryParam("api-version", "2019-12-01")
                .WithOAuthBearerToken(token)
                .PutJsonAsync(componentRunnerDefinition)
                .ConfigureAwait(false);

            var responseJson = await response.Content
                .ReadAsJsonAsync()
                .ConfigureAwait(false);

            componentDeployment.ResourceId = responseJson.SelectToken("$.id").ToString();

            return await componentDeploymentRepository
                .SetAsync(componentDeployment)
                .ConfigureAwait(false);
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
            public Component Component { get; set; }
        }
    }
}
