/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

                var (componentShareAccount, componentShareName, componentShareKey) = await GetComponentShareInfoAsync(component)
                    .ConfigureAwait(false);

                var componentLocation = await GetComponentRunnerLocationAsync(component)
                    .ConfigureAwait(false);

                // we must not use the component task's id as runner label as this will
                // violate the maximum length of a SSL certificats CN name.
                var componentRunnerRoot = $".{componentLocation}.azurecontainer.io";
                var componentRunnerLabelName = GetComponentRunnerLabel(componentTask, 64 - componentRunnerRoot.Length);
                var componentRunnerHostName = string.Concat(componentRunnerLabelName, componentRunnerRoot);

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
                                        name = "TaskId",
                                        value = componentTask.Id
                                    },
                                    new {
                                        name = "TaskHost",
                                        value = componentRunnerHostName
                                    },
                                    new {
                                        name = "TaskType",
                                        value = componentTask.TypeName ?? componentTask.Type.ToString()
                                    },
                                    new {
                                        name = "ComponentLocation",
                                        value = componentLocation
                                    },
                                    new
                                    {
                                        name = "ComponentTemplateBaseUrl",
                                        value = $"http://{componentRunnerHostName}/{componentTemplate.Folder.Trim().TrimStart('/')}"
                                    },
                                    new
                                    {
                                        name = "ComponentTemplateFolder",
                                        value = $"file:///mnt/templates/root/{componentTemplate.Folder.Trim().TrimStart('/')}"
                                    },
                                    new
                                    {
                                        name = "ComponentTemplateParameters",
                                        value = component.InputJson
                                    },
                                    new
                                    {
                                        name = "ComponentResourceGroup",
                                        value = componentResourceId.ResourceGroup
                                    },
                                    new
                                    {
                                        name = "ComponentSubscription",
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
                                    },
                                    new {
                                        name = "secrets",
                                        mountPath = "/mnt/secrets",
                                        readOnly = false
                                    },
                                    new {
                                        name = "temporary",
                                        mountPath = "/mnt/temporary",
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
                            dnsNameLabel = componentRunnerLabelName,
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
                                      shareName = componentShareName,
                                      storageAccountName = componentShareAccount,
                                      storageAccountKey = componentShareKey
                                }
                            },
                            new {
                                name = "secrets",
                                secret = await GetComponentVaultSecretsAsync(component).ConfigureAwait(false)
                            },
                            new {
                                name = "temporary",
                                emptyDir = new { }
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
                if (exc is FlurlHttpException httpExc)
                {
                    var error = await httpExc.Call.Response
                        .ReadAsJsonAsync()
                        .ConfigureAwait(false);

                    var errorMessage = error.SelectToken("..message")?.ToString() ?? exc.Message;

                    log.LogError(exc, $"Failed to create runner for component deployment {componentTask}: {errorMessage}");
                }
                else
                {
                    log.LogError(exc, $"Failed to create runner for component deployment {componentTask}: {exc.Message}");
                }

                throw exc.AsSerializable();
            }

            return componentTask;
        }

        private async Task<(string, string, string)> GetComponentShareInfoAsync(Component component)
        {
            if (!AzureResourceIdentifier.TryParse(component.StorageId, out var _))
                throw new NullReferenceException($"Missing storage id for component {component.Id}");

            var componentStorage = await azureResourceService
                .GetResourceAsync<AzureStorageAccountResource>(component.StorageId, true)
                .ConfigureAwait(false);

            var componentShare = await componentStorage
                .CreateShareClientAsync(component.Id)
                .ConfigureAwait(false);

            await componentShare
                .CreateIfNotExistsAsync()
                .ConfigureAwait(false);

            var componentStorageKeys = await componentStorage
                .GetKeysAsync()
                .ConfigureAwait(false);

            return (componentShare.AccountName, componentShare.Name, componentStorageKeys.First());
        }

        private async Task<dynamic> GetComponentVaultSecretsAsync(Component component)
        {
            if (!AzureResourceIdentifier.TryParse(component.VaultId, out var _))
                throw new NullReferenceException($"Missing vault id for component {component.Id}");

            var componentVault = await azureResourceService
                .GetResourceAsync<AzureKeyVaultResource>(component.VaultId, true)
                .ConfigureAwait(false);

            var identity = await azureResourceService.AzureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            await componentVault
                .SetAllSecretPermissionsAsync(identity.ObjectId)
                .ConfigureAwait(false);

            dynamic secretsObject = new ExpandoObject(); // the secrets container
            var secretsProperties = secretsObject as IDictionary<string, object>;

            await foreach (var secret in componentVault.GetSecretsAsync())
            {
                var secretNameSafe = Regex.Replace(secret.Key, "[^A-Za-z0-9_]", string.Empty);
                var secretValueSafe = Convert.ToBase64String(Encoding.UTF8.GetBytes(secret.Value));
                secretsProperties.Add(new KeyValuePair<string, object>(secretNameSafe, secretValueSafe));
            }

            var secretsCount = Convert.ToBase64String(Encoding.UTF8.GetBytes(secretsProperties.Count.ToString()));
            secretsProperties.Add(new KeyValuePair<string, object>($"_{nameof(secretsCount)}", secretsCount));

            return secretsObject;
        }

        private async Task<string> GetComponentRunnerLocationAsync(Component component)
        {
            var tenantId = (await azureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            return organization.Location;
        }

        private string GetComponentRunnerLabel(ComponentTask componentTask, int length)
        {
            using var algorithm = new System.Security.Cryptography.SHA512Managed();

            var buffer = algorithm.ComputeHash(Encoding.UTF8.GetBytes(componentTask.Id));

            return string.Concat(buffer.Skip(1).Take(length).Select(b => Convert.ToChar((b % 26) + (byte)'a')));
        }

        internal struct Input
        {
            public ComponentTask ComponentTask { get; set; }
        }
    }
}
