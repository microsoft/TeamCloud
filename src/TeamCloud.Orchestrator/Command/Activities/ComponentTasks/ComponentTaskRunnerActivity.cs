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
using Azure.Core;
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TeamCloud.Adapters;
using TeamCloud.Azure;
using TeamCloud.Azure.Resources;
using TeamCloud.Azure.Resources.Typed;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Options;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestrator.Command.Activities.ComponentTasks;

public sealed class ComponentTaskRunnerActivity
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IDeploymentScopeRepository deploymentScopeRepository;
    private readonly IProjectRepository projectRepository;
    private readonly IComponentRepository componentRepository;
    private readonly IComponentTemplateRepository componentTemplateRepository;
    private readonly IComponentTaskRepository componentTaskRepository;
    private readonly IAzureService azure;
    private readonly IAzureResourceService azureResourceService;
    private readonly IAdapterProvider adapterProvider;
    private readonly IRunnerOptions runnerOptions;

    public ComponentTaskRunnerActivity(IOrganizationRepository organizationRepository,
                                       IDeploymentScopeRepository deploymentScopeRepository,
                                       IProjectRepository projectRepository,
                                       IComponentRepository componentRepository,
                                       IComponentTemplateRepository componentTemplateRepository,
                                       IComponentTaskRepository componentTaskRepository,
                                       IAzureService azureService,
                                       IAzureResourceService azureResourceService,
                                       IAdapterProvider adapterProvider,
                                       IRunnerOptions runnerOptions)
    {
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
        this.azure = azureService ?? throw new ArgumentNullException(nameof(azureService));
        this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
        this.adapterProvider = adapterProvider ?? throw new ArgumentNullException(nameof(adapterProvider));
        this.runnerOptions = runnerOptions ?? throw new ArgumentNullException(nameof(runnerOptions));
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
            componentTask.TaskState = TaskState.Succeeded;
        }
        else
        {
            var pairedRegionFallback = false; pairedRegionFallback:

            try
            {
                var tenantId = await azure
                    .GetTenantIdAsync()
                    .ConfigureAwait(false);

                var organization = await organizationRepository
                    .GetAsync(tenantId, componentTask.Organization)
                    .ConfigureAwait(false);

                var project = await projectRepository
                    .GetAsync(componentTask.Organization, componentTask.ProjectId)
                    .ConfigureAwait(false);

                var deploymentScope = await deploymentScopeRepository
                    .GetAsync(component.Organization, component.DeploymentScopeId)
                    .ConfigureAwait(false);

                var adapter = adapterProvider.GetAdapter(deploymentScope.Type);

                var projectResourceId = new ResourceIdentifier(project.ResourceId);

                // we must not use the component task's id as runner label as this
                // will violate the maximum length of a SSL certificats CN name.

                var componentRunnerCpu = 1;
                var componentRunnerMemory = 2;
                var componentRunnerLocation = await GetLocationAsync(component, projectResourceId.SubscriptionId, componentRunnerCpu, componentRunnerMemory, pairedRegionFallback).ConfigureAwait(false);
                var componentRunnerRoot = $".{componentRunnerLocation}.azurecontainer.io";
                var componentRunnerLabel = CreateUniqueString(64 - componentRunnerRoot.Length, componentTask.Id);
                var componentRunnerHost = string.Concat(componentRunnerLabel, componentRunnerRoot);

                var componentRunnerDefinition = new
                {
                    location = componentRunnerLocation,
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
                        imageRegistryCredentials = await GetRegistryCredentialsAsync(organization).ConfigureAwait(false),
                        containers = await GetContainersAsync(organization, project, deploymentScope, adapter, component, componentTemplate, componentTask, componentRunnerHost, componentRunnerCpu, componentRunnerMemory).ConfigureAwait(false),
                        osType = "Linux",
                        restartPolicy = "Never",
                        ipAddress = GetIpAddress(componentTemplate, componentRunnerLabel),
                        volumes = await GetVolumesAsync(organization, project, deploymentScope, adapter, component, componentTemplate).ConfigureAwait(false)
                    }
                };

                var token = await azure
                    .AcquireTokenAsync()
                    .ConfigureAwait(false);

                try
                {
                    var response = await projectResourceId.GetApiUrl(azure.ArmEnvironment)
                        .AppendPathSegment($"/providers/Microsoft.ContainerInstance/containerGroups/{component.Id}")
                        .SetQueryParam("api-version", "2021-09-01")
                        .WithOAuthBearerToken(token)
                        .PutJsonAsync(componentRunnerDefinition)
                        .ConfigureAwait(false);

                    var responseJson = await response
                        .GetJsonAsync<JObject>()
                        .ConfigureAwait(false);

                    componentTask.ResourceId = responseJson.SelectToken("$.id").ToString();
                }
                catch (FlurlHttpException apiExc) when (apiExc.StatusCode == StatusCodes.Status409Conflict)
                {
                    if (pairedRegionFallback)
                    {
                        // give azure some time to do its work
                        // and remove any naming conflicts

                        await Task
                            .Delay(TimeSpan.FromMinutes(1))
                            .ConfigureAwait(false);
                    }

                    var response = await projectResourceId.GetApiUrl(azure.ArmEnvironment)
                        .AppendPathSegment($"/providers/Microsoft.ContainerInstance/containerGroups/{componentTask.Id}")
                        .SetQueryParam("api-version", "2019-12-01")
                        .AllowHttpStatus(HttpStatusCode.NotFound)
                        .WithOAuthBearerToken(token)
                        .GetAsync()
                        .ConfigureAwait(false);

                    if (response.IsSuccessStatusCode())
                    {
                        var responseJson = await response
                            .GetJsonAsync<JObject>()
                            .ConfigureAwait(false);

                        componentTask.ResourceId = responseJson
                            .SelectToken("$.id")?
                            .ToString();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (FlurlHttpException apiExc) when (apiExc.StatusCode == StatusCodes.Status504GatewayTimeout && !pairedRegionFallback)
                {
                    // enable paired region fallback - this will affect error handling
                    pairedRegionFallback = true;

                    // goto paired region fallback label and restart processing
                    goto pairedRegionFallback;
                }

                componentTask = await componentTaskRepository
                    .SetAsync(componentTask)
                    .ConfigureAwait(false);


            }
            catch (Exception exc)
            {
                var errorMessage = exc.Message;

                if (exc is FlurlHttpException flurlExc && flurlExc.Call.Completed)
                {
                    var error = await flurlExc
                        .GetResponseJsonAsync<JObject>()
                        .ConfigureAwait(false);

                    errorMessage = error.SelectToken("..message")?.ToString() ?? errorMessage;
                }

                log.LogError(exc, $"Failed to create runner for component deployment {componentTask}: {errorMessage}");

                throw exc.AsSerializable();
            }
        }

        return componentTask;
    }

    public object GetIpAddress(ComponentTemplate componentTemplate, string runnerLabel)
    {
        if (componentTemplate.TaskRunner?.WebServer ?? false)
        {
            return new
            {
                type = "Public",
                dnsNameLabel = runnerLabel,
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
            };
        }

        return null;
    }

    private async Task<string> GetContainerImageAsync(Organization organization, ComponentTemplate componentTemplate)
    {
        if (runnerOptions.ImportDockerHub
            && AzureContainerRegistryResource.TryResolveFullyQualifiedContainerImageName(componentTemplate.TaskRunner.Id, out var sourceContainerImage)
            && AzureContainerRegistryResource.IsDockerHubContainerImage(sourceContainerImage)
            && sourceContainerImage.Contains("/teamcloud/", StringComparison.OrdinalIgnoreCase))
        {
            var organizationRegistry = AzureResourceIdentifier.TryParse(organization.RegistryId, out var organizationRegistryId)
                ? await azureResourceService.GetResourceAsync<AzureContainerRegistryResource>(organizationRegistryId.ToString()).ConfigureAwait(false)
                : null;

            if (organizationRegistry is null)
            {
                // the current organization has now container registry
                // assigned so we use the orginal container image origion

                return sourceContainerImage;
            }
            else
            {
                var sourceContainerImport = false; // by default we are not going to import the docker hub container image into the organization container registry
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

            if (response.IsSuccessStatusCode())
            {
                var json = await response
                    .GetJsonAsync<JObject>()
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

    private async Task<object[]> GetRegistryCredentialsAsync(Organization organization)
    {
        var credentials = new List<object>();

        var organizationRegistry = AzureResourceIdentifier.TryParse(organization.RegistryId, out var organizationRegistryId)
            ? await azureResourceService.GetResourceAsync<AzureContainerRegistryResource>(organizationRegistryId.ToString()).ConfigureAwait(false)
            : null;

        if (organizationRegistry is not null)
        {
            var registryCredentials = await organizationRegistry
                .GetCredentialsAsync()
                .ConfigureAwait(false);

            if (registryCredentials is not null)
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

    private async Task<object[]> GetContainersAsync(Organization organization, Project project, DeploymentScope deploymentScope, IAdapter adapter, Component component, ComponentTemplate componentTemplate, ComponentTask componentTask, string runnerHost, int runnerCpu, int runnerMemory)
    {
        var taskToken = CreateUniqueString(30);

        var containers = new List<object>()
        {
            new
            {
                name = componentTask.Id,
                properties = new
                {
                    image = await GetContainerImageAsync(organization, componentTemplate).ConfigureAwait(false),
                    resources = new
                    {
                        requests = new
                        {
                            cpu = runnerCpu,
                            memoryInGB = runnerMemory
                        }
                    },
                    environmentVariables = await GetEnvironmentAsync().ConfigureAwait(false),
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
        };

        if (componentTemplate.TaskRunner?.WebServer ?? false)
        {
            containers.Add(new
            {
                name = "webserver",
                properties = new
                {
                    image = runnerOptions.WebServerImage,
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
                    },
                    resources = new
                    {
                        requests = new
                        {
                            cpu = 1,
                            memoryInGB = 1
                        }
                    },
                    environmentVariables = new object[]
                    {
                        new
                        {
                            name = "TaskHost",
                            value = runnerHost
                        },
                        new
                        {
                            name = "TaskToken",
                            value = taskToken
                        }
                    },
                    volumeMounts = new[]
                    {
                        new
                        {
                            name = "templates",
                            mountPath = "/mnt/templates",
                            readOnly = true
                        }
                    }
                }
            });
        }

        return containers.ToArray();

        async Task<object[]> GetEnvironmentAsync()
        {
            IDictionary<string, string> environment = componentTemplate.TaskRunner?.With ?? new Dictionary<string, string>();

            if (adapter is IAdapterRunner adapterRunner)
            {
                var adapterEnvironment = await adapterRunner
                    .GetEnvironmentAsync(deploymentScope, component)
                    .ConfigureAwait(false);

                if (adapterEnvironment is not null)
                    environment = environment.Override(adapterEnvironment);
            }

            environment["TaskId"] = componentTask.Id;
            environment["TaskHost"] = runnerHost;
            environment["TaskToken"] = taskToken;
            environment["TaskType"] = componentTask.TypeName ?? componentTask.Type.ToString();
            environment["WebServerEnabled"] = ((componentTemplate.TaskRunner?.WebServer ?? false) ? 1 : 0).ToString();

            environment["ComponentLocation"] = organization.Location;
            environment["ComponentResourceId"] = component.ResourceId;

            environment["ComponentTemplateBaseUrl"] = $"http://{runnerHost}/{componentTemplate.Folder.Trim().TrimStart('/')}";
            environment["ComponentTemplateFolder"] = $"file:///mnt/templates/{componentTemplate.Folder.Trim().TrimStart('/')}";
            environment["ComponentTemplateParameters"] = string.IsNullOrWhiteSpace(component.InputJson) ? "{}" : component.InputJson;

            return environment
                .Where(kvp => kvp.Value is not null)
                .Select(kvp => new { name = kvp.Key, value = kvp.Value })
                .ToArray();
        }
    }

    private async Task<object[]> GetVolumesAsync(Organization  organization, Project project, DeploymentScope deploymentScope, IAdapter adapter, Component component, ComponentTemplate componentTemplate)
    {
        return new object[]
        {
            new
            {
                name = "templates",
                gitRepo = new
                {
                    directory = ".",
                    repository = componentTemplate.Repository.Url,
                    revision = componentTemplate.Repository.Ref
                }
            },
            new
            {
                name = "storage",
                azureFile = await GetShareAsync().ConfigureAwait(false)
            },
            new
            {
                name = "secrets",
                secret = await GetSecretsAsync().ConfigureAwait(false)
            },
            new
            {
                name = "credentials",
                secret = await GetCredentialsAsync().ConfigureAwait(false)
            },
            new
            {
                name = "temporary",
                emptyDir = new { }
            }
        };

        async Task<object> GetShareAsync()
        {
            if (string.IsNullOrEmpty(project.StorageId))
                throw new NullReferenceException($"Missing storage id for project {project.Id}");

            var shareClient = await azure.Storage.FileShares
                .GetShareClientAsync(project.StorageId, component.Id)
                .ConfigureAwait(false);

            await shareClient
                .CreateIfNotExistsAsync()
                .ConfigureAwait(false);

            var componentStorageKeys = await azure.Storage
                .GetStorageAccountKeysAsync(project.StorageId)
                .ConfigureAwait(false);

            return new
            {
                shareName = shareClient.Name,
                storageAccountName = shareClient.AccountName,
                storageAccountKey = componentStorageKeys.First()
            };
        }

        async Task<object> GetSecretsAsync()
        {
            if (string.IsNullOrEmpty(project.SharedVaultId))
                throw new NullReferenceException($"Missing vault id for project {project.Id}");

            dynamic secretsObject = new ExpandoObject(); // the secrets container
            var secretsProperties = secretsObject as IDictionary<string, object>;

            if (adapter is IAdapterRunner adapterRunner)
            {
                var secrets = await adapterRunner
                    .GetSecretsAsync(deploymentScope, component)
                    .ConfigureAwait(false);

                foreach (var secret in (secrets ?? new Dictionary<string, string>()).Where(kvp => !string.IsNullOrEmpty(kvp.Value)))
                {
                    secretsProperties[Regex.Replace(secret.Key, "[^A-Za-z0-9_]", string.Empty)] = EncodeValue(secret.Value);
                }
            }

            await foreach (var secret in azure.KeyVaults.GetSecretsAsync(project.SharedVaultId, ensureIdentityAccess: true).Where(kvp => !string.IsNullOrEmpty(kvp.Value)))
            {
                secretsProperties[Regex.Replace(secret.Key, "[^A-Za-z0-9_]", string.Empty)] = EncodeValue(secret.Value);
            }

            var count = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretsProperties.Count}"));
            secretsProperties.Add(new KeyValuePair<string, object>($"_{nameof(count)}", count));

            return secretsObject;
        }

        async Task<object> GetCredentialsAsync()
        {
            dynamic credentialObject = new ExpandoObject(); // the credentials container
            var credentialProperties = credentialObject as IDictionary<string, object>;

            if (adapter is IAdapterRunner adapterRunner)
            {
                var credentials = await adapterRunner
                    .GetCredentialsAsync(deploymentScope, component)
                    .ConfigureAwait(false);

                foreach (var credential in (credentials ?? new Dictionary<string, string>()).Where(kvp => !string.IsNullOrEmpty(kvp.Value)))
                {
                    credentialProperties[Regex.Replace(credential.Key, "[^A-Za-z0-9_]", string.Empty)] = EncodeValue(credential.Value);
                }
            }

            var count = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentialProperties.Count}"));
            credentialProperties.Add(new KeyValuePair<string, object>($"_{nameof(count)}", count));

            return credentialObject;
        }

        string EncodeValue(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    private async Task<string> GetLocationAsync(Component component, string subscriptionId, int runnerCPU, int runnerMemory, bool usePairedRegion = false)
    {
        var tenantId = await azure
            .GetTenantIdAsync()
            .ConfigureAwait(false);

        var organization = await organizationRepository
            .GetAsync(tenantId, component.Organization)
            .ConfigureAwait(false);

        var location = organization.Location;

        if (usePairedRegion || !await IsRegionCapableAsync(location))
        {
            var region = await azure.GetSubscription(subscriptionId)
                .GetLocationsAsync()
                .SingleOrDefaultAsync(l => l.Name.Equals(location))
                .ConfigureAwait(false);

            if (region?.Metadata?.PairedRegions?.Any() ?? false)
            {
                var pairedInfo = region.Metadata.PairedRegions
                    .Select(pr => new { Region = pr.Name, Capable = IsRegionCapableAsync(pr.Name) });

                await pairedInfo
                    .Select(pi => pi.Capable)
                    .WhenAll()
                    .ConfigureAwait(false);

                location = pairedInfo
                    .FirstOrDefault(pi => pi.Capable.Result)?.Region ?? location;
            }
        }

        return location;

        async Task<bool> IsRegionCapableAsync(string region)
        {
            var capabilities = azure.ContainerInstances
                .GetCapabilitiesAsync(subscriptionId, region);

            return await capabilities
                .AnyAsync(c => c.OsType.Equals("Linux", StringComparison.OrdinalIgnoreCase)
                               && c.Gpu.Equals("None", StringComparison.OrdinalIgnoreCase)
                               && c.CapabilitiesProperty.MaxCpu >= runnerCPU
                               && c.CapabilitiesProperty.MaxMemoryInGB >= runnerMemory)
                .ConfigureAwait(false);
        }
    }

    private static string CreateUniqueString(int length, params string[] source)
    {
        if (source.Length == 0) source = new string[] { Guid.NewGuid().ToString() };

        var hash = System.Security.Cryptography.SHA512.HashData(Encoding.UTF8.GetBytes(String.Join('|', source)));

        return string.Concat(hash.Skip(1).Take(length).Select(b => Convert.ToChar((b % 26) + (byte)'a')));
    }

    internal struct Input
    {
        public ComponentTask ComponentTask { get; set; }
    }
}
