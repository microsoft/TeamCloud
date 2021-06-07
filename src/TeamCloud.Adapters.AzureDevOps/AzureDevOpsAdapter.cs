/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.DevOps.Licensing.WebApi;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Licensing;
using Microsoft.VisualStudio.Services.MemberEntitlementManagement.WebApi;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;
using TeamCloud.Serialization.Forms;
using TeamCloud.Templates;
using IHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;

namespace TeamCloud.Adapters.AzureDevOps
{
    public sealed class AzureDevOpsAdapter : Adapter, IAdapterAuthorize
    {
        private const string VisualStudioAuthUrl = "https://app.vssps.visualstudio.com/oauth2/authorize";
        private const string VisualStudioTokenUrl = "https://app.vssps.visualstudio.com/oauth2/token";

        private readonly IHttpClientFactory httpClientFactory;
        private readonly IUserRepository userRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IFunctionsHost functionsHost;

        public AzureDevOpsAdapter(IAuthorizationSessionClient sessionClient,
                                  IAuthorizationTokenClient tokenClient,
                                  IDistributedLockManager distributedLockManager,
                                  IHttpClientFactory httpClientFactory,
                                  IUserRepository userRepository,
                                  IDeploymentScopeRepository deploymentScopeRepository,
                                  IProjectRepository projectRepository,
                                  IComponentRepository componentRepository,
                                  IFunctionsHost functionsHost = null)
            : base(sessionClient, tokenClient, distributedLockManager)
        {
            this.httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.functionsHost = functionsHost ?? FunctionsHost.Default;
        }

        public override DeploymentScopeType Type
            => DeploymentScopeType.AzureDevOps;

        public override IEnumerable<ComponentType> ComponentTypes
            => new ComponentType[] { ComponentType.Repository };

        public override string DisplayName
            => "Azure DevOps";

        public override Task<string> GetInputDataSchemaAsync()
            => TeamCloudForm.GetDataSchemaAsync<AzureDevOpsData>().ContinueWith(t => t.Result.ToString(), TaskScheduler.Current);

        public override Task<string> GetInputFormSchemaAsync()
            => TeamCloudForm.GetFormSchemaAsync<AzureDevOpsData>().ContinueWith(t => t.Result.ToString(), TaskScheduler.Current);

        public override async Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var token = await TokenClient
                .GetAsync<AzureDevOpsToken>(deploymentScope)
                .ConfigureAwait(false);

            return !(token is null);
        }

        Task IAdapterAuthorize.CreateSessionAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (deploymentScope.Type != Type)
                throw new ArgumentException("Argument value can not be handled", nameof(deploymentScope));

            return SessionClient.SetAsync(new AzureDevOpsSession(deploymentScope));
        }

        async Task<IActionResult> IAdapterAuthorize.HandleAuthorizeAsync(DeploymentScope deploymentScope, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (authorizationEndpoints is null)
                throw new ArgumentNullException(nameof(authorizationEndpoints));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var authorizationSession = await SessionClient
                .GetAsync<AzureDevOpsSession>(deploymentScope)
                .ConfigureAwait(false);

            var task = requestMessage.Method switch
            {
                HttpMethod m when m == HttpMethod.Get => HandleAuthorizeGetAsync(authorizationSession, requestMessage, authorizationEndpoints, log),
                HttpMethod m when m == HttpMethod.Post => HandleAuthorizePostAsync(authorizationSession, requestMessage, authorizationEndpoints, log),
                _ => Task.FromException<IActionResult>(new NotImplementedException())
            };

            return await task.ConfigureAwait(false);
        }

        async Task<IActionResult> IAdapterAuthorize.HandleCallbackAsync(DeploymentScope deploymentScope, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (requestMessage is null)
                throw new ArgumentNullException(nameof(requestMessage));

            if (authorizationEndpoints is null)
                throw new ArgumentNullException(nameof(authorizationEndpoints));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            var authorizationSession = await SessionClient
                .GetAsync<AzureDevOpsSession>(deploymentScope)
                .ConfigureAwait(false);

            if (authorizationSession is null)
            {
                return new NotFoundResult();
            }

            var queryParams = requestMessage.RequestUri.ParseQueryString();

            if (queryParams.TryGetValue("error", out string error))
            {
                return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", error));
            }
            else if (!queryParams.TryGetValue("state", out string state) || !authorizationSession.SessionState.Equals(state, StringComparison.OrdinalIgnoreCase))
            {
                return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", "Authorization state invalid"));
            }
            else
            {
                var form = new
                {
                    client_assertion_type = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                    client_assertion = authorizationSession.ClientSecret,
                    grant_type = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    assertion = queryParams.Get("code"),
                    redirect_uri = authorizationEndpoints.CallbackUrl
                };

                var responseMessage = await VisualStudioTokenUrl
                    .WithHeaders(new MediaTypeWithQualityHeaderValue("application/json"))
                    .AllowAnyHttpStatus()
                    .PostUrlEncodedAsync(form)
                    .ConfigureAwait(false);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var azureDevOpsAuthorizationToken = new AzureDevOpsToken(deploymentScope)
                    {
                        Organization = authorizationSession.Organization,
                        ClientId = authorizationSession.ClientId,
                        ClientSecret = authorizationSession.ClientSecret,
                        RefreshCallback = authorizationEndpoints.CallbackUrl
                    };

                    var json = await responseMessage
                        .ReadAsJsonAsync()
                        .ConfigureAwait(false);

                    TeamCloudSerialize.PopulateObject(json.ToString(), azureDevOpsAuthorizationToken);

                    _ = await TokenClient
                        .SetAsync(azureDevOpsAuthorizationToken, true)
                        .ConfigureAwait(false);

                    log.LogInformation($"Token information successfully acquired.");
                }
                else
                {
                    error = await (responseMessage.StatusCode == HttpStatusCode.BadRequest
                        ? GetErrorDescriptionAsync(responseMessage)
                        : Task.FromResult(responseMessage.ReasonPhrase)).ConfigureAwait(false);

                    return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", error));
                }
            }

            _ = await SessionClient
                .SetAsync(authorizationSession)
                .ConfigureAwait(false);

            return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("succeeded"));

            async Task<string> GetErrorDescriptionAsync(HttpResponseMessage responseMessage)
            {
                try
                {
                    var json = await responseMessage
                        .ReadAsJsonAsync()
                        .ConfigureAwait(false);

                    return json?.SelectToken("$..ErrorDescription")?.ToString() ?? json.ToString();
                }
                catch
                {
                    return null;
                }
            }
        }

        private async Task<IActionResult> HandleAuthorizeGetAsync(AzureDevOpsSession authorizationSession, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            var queryParams = Url.ParseQueryParams(requestMessage.RequestUri.Query);
            var queryError = queryParams.GetValueOrDefault("error", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(queryError))
            {
                log.LogWarning($"Authorization failed: {queryError}");
            }
            else if (queryParams.ContainsKey("succeeded"))
            {
                log.LogInformation($"Authorization succeeded");
            }

            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "text/html",
                Content = Assembly.GetExecutingAssembly().GetManifestResourceTemplate($"{GetType().FullName}.html", new
                {
                    applicationWebsite = functionsHost.HostUrl,
                    applicationCallback = authorizationEndpoints.CallbackUrl,
                    session = authorizationSession,
                    error = queryError ?? string.Empty,
                    succeeded = queryParams.ContainsKey("succeeded")
                })
            };
        }

        private async Task<IActionResult> HandleAuthorizePostAsync(AzureDevOpsSession authorizationSession, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints, ILogger log)
        {
            var payload = await requestMessage.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            var payloadParams = Url.ParseQueryParams(payload);

            authorizationSession.Organization = payloadParams.GetValueOrDefault("organization", StringComparison.OrdinalIgnoreCase);
            authorizationSession.ClientId = payloadParams.GetValueOrDefault("client_id", StringComparison.OrdinalIgnoreCase);
            authorizationSession.ClientSecret = payloadParams.GetValueOrDefault("client_secret", StringComparison.OrdinalIgnoreCase);
            authorizationSession = await SessionClient.SetAsync(authorizationSession).ConfigureAwait(false);

            var url = VisualStudioAuthUrl
                .SetQueryParam("client_id", authorizationSession.ClientId)
                .SetQueryParam("response_type", "Assertion")
                .SetQueryParam("state", authorizationSession.SessionState)
                .SetQueryParam("scope", string.Join(' ', AzureDevOpsSession.Scopes))
                .SetQueryParam("redirect_uri", authorizationEndpoints.CallbackUrl)
                .ToString();

            log.LogDebug($"Redirecting authorize POST response to {url}");

            return new RedirectResult(url);
        }

        private async Task<T> CreateClientAsync<T>(Component component)
            where T : VssHttpClientBase
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId, true)
                .ConfigureAwait(false);

            return await CreateClientAsync<T>(deploymentScope)
                .ConfigureAwait(false);
        }

        private async Task<T> CreateClientAsync<T>(DeploymentScope deploymentScope)
            where T : VssHttpClientBase
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var token = await TokenClient
                .GetAsync<AzureDevOpsToken>(deploymentScope)
                .ConfigureAwait(false);

            if (token is null)
            {
                return null; // no token - no client
            }
            else if (token.AccessTokenExpired)
            {
                if (token.RefreshTokenExpired)
                    throw new Exception("Refresh");

                token = await RefreshTokenAsync(token)
                    .ConfigureAwait(false);
            }

#pragma warning disable CA2000 // Dispose objects before losing scope

            var connection = new VssConnection(
                new Uri(token.Organization),
                new VssHttpMessageHandler(
                    new VssOAuthAccessTokenCredential(token.AccessToken),
                    VssClientHttpRequestSettings.Default.Clone(),
                    httpClientFactory.CreateMessageHandler()),
                null);

            return await connection
                .GetClientAsync<T>()
                .ConfigureAwait(false);

#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        private async Task<AzureDevOpsToken> RefreshTokenAsync(AzureDevOpsToken token)
        {
            if (token is null)
                throw new ArgumentNullException(nameof(token));

            if (token.RefreshTokenExpired)
                throw new ArgumentException($"Refresh token expired on '{token.RefreshTokenExpires}'.", nameof(token));

            var form = new
            {
                client_assertion_type = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                client_assertion = token.ClientSecret,
                grant_type = "refresh_token",
                assertion = token.RefreshToken,
                redirect_uri = token.RefreshCallback
            };

            var response = await VisualStudioTokenUrl
                .WithHeaders(new MediaTypeWithQualityHeaderValue("application/json"))
                .AllowAnyHttpStatus()
                .PostUrlEncodedAsync(form)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                TeamCloudSerialize.PopulateObject(json, token);

                token = await TokenClient
                    .SetAsync(token)
                    .ConfigureAwait(false);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response
                    .GetBadRequestErrorDescriptionAsync()
                    .ConfigureAwait(false);

                token = null;
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }

            return token;
        }

        private async Task<Operation> WaitForOperationAsync(Component component, Guid operationId, int intervalInSeconds = 5, int timeoutInSeconds = 60)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId, true)
                .ConfigureAwait(false);

            return await WaitForOperationAsync(deploymentScope, operationId, intervalInSeconds, timeoutInSeconds)
                .ConfigureAwait(false);
        }

        private async Task<Operation> WaitForOperationAsync(DeploymentScope deploymentScope, Guid operationId, int intervalInSeconds = 5, int timeoutInSeconds = 60)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var expiration = DateTime.Now.AddSeconds(timeoutInSeconds);

            using var operationsClient = await CreateClientAsync<OperationsHttpClient>(deploymentScope).ConfigureAwait(false);

            while (true)
            {
                var operation = await operationsClient
                    .GetOperation(operationId)
                    .ConfigureAwait(false);

                if (operation.Completed)
                {
                    return operation;
                }
                else if (DateTime.Now > expiration)
                {
                    throw new Exception($"Operation did not complete in {timeoutInSeconds} seconds.");
                }
                else
                {
                    await Task.Delay(intervalInSeconds * 1000).ConfigureAwait(false);
                }
            }
        }

        private async Task<Component> ExecuteAsync(Component component, IAsyncCollector<ICommand> commandQueue, ILogger log, bool ensureResourceExists, Func<TeamProject, Task<Component>> callback)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var componentProject = await projectRepository
                .GetAsync(component.Organization, component.ProjectId)
                .ConfigureAwait(false);

            using var projectClient = await CreateClientAsync<ProjectHttpClient>(component).ConfigureAwait(false);

            var projectIdentifier = AzureDevOpsIdentifier.TryParse(component.ResourceId, out var identifier)
                ? identifier.Project ?? componentProject.DisplayName
                : componentProject.DisplayName;

            TeamProject project;

            await using (var adapterLock = await AcquireLockAsync(nameof(AzureDevOpsAdapter), component.DeploymentScopeId).ConfigureAwait(false))
            {
                try
                {
                    project = await projectClient
                        .GetProject(projectIdentifier)
                        .ConfigureAwait(false);
                }
                catch
                {
                    project = null;
                }

                if (project is null || !(await MatchesTeamCloudProjectIdAsync(project.Id).ConfigureAwait(false)))
                {
                    var projectReferences = await projectClient
                        .GetProjects()
                        .ConfigureAwait(false);

                    var projectReference = await projectReferences
                        .AsContinuousCollectionAsync(continuationToken => projectClient.GetProjects(continuationToken: continuationToken))
                        .FirstOrDefaultAwaitAsync(projectRef => MatchesTeamCloudProjectIdAsync(projectRef.Id))
                        .ConfigureAwait(false);

                    project = projectReference is null ? null : await projectClient
                        .GetProject(projectReference.Id.ToString())
                        .ConfigureAwait(false);
                }

                if (project is null && ensureResourceExists)
                {
                    using var processClient = await CreateClientAsync<ProcessHttpClient>(component).ConfigureAwait(false);

                    var processTemplates = await processClient
                        .GetProcessesAsync()
                        .ConfigureAwait(false);

                    var processCapabilities = new Dictionary<string, string>()
                    {
                        { TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityTemplateTypeIdAttributeName, processTemplates.Single(pt => pt.Name.Equals("Agile", StringComparison.OrdinalIgnoreCase)).Id.ToString() }
                    };

                    var versionControlCapabilities = new Dictionary<string, string>()
                    {
                        { TeamProjectCapabilitiesConstants.VersionControlCapabilityAttributeName, SourceControlTypes.Git.ToString() }
                    };

                    var projectTemplate = new TeamProject()
                    {
                        Name = await projectClient.GenerateProjectNameAsync(componentProject.DisplayName).ConfigureAwait(false),
                        Description = string.Empty,
                        Capabilities = new Dictionary<string, Dictionary<string, string>>()
                        {
                            { TeamProjectCapabilitiesConstants.VersionControlCapabilityName, versionControlCapabilities },
                            { TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityName, processCapabilities }
                        }
                    };

                    var projectOperation = await projectClient
                        .QueueCreateProject(projectTemplate)
                        .ConfigureAwait(false);

                    if (!projectOperation.Status.IsFinal())
                        await WaitForOperationAsync(component, projectOperation.Id).ConfigureAwait(false);

                    project = await projectClient
                        .GetProject(projectTemplate.Name)
                        .ConfigureAwait(false);

                    var properties = new JsonPatchDocument();

                    properties.Add(new JsonPatchOperation()
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path = $"/TeamCloud.Organization",
                        Value = $"{componentProject.Organization}"
                    });

                    properties.Add(new JsonPatchOperation()
                    {
                        Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                        Path = $"/TeamCloud.Project",
                        Value = $"{componentProject.Id}"
                    });

                    await projectClient
                        .SetProjectPropertiesAsync(project.Id, properties)
                        .ConfigureAwait(false);
                }

                if (project != null && !AzureDevOpsIdentifier.TryParse(component.ResourceId, out var _))
                {
                    component.ResourceId = AzureDevOpsIdentifier
                        .FromUrl(project.Url)
                        .ToString();

                    component = await componentRepository
                        .SetAsync(component)
                        .ConfigureAwait(false);
                }
            }

            return await callback(project).ConfigureAwait(false);

            async ValueTask<bool> MatchesTeamCloudProjectIdAsync(Guid projectId)
            {
                var projectProperties = await projectClient
                    .GetProjectPropertiesAsync(projectId, new string[] { "TeamCloud.Project" })
                    .ConfigureAwait(false);

                return componentProject.Id.Equals(projectProperties.FirstOrDefault()?.Value as string, StringComparison.OrdinalIgnoreCase);
            }
        }

        public override Task<Component> CreateComponentAsync(Component component, IAsyncCollector<ICommand> commandQueue, ILogger log) => ExecuteAsync(component, commandQueue, log, true, async (teamProject) =>
        {
            var resourceId = AzureDevOpsIdentifier.Parse(component.ResourceId);
            var resourceIdUpdated = false;

            using var gitClient = await CreateClientAsync<GitHttpClient>(component).ConfigureAwait(false);

            GitRepository gitRepo = null;

            if (resourceId.TryGetResourceValue("repositories", true, out var repositoryValue) && Guid.TryParse(repositoryValue, out var repositoryId))
            {
                gitRepo = await gitClient
                    .GetRepositoryAsync(repositoryId)
                    .ConfigureAwait(false);
            }

            if (gitRepo is null)
            {
                var gitRepos = await gitClient
                    .GetRepositoriesAsync(teamProject.Id)
                    .ConfigureAwait(false);

                gitRepo = gitRepos
                    .SingleOrDefault(r => r.Name.Equals(teamProject.Name, StringComparison.OrdinalIgnoreCase));

                resourceIdUpdated = !(gitRepo is null);
            }

            if (gitRepo is null)
            {
                gitRepo = await gitClient
                    .CreateRepositoryAsync(new GitRepository() { Name = component.DisplayName, ProjectReference = teamProject })
                    .ConfigureAwait(false);

                resourceIdUpdated = true;
            }

            if (resourceIdUpdated)
            {
                component.ResourceId = AzureDevOpsIdentifier.FromUrl(gitRepo.Url).ToString();
                component.ResourceUrl = gitRepo.WebUrl;

                component = await componentRepository
                    .SetAsync(component)
                    .ConfigureAwait(false);
            }

            return await UpdateComponentAsync(component, commandQueue, log).ConfigureAwait(false);
        });

        public override Task<Component> UpdateComponentAsync(Component component, IAsyncCollector<ICommand> commandQueue, ILogger log) => ExecuteAsync(component, commandQueue, log, false, async (teamProject) =>
        {
            if (teamProject is null)
                throw new ArgumentNullException(nameof(teamProject));

            using var graphClient = await CreateClientAsync<GraphHttpClient>(component).ConfigureAwait(false);
            using var entitlementClient = await CreateClientAsync<MemberEntitlementManagementHttpClient>(component).ConfigureAwait(false);
            using var teamClient = await CreateClientAsync<TeamHttpClient>(component).ConfigureAwait(false);

            var aadGraphUsers = new ConcurrentDictionary<string, string>();
            var aadGraphGroups = new ConcurrentDictionary<string, string>();

            await Task.WhenAll(

                graphClient
                    .ListAllUsersAsync()
                    .Where(graphUser => graphUser.Origin.Equals("aad", StringComparison.OrdinalIgnoreCase))
                    .ForEachAsync(graphUser => aadGraphUsers.TryAdd(graphUser.OriginId, graphUser.Descriptor)),

                graphClient
                    .ListAllGroupsAsync()
                    .Where(graphGroup => graphGroup.Origin.Equals("aad", StringComparison.OrdinalIgnoreCase))
                    .ForEachAsync(graphGroup => aadGraphGroups.TryAdd(graphGroup.OriginId, graphGroup.Descriptor))

                ).ConfigureAwait(false);


            var users = await userRepository
                .ListAsync(component.Organization, component.ProjectId)
                .WhereAwait(user => EnsureUserAsync(user, entitlementClient))
                .ToListAsync()
                .ConfigureAwait(false);

            var projectDescriptorResult = await graphClient
                .GetDescriptorAsync(teamProject.Id)
                .ConfigureAwait(false);

            await Task.WhenAll(

                SyncGroupAsync(ProjectUserRole.Admin, projectDescriptorResult.Value, users.Where(u => u.IsAdmin(component.ProjectId)).Select(u => GetUserDescriptor(u))),
                SyncGroupAsync(ProjectUserRole.Owner, projectDescriptorResult.Value, users.Where(u => u.IsOwner(component.ProjectId)).Select(u => GetUserDescriptor(u))),
                SyncGroupAsync(ProjectUserRole.Member, projectDescriptorResult.Value, users.Where(u => u.IsMember(component.ProjectId)).Select(u => GetUserDescriptor(u)))

                ).ConfigureAwait(false);


            return component;

            string GetUserDescriptor(User user) => user is null ? null : user.UserType switch
            {
                UserType.User => aadGraphUsers.GetValueOrDefault(user.Id),
                UserType.Group => aadGraphGroups.GetValueOrDefault(user.Id),
                _ => default
            };

            async ValueTask<bool> EnsureUserAsync(User user, MemberEntitlementManagementHttpClient memberClient)
            {
                var userExists = false;

                try
                {
                    switch (user.UserType)
                    {
                        case UserType.User:

                            if (!aadGraphUsers.ContainsKey(user.Id))
                            {
                                var graphUser = await graphClient
                                    .CreateUserAsync(new GraphUserOriginIdCreationContext { OriginId = user.Id })
                                    .ConfigureAwait(false);

                                var entitlement = new UserEntitlement()
                                {
                                    User = graphUser,
                                    AccessLevel = new AccessLevel
                                    {
                                        AccountLicenseType = AccountLicenseType.Express
                                    }
                                };

                                await memberClient
                                    .AddUserEntitlementAsync(entitlement)
                                    .ConfigureAwait(false);

                                aadGraphUsers.TryAdd(graphUser.OriginId, graphUser.Descriptor);
                            }

                            userExists = true;

                            break;

                        case UserType.Group:

                            if (!aadGraphGroups.ContainsKey(user.Id))
                            {
                                var graphGroup = await graphClient
                                    .CreateGroupAsync(new GraphGroupOriginIdCreationContext { OriginId = user.Id })
                                    .ConfigureAwait(false);

                                var entitlement = new GroupEntitlement()
                                {
                                    Group = graphGroup,
                                    LicenseRule = new AccessLevel
                                    {
                                        AccountLicenseType = AccountLicenseType.Express
                                    }
                                };

                                await memberClient
                                    .AddGroupEntitlementAsync(entitlement)
                                    .ConfigureAwait(false);

                                aadGraphGroups.TryAdd(graphGroup.OriginId, graphGroup.Descriptor);
                            }

                            userExists = true;

                            break;
                    }
                }
                catch
                {
                    // swallow and assume the user doesn't exist
                }

                return userExists;
            }

            async Task SyncGroupAsync(ProjectUserRole projectRole, string projectDescriptor, IEnumerable<string> userDescriptors)
            {
                var groupName = $"TeamCloud Project {projectRole}s";

                var group = await graphClient
                    .ListAllGroupsAsync(projectDescriptor)
                    .FirstOrDefaultAsync(g =>
                        g.Origin.Equals("vsts", StringComparison.OrdinalIgnoreCase) &&
                        g.DisplayName.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                    .ConfigureAwait(false);

                var memberDescriptors = Enumerable.Empty<string>();

                if (group is null)
                {
                    var groupContext = new GraphGroupVstsCreationContext
                    {
                        DisplayName = groupName,
                        Description = "Managed by TeamCloud"
                    };

                    group = await graphClient
                        .CreateGroupAsync(groupContext, projectDescriptor)
                        .ConfigureAwait(false);
                }
                else
                {
                    var memberships = await graphClient
                        .ListMembershipsAsync(group.Descriptor, GraphTraversalDirection.Down, depth: 1)
                        .ConfigureAwait(false);

                    memberDescriptors = memberships
                        .Select(membership => membership.MemberDescriptor.ToString());
                }

                var defaultTeamDescriptor = await graphClient
                    .GetDescriptorAsync(teamProject.DefaultTeam.Id)
                    .ConfigureAwait(false);

                var defaultTeamGroup = await graphClient
                    .GetGroupAsync(defaultTeamDescriptor.Value)
                    .ConfigureAwait(false);

                if (!await graphClient.CheckMembershipExistenceAsync(group.Descriptor, defaultTeamGroup.Descriptor).ConfigureAwait(false))
                {
                    // every group managed by TeamCloud will become a member of the default team of the target project in Azure DevOps

                    await graphClient
                        .AddMembershipAsync(group.Descriptor, defaultTeamGroup.Descriptor)
                        .ConfigureAwait(false);
                }

                if (projectRole == ProjectUserRole.Owner || projectRole == ProjectUserRole.Admin)
                {
                    var projectAdministratorsGroup = await graphClient
                        .ListAllGroupsAsync(projectDescriptor)
                        .FirstOrDefaultAsync(g =>
                            g.Origin.Equals("vsts", StringComparison.OrdinalIgnoreCase) &&
                            g.DisplayName.Equals("Project Administrators", StringComparison.Ordinal))
                        .ConfigureAwait(false);

                    if (projectAdministratorsGroup != null && !await graphClient.CheckMembershipExistenceAsync(group.Descriptor, projectAdministratorsGroup.Descriptor).ConfigureAwait(false))
                    {
                        // owner and admin groups managed by TeamCloud will become a member of the Project Administrators group of the target project in Azure DevOps

                        await graphClient
                            .AddMembershipAsync(group.Descriptor, projectAdministratorsGroup.Descriptor)
                            .ConfigureAwait(false);
                    }
                }

                await Task.WhenAll(

                    userDescriptors
                        .Except(memberDescriptors)
                        .Select(userDescriptor => graphClient.AddMembershipAsync(userDescriptor, group.Descriptor))
                        .WhenAll(),

                    memberDescriptors
                        .Except(userDescriptors)
                        .Select(userDescriptor => graphClient.RemoveMembershipAsync(userDescriptor, group.Descriptor))
                        .WhenAll()

                    ).ConfigureAwait(false);
            }
        });

        public override Task<Component> DeleteComponentAsync(Component component, IAsyncCollector<ICommand> commandQueue, ILogger log) => ExecuteAsync(component, commandQueue, log, false, (teamProject) =>
        {
            return Task.FromResult(component);
        });

        public override async Task<NetworkCredential> GetServiceCredentialAsync(Component component)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId, true)
                .ConfigureAwait(false);

            var token = await TokenClient
                .GetAsync<AzureDevOpsToken>(deploymentScope)
                .ConfigureAwait(false);

            if (token is null)
            {
                return null; // no token - no client
            }
            else if (token.AccessTokenExpired)
            {
                if (token.RefreshTokenExpired)
                    throw new Exception("Refresh");

                token = await RefreshTokenAsync(token)
                    .ConfigureAwait(false);
            }

            return token is null ? null : new NetworkCredential("bearer", token.AccessToken, token.Organization);
        }
    }
}
