/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters;
using TeamCloud.Azure;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.ComponentTasks
{
    public sealed class ComponentTaskRunnerActivity
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IComponentTaskRepository componentTaskRepository;
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;
        private readonly IEnumerable<IAdapter> adapters;

        public ComponentTaskRunnerActivity(IOrganizationRepository organizationRepository,
                                           IDeploymentScopeRepository deploymentScopeRepository,
                                           IProjectRepository projectRepository,
                                           IComponentRepository componentRepository,
                                           IComponentTemplateRepository componentTemplateRepository,
                                           IComponentTaskRepository componentTaskRepository,
                                           IAzureSessionService azureSessionService,
                                           IAzureResourceService azureResourceService,
                                           IEnumerable<IAdapter> adapters)
        {
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.adapters = adapters ?? Enumerable.Empty<IAdapter>();
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

            var component = await componentRepository
                .GetAsync(componentTask.ProjectId, componentTask.ComponentId)
                .ConfigureAwait(false);

            var componentTemplate = await componentTemplateRepository
                .GetAsync(component.Organization, component.ProjectId, component.TemplateId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(componentTemplate.TaskRunner?.Id))
            {
                componentTask.Started = componentTask.Created;
                componentTask.Finished = componentTask.Created;
                componentTask.ExitCode = 0;
                componentTask.ResourceState = Model.Common.ResourceState.Succeeded;
            }
            else
            {
                try
                {
                    var identity = await azureSessionService
                        .GetIdentityAsync()
                        .ConfigureAwait(false);

                    var organization = await organizationRepository
                        .GetAsync(identity.TenantId.ToString(), componentTask.Organization)
                        .ConfigureAwait(false);

                    var organizationRegistry = AzureResourceIdentifier.TryParse(organization.RegistryId, out var organizationRegistryId)
                        ? await azureResourceService.GetResourceAsync<AzureContainerRegistryResource>(organizationRegistryId.ToString()).ConfigureAwait(false)
                        : null;

                    var project = await projectRepository
                        .GetAsync(componentTask.Organization, componentTask.ProjectId)
                        .ConfigureAwait(false);

                    var projectResourceId = AzureResourceIdentifier
                        .Parse(project.ResourceId);

                    var (componentShareAccount, componentShareName, componentShareKey) = await GetComponentShareInfoAsync(project, component)
                        .ConfigureAwait(false);

                    var componentLocation = await GetComponentRunnerLocationAsync(component)
                        .ConfigureAwait(false);

                    // we must not use the component task's id as runner label as this
                    // will violate the maximum length of a SSL certificats CN name.
                    var componentRunnerRoot = $".{componentLocation}.azurecontainer.io";
                    var componentRunnerLabelName = GetComponentRunnerLabel(componentTask, 64 - componentRunnerRoot.Length);
                    var componentRunnerHostName = string.Concat(componentRunnerLabelName, componentRunnerRoot);
                    var componentRunnerContainer = await ResolveContainerImage(componentTemplate, organizationRegistry, log).ConfigureAwait(false);

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
                            imageRegistryCredentials = await GetRegistryCredentialsAsync(componentRunnerContainer).ConfigureAwait(false),
                            containers = new[]
                            {
                                new
                                {
                                    name = "runner",
                                    properties = new
                                    {
                                        image = componentRunnerContainer,
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
                                        environmentVariables = GetEnvironmentVariables(),
                                        volumeMounts = new []
                                        {
                                            new
                                            {
                                                name = "templates",
                                                mountPath = "/mnt/templates",
                                                readOnly = false
                                            },
                                            new
                                            {
                                                name = "storage",
                                                mountPath = "/mnt/storage",
                                                readOnly = false
                                            },
                                            new
                                            {
                                                name = "secrets",
                                                mountPath = "/mnt/secrets",
                                                readOnly = false
                                            },
                                            new
                                            {
                                                name = "credentials",
                                                mountPath = "/mnt/credentials",
                                                readOnly = false
                                            },
                                            new
                                            {
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
                                new
                                {
                                    name = "templates",
                                    gitRepo = new
                                    {
                                        directory = "root",
                                        repository = componentTemplate.Repository.Url,
                                        revision = componentTemplate.Repository.Ref
                                    }
                                },
                                new
                                {
                                    name = "storage",
                                    azureFile = new
                                    {
                                          shareName = componentShareName,
                                          storageAccountName = componentShareAccount,
                                          storageAccountKey = componentShareKey
                                    }
                                },
                                new
                                {
                                    name = "secrets",
                                    secret = await GetVaultSecretsAsync(project).ConfigureAwait(false)
                                },
                                new
                                {
                                    name = "credentials",
                                    secret = await GetServiceCredentialsAsync(component).ConfigureAwait(false)
                                },
                                new
                                {
                                    name = "temporary",
                                    emptyDir = new { }
                                }
                            }
                        }
                    };

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

                    object[] GetEnvironmentVariables()
                    {
                        var envVariables = componentTemplate.TaskRunner?.With ?? new Dictionary<string, string>();

                        envVariables["TaskId"] = componentTask.Id;
                        envVariables["TaskHost"] = componentRunnerHostName;
                        envVariables["TaskType"] = componentTask.TypeName ?? componentTask.Type.ToString();
                        envVariables["ComponentLocation"] = componentLocation;
                        envVariables["ComponentTemplateBaseUrl"] = $"http://{componentRunnerHostName}/{componentTemplate.Folder.Trim().TrimStart('/')}";
                        envVariables["ComponentTemplateFolder"] = $"file:///mnt/templates/root/{componentTemplate.Folder.Trim().TrimStart('/')}";
                        envVariables["ComponentTemplateParameters"] = string.IsNullOrWhiteSpace(component.InputJson) ? "{}" : component.InputJson;
                        envVariables["ComponentResourceId"] = component.ResourceId;

                        if (AzureResourceIdentifier.TryParse(component.ResourceId, out var componentResourceId))
                        {
                            envVariables["ComponentResourceGroup"] = componentResourceId.ResourceGroup;
                            envVariables["ComponentSubscription"] = componentResourceId.SubscriptionId.ToString();
                        }

                        return envVariables.Select(kvp => new { name = kvp.Key, value = kvp.Value }).ToArray();
                    }

                    async Task<object[]> GetRegistryCredentialsAsync(string containerImageName)
                    {
                        var credentials = new List<object>();

                        if (organizationRegistry != null)
                        {
                            var registryCredentials = await organizationRegistry
                                .GetCredentialsAsync(containerImageName)
                                .ConfigureAwait(false);

                            if (AzureContainerRegistryResource.GetContainerHost(containerImageName).Equals(registryCredentials?.Domain, StringComparison.OrdinalIgnoreCase))
                            {
                                credentials.Add(new
                                {
                                    server = registryCredentials.Domain,
                                    username = registryCredentials.UserName,
                                    password = registryCredentials.Password
                                });
                            }
                        }

                        return credentials.ToArray();
                    }
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
            }

            return componentTask;
        }

        private static async Task<string> ResolveContainerImage(ComponentTemplate componentTemplate, AzureContainerRegistryResource organizationRegistry, ILogger log)
        {
            if (componentTemplate is null)
                throw new ArgumentNullException(nameof(componentTemplate));

            if (string.IsNullOrWhiteSpace(componentTemplate.TaskRunner?.Id))
                throw new ArgumentException($"'{nameof(componentTemplate)}' must contain a TaskRunner container image reference.", nameof(componentTemplate));

            if (organizationRegistry is null)
                throw new ArgumentNullException(nameof(organizationRegistry));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            if (AzureContainerRegistryResource.TryResolveFullyQualifiedContainerImageName(componentTemplate.TaskRunner.Id, out var sourceContainerImage)
                && AzureContainerRegistryResource.IsDockerHubContainerImage(sourceContainerImage)
                && sourceContainerImage.Contains("/teamcloud/", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var sourceContainerImport = false;
                    var targetContainerImage = $"{organizationRegistry.Hostname}{sourceContainerImage.Substring(sourceContainerImage.IndexOf('/', StringComparison.OrdinalIgnoreCase))}";

                    if (AzureContainerRegistryResource.IsContainerImageNameTagBased(sourceContainerImage))
                    {
                        var sourceContainerDigest = await GetDockerHubContainerImageDigestAsync(sourceContainerImage).ConfigureAwait(false);

                        if (string.IsNullOrEmpty(sourceContainerDigest))
                        {
                            // the container image is part of a private registry
                            // on docker hub or just doesn't exist. return the 
                            // fully qualified container image name of the source
                            // and let the caller handle a potential error

                            return sourceContainerImage;
                        }

                        var targetContainerDigest = await organizationRegistry.GetContainerImageDigestAsync(targetContainerImage).ConfigureAwait(false);

                        if (sourceContainerDigest?.Equals(targetContainerDigest, StringComparison.Ordinal) ?? false)
                        {
                            // our organization registry contains the requested image
                            // let's use this one to shorten the image pull duration

                            return targetContainerImage;
                        }

                        // we go with the source container image but mark this
                        // image for import into the organization registry

                        sourceContainerImport = true;
                    }
                    else if (await organizationRegistry.ContainesContainerImageAsync(targetContainerImage).ConfigureAwait(false))
                    {
                        // our organization registry contains the requested image
                        // let's use this one to shorten the image pull duration

                        return targetContainerImage;
                    }
                    else
                    {
                        // we go with the source container image but mark this
                        // image for import into the organization registry

                        sourceContainerImport = true;
                    }

                    if (sourceContainerImport)
                    {
                        var sourceContainerTags = await GetDockerHubContainerImageTagsAsync(targetContainerImage)
                            .ToArrayAsync()
                            .ConfigureAwait(false);

                        await organizationRegistry
                            .ImportContainerImageAsync(sourceContainerImage, sourceContainerTags, force: true)
                            .ConfigureAwait(false);
                    }

                    return sourceContainerImage;
                }
                catch (Exception exc)
                {
                    log.LogWarning(exc, $"Failed to resolve local container image for of '{sourceContainerImage}'");
                }
            }

            // this is our fallback if something bad happend
            // during the container image evaluation

            return componentTemplate.TaskRunner.Id;

            async Task<string> GetDockerHubContainerImageDigestAsync(string containerImageName)
            {
                if (!AzureContainerRegistryResource.IsContainerImageNameTagBased(containerImageName))
                    throw new ArgumentException($"'{nameof(containerImageName)}' contain a tag based reference.", nameof(containerImageName));

                containerImageName = AzureContainerRegistryResource.ResolveFullyQualifiedContainerImageName(containerImageName);

                var containerName = AzureContainerRegistryResource.GetContainerName(containerImageName);
                var containerReference = AzureContainerRegistryResource.GetContainerReference(containerImageName);

                var response = await $"https://hub.docker.com/v2/repositories/{containerName}/tags/{containerReference}"
                    .AllowHttpStatus(HttpStatusCode.NotFound)
                    .GetAsync()
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content
                        .ReadAsJsonAsync()
                        .ConfigureAwait(false);

                    return json.SelectToken("images[?(@.status == 'active')].digest")?.ToString();
                }

                return null;
            }

            async IAsyncEnumerable<string> GetDockerHubContainerImageTagsAsync(string containerImageName)
            {
                if (AzureContainerRegistryResource.IsContainerImageNameTagBased(containerImageName))
                {
                    var digest = await GetDockerHubContainerImageDigestAsync(containerImageName).ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(digest))
                        throw new ArgumentException($"Failed to resolve digest for argument '{nameof(containerImageName)}'.", nameof(containerImageName));

                    containerImageName = AzureContainerRegistryResource.ChangeContainerReference(containerImageName, digest);
                }

                var containerName = AzureContainerRegistryResource.GetContainerName(containerImageName);
                var containerDigest = AzureContainerRegistryResource.GetContainerReference(containerImageName);

                var registryUrl = $"https://hub.docker.com/v2/repositories/{containerName}/tags/";

                while (!string.IsNullOrEmpty(registryUrl))
                {
                    var json = await registryUrl
                        .GetJObjectAsync()
                        .ConfigureAwait(false);

                    foreach (var tag in json.SelectTokens($"results[?(@.images[?(@.status == 'active' && @.digest == '{containerDigest}')])].name"))
                        yield return tag.ToString();

                    registryUrl = json
                        .SelectToken("next")?
                        .ToString();
                }
            }
        }

        private async Task<(string, string, string)> GetComponentShareInfoAsync(Project project, Component component)
        {
            if (!AzureResourceIdentifier.TryParse(project.StorageId, out var _))
                throw new NullReferenceException($"Missing storage id for project {project.Id}");

            var componentStorage = await azureResourceService
                .GetResourceAsync<AzureStorageAccountResource>(project.StorageId, true)
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

        private async Task<dynamic> GetVaultSecretsAsync(Project project)
        {
            if (!AzureResourceIdentifier.TryParse(project.VaultId, out var _))
                throw new NullReferenceException($"Missing vault id for project {project.Id}");

            var componentVault = await azureResourceService
                .GetResourceAsync<AzureKeyVaultResource>(project.VaultId, true)
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
                if (secret.Value is null) continue;

                var secretNameSafe = Regex.Replace(secret.Key, "[^A-Za-z0-9_]", string.Empty);
                var secretValueSafe = Convert.ToBase64String(Encoding.UTF8.GetBytes(secret.Value));

                secretsProperties.Add(new KeyValuePair<string, object>(secretNameSafe, secretValueSafe));
            }

            var count = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretsProperties.Count}"));
            secretsProperties.Add(new KeyValuePair<string, object>($"_{nameof(count)}", count));

            return secretsObject;
        }

        private async Task<dynamic> GetServiceCredentialsAsync(Component component)
        {
            dynamic credentialObject = new ExpandoObject(); // the credentials container
            var credentialProperties = credentialObject as IDictionary<string, object>;

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId)
                .ConfigureAwait(false);

            if (adapters.TryGetAdapter(deploymentScope.Type, out var adapter))
            {
                var credential = await adapter
                    .GetServiceCredentialAsync(component)
                    .ConfigureAwait(false);

                if (credential != null)
                {
                    credentialProperties.Add(new KeyValuePair<string, object>("domain", EncodeValue(credential.Domain)));
                    credentialProperties.Add(new KeyValuePair<string, object>("username", EncodeValue(credential.UserName)));
                    credentialProperties.Add(new KeyValuePair<string, object>("password", EncodeValue(credential.Password)));

                    string EncodeValue(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
                }
            }

            var count = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentialProperties.Count}"));
            credentialProperties.Add(new KeyValuePair<string, object>($"_{nameof(count)}", count));

            return credentialObject;
        }

        private async Task<string> GetComponentRunnerLocationAsync(Component component)
        {
            var tenantId = (await azureSessionService.GetIdentityAsync().ConfigureAwait(false)).TenantId;

            var organization = await organizationRepository
                .GetAsync(tenantId.ToString(), component.Organization)
                .ConfigureAwait(false);

            return organization.Location;
        }

        private static string GetComponentRunnerLabel(ComponentTask componentTask, int length)
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
