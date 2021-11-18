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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.DevOps.Licensing.WebApi;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Licensing;
using Microsoft.VisualStudio.Services.MemberEntitlementManagement.WebApi;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.VisualStudio.Services.ServiceEndpoints;
using Microsoft.VisualStudio.Services.ServiceEndpoints.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Azure.Directory;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Secrets;
using TeamCloud.Serialization;
using TeamCloud.Serialization.Forms;
using TeamCloud.Templates;
using EndpointAuthorization = Microsoft.VisualStudio.Services.ServiceEndpoints.WebApi.EndpointAuthorization;
using IHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;
using ProjectReference = Microsoft.VisualStudio.Services.ServiceEndpoints.WebApi.ProjectReference;
using ServiceEndpoint = Microsoft.VisualStudio.Services.ServiceEndpoints.WebApi.ServiceEndpoint;
using User = TeamCloud.Model.Data.User;
using UserType = TeamCloud.Model.Data.UserType;

namespace TeamCloud.Adapters.AzureDevOps
{
    public sealed class AzureDevOpsAdapter : AdapterWithIdentity, IAdapterAuthorize
    {
        private const string VisualStudioAuthUrl = "https://app.vssps.visualstudio.com/oauth2/authorize";
        private const string VisualStudioTokenUrl = "https://app.vssps.visualstudio.com/oauth2/token";

        private readonly ILogger log;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IUserRepository userRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IAzureResourceService azureResourceService;
        private readonly IFunctionsHost functionsHost;

        public AzureDevOpsAdapter(
            IAuthorizationSessionClient sessionClient,
            IAuthorizationTokenClient tokenClient,
            IDistributedLockManager distributedLockManager,
            ISecretsStoreProvider secretsStoreProvider,
            IHttpClientFactory httpClientFactory,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IDeploymentScopeRepository deploymentScopeRepository,
            IProjectRepository projectRepository,
            IComponentRepository componentRepository,
            IComponentTemplateRepository componentTemplateRepository,
            IAzureSessionService azureSessionService,
            IAzureResourceService azureResourceService,
            IAzureDirectoryService azureDirectoryService,
            IFunctionsHost functionsHost = null,
            ILoggerFactory loggerFactory = null)
            : base(sessionClient, tokenClient, distributedLockManager, secretsStoreProvider, azureSessionService, azureDirectoryService, organizationRepository, deploymentScopeRepository, projectRepository, userRepository)
        {
            this.httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.functionsHost = functionsHost ?? FunctionsHost.Default;

            log = loggerFactory.CreateLogger(this.GetType());
        }

        public override DeploymentScopeType Type
            => DeploymentScopeType.AzureDevOps;

        public override IEnumerable<ComponentType> ComponentTypes
            => new ComponentType[] { ComponentType.Repository };

        public override string DisplayName
            => "Azure DevOps";

        public override Task<string> GetInputDataSchemaAsync()
            => TeamCloudForm.GetDataSchemaAsync<AzureDevOpsData>()
            .ContinueWith(t => t.Result.ToString(), TaskContinuationOptions.OnlyOnRanToCompletion);

        public override Task<string> GetInputFormSchemaAsync()
            => TeamCloudForm.GetFormSchemaAsync<AzureDevOpsData>()
            .ContinueWith(t => t.Result.ToString(), TaskContinuationOptions.OnlyOnRanToCompletion);

        public override async Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var token = await TokenClient
                .GetAsync<AzureDevOpsToken>(deploymentScope)
                .ConfigureAwait(false);

            return !(token is null);
        }

        Task<AzureServicePrincipal> IAdapterAuthorize.ResolvePrincipalAsync(DeploymentScope deploymentScope, HttpRequest request)
            => Task.FromResult<AzureServicePrincipal>(null);

        async Task<IActionResult> IAdapterAuthorize.HandleAuthorizeAsync(DeploymentScope deploymentScope, HttpRequest request, IAuthorizationEndpoints authorizationEndpoints)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (authorizationEndpoints is null)
                throw new ArgumentNullException(nameof(authorizationEndpoints));

            var authorizationSession = await SessionClient
                .GetAsync<AzureDevOpsSession>(deploymentScope)
                .ConfigureAwait(false);

            authorizationSession ??= await SessionClient
                .SetAsync(new AzureDevOpsSession(deploymentScope))
                .ConfigureAwait(false);

            var task = request.Method.ToUpperInvariant() switch
            {
                "GET" => HandleAuthorizeGetAsync(deploymentScope, authorizationSession, request, authorizationEndpoints),
                "POST" => HandleAuthorizePostAsync(deploymentScope, authorizationSession, request, authorizationEndpoints),
                _ => Task.FromException<IActionResult>(new NotImplementedException())
            };

            return await task.ConfigureAwait(false);
        }

        async Task<IActionResult> IAdapterAuthorize.HandleCallbackAsync(DeploymentScope deploymentScope, HttpRequest request, IAuthorizationEndpoints authorizationEndpoints)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (authorizationEndpoints is null)
                throw new ArgumentNullException(nameof(authorizationEndpoints));

            var authorizationSession = await SessionClient
                .GetAsync<AzureDevOpsSession>(deploymentScope)
                .ConfigureAwait(false);

            if (authorizationSession is null)
            {
                return new NotFoundResult();
            }

            if (request.Query.TryGetValue("error", out var error))
            {
                return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", error));
            }
            else if (!request.Query.TryGetValue("state", out var state) || !authorizationSession.SessionState.Equals(state, StringComparison.OrdinalIgnoreCase))
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
                    assertion = request.Query.GetValueOrDefault("code").ToString(),
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

            static async Task<string> GetErrorDescriptionAsync(HttpResponseMessage responseMessage)
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

        private async Task<IActionResult> HandleAuthorizeGetAsync(DeploymentScope deploymentScope, AzureDevOpsSession authorizationSession, HttpRequest request, IAuthorizationEndpoints authorizationEndpoints)
        {
            var queryError = request.Query.GetValueOrDefault("error").ToString();

            if (!string.IsNullOrEmpty(queryError))
            {
                log.LogWarning($"Authorization failed: {queryError}");
            }
            else if (request.Query.ContainsKey("succeeded"))
            {
                log.LogInformation($"Authorization succeeded");
            }

            var data = string.IsNullOrWhiteSpace(deploymentScope.InputData)
                ? default
                : TeamCloudSerialize.DeserializeObject<AzureDevOpsData>(deploymentScope.InputData);

            return new ContentResult
            {
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "text/html",
                Content = Assembly.GetExecutingAssembly().GetManifestResourceTemplate($"{GetType().FullName}.html", new
                {
                    applicationWebsite = functionsHost.HostUrl,
                    applicationCallback = authorizationEndpoints.CallbackUrl,
                    data = data,
                    session = authorizationSession,
                    error = queryError ?? string.Empty,
                    succeeded = request.Query.ContainsKey("succeeded")
                })
            };
        }

        private async Task<IActionResult> HandleAuthorizePostAsync(DeploymentScope deploymentScope, AzureDevOpsSession authorizationSession, HttpRequest request, IAuthorizationEndpoints authorizationEndpoints)
        {
            var payload = await request
                .ReadStringAsync()
                .ConfigureAwait(false);

            var payloadParams = Url.ParseQueryParams(payload);

            var organization = payloadParams.GetValueOrDefault("organization", StringComparison.OrdinalIgnoreCase);
            var personalAccessToken = payloadParams.GetValueOrDefault("pat", StringComparison.OrdinalIgnoreCase);

            string redirectUrl;

            if (string.IsNullOrEmpty(organization))
            {
                redirectUrl = authorizationEndpoints
                    .AuthorizationUrl
                    .SetQueryParam("error", "Missing organization name or URL.")
                    .ToString();
            }
            else if (string.IsNullOrEmpty(personalAccessToken))
            {
                authorizationSession.Organization = organization;
                authorizationSession.ClientId = payloadParams.GetValueOrDefault("client_id", StringComparison.OrdinalIgnoreCase);
                authorizationSession.ClientSecret = payloadParams.GetValueOrDefault("client_secret", StringComparison.OrdinalIgnoreCase);
                authorizationSession = await SessionClient.SetAsync(authorizationSession).ConfigureAwait(false);

                redirectUrl = VisualStudioAuthUrl
                    .SetQueryParam("client_id", authorizationSession.ClientId)
                    .SetQueryParam("response_type", "Assertion")
                    .SetQueryParam("state", authorizationSession.SessionState)
                    .SetQueryParam("scope", string.Join(' ', AzureDevOpsSession.Scopes))
                    .SetQueryParam("redirect_uri", authorizationEndpoints.CallbackUrl)
                    .ToString();
            }
            else
            {
                _ = await TokenClient
                    .SetAsync(new AzureDevOpsToken(deploymentScope)
                    {
                        Organization = organization,
                        PersonalAccessToken = personalAccessToken
                    })
                    .ConfigureAwait(false);

                redirectUrl = authorizationEndpoints
                    .AuthorizationUrl
                    .SetQueryParam("succeeded")
                    .ToString();
            }

            return new RedirectResult(redirectUrl);
        }

        private async Task<T> CreateClientAsync<T>(DeploymentScope deploymentScope, bool patAuthRequired = false)
            where T : VssHttpClientBase
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var token = await TokenClient
                .GetAsync<AzureDevOpsToken>(deploymentScope)
                .ConfigureAwait(false);

            if (token is null || (patAuthRequired && string.IsNullOrEmpty(token.PersonalAccessToken)))
            {
                // there are two scenarios that can lead to this
                // a) the obvious one - there is no token object available
                // b) there is a token object available, but the requisted client requires PAT auth.
                //    unfortunately this case is pretty common, as some of the Azure DevOps REST APIs
                //    are not yet migrated to OAuth2 auth and therefore fail with an unauthorized result.

                return null;
            }
            else if (string.IsNullOrEmpty(token.PersonalAccessToken) && token.AccessTokenExpired)
            {
                if (token.RefreshTokenExpired)
                    throw new Exception("Refresh");

                token = await RefreshTokenAsync(token)
                    .ConfigureAwait(false);
            }

#pragma warning disable CA2000 // Dispose objects before losing scope

            var credentials = string.IsNullOrEmpty(token.PersonalAccessToken)
                ? (VssCredentials)new VssOAuthAccessTokenCredential(accessToken: token.AccessToken)
                : (VssCredentials)new VssBasicCredential(string.Empty, token.PersonalAccessToken);

            var connection = new VssConnection(
                new Uri(token.Organization),
                new VssHttpMessageHandler(
                    credentials,
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
                _ = await response
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

        private async Task<Component> ExecuteAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue, bool ensureResourceExists, Func<TeamProject, Task<Component>> callback)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (componentOrganization is null)
                throw new ArgumentNullException(nameof(componentOrganization));

            if (componentDeploymentScope is null)
                throw new ArgumentNullException(nameof(componentDeploymentScope));

            if (componentProject is null)
                throw new ArgumentNullException(nameof(componentProject));            

            if (contextUser is null)
                throw new ArgumentNullException(nameof(contextUser));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            using var projectClient = await CreateClientAsync<ProjectHttpClient>(componentDeploymentScope).ConfigureAwait(false);

            var projectName = $"{componentOrganization.DisplayName} - {componentProject.DisplayName}";

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
                    using var processClient = await CreateClientAsync<ProcessHttpClient>(componentDeploymentScope).ConfigureAwait(false);

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
                        Name = await projectClient.GenerateProjectNameAsync(projectName).ConfigureAwait(false),
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
                        await WaitForOperationAsync(componentDeploymentScope, projectOperation.Id).ConfigureAwait(false);

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

                if (project is not null && !AzureDevOpsIdentifier.TryParse(component.ResourceId, out var _))
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

        protected override Task<Component> CreateComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue) 
            => ExecuteAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue, true, async (teamProject) =>
        {
            await using (var teamProjectLock = await AcquireLockAsync(nameof(AzureDevOpsAdapter), teamProject.Id.ToString()).ConfigureAwait(false))
            {
                var resourceId = AzureDevOpsIdentifier.Parse(component.ResourceId);
                var resourceIdUpdated = false;

                using var gitClient = await CreateClientAsync<GitHttpClient>(componentDeploymentScope).ConfigureAwait(false);

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
                    var repoSettings = new GitRepository()
                    {
                        Name = component.DisplayName,
                        ProjectReference = teamProject
                    };

                    gitRepo = await gitClient
                        .CreateRepositoryAsync(repoSettings)
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

                var hasCommits = await gitClient
                    .HasCommitsAsync(gitRepo.Id)
                    .ConfigureAwait(false);

                if (!hasCommits)
                {
                    var componentTemplate = await componentTemplateRepository
                        .GetAsync(component.Organization, component.ProjectId, component.TemplateId)
                        .ConfigureAwait(false) as ComponentRepositoryTemplate;

                    if (!string.IsNullOrEmpty(componentTemplate?.Configuration.TemplateRepository))
                    {
                        var importRequest = new GitImportRequest()
                        {
                            Parameters = new GitImportRequestParameters()
                            {
                                GitSource = new GitImportGitSource() { Url = componentTemplate.Configuration.TemplateRepository },
                                DeleteServiceEndpointAfterImportIsDone = true
                            }
                        };

                        await gitClient
                            .CreateImportRequestAsync(importRequest, teamProject.Id, gitRepo.Id)
                            .ConfigureAwait(false);
                    }
                }
            }

            return await UpdateComponentAsync(component, contextUser, commandQueue).ConfigureAwait(false);
        });

        protected override Task<Component> UpdateComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue)
            => ExecuteAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue, false, async (teamProject) =>
        {
            if (teamProject is null)
                throw new ArgumentNullException(nameof(teamProject));

            await using (var teamProjectLock = await AcquireLockAsync(nameof(AzureDevOpsAdapter), teamProject.Id.ToString()).ConfigureAwait(false))
            {
                using var graphClient = await CreateClientAsync<GraphHttpClient>(componentDeploymentScope).ConfigureAwait(false);
                using var entitlementClient = await CreateClientAsync<MemberEntitlementManagementHttpClient>(componentDeploymentScope).ConfigureAwait(false);
                using var teamClient = await CreateClientAsync<TeamHttpClient>(componentDeploymentScope).ConfigureAwait(false);

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

                    SyncTeamCloudIdentityAsync(),
                    SyncVariableGroupsAsync(),

                    SyncGroupAsync(ProjectUserRole.Admin, projectDescriptorResult.Value, users.Where(u => u.IsAdmin(component.ProjectId)).Select(u => GetUserDescriptor(u))),
                    SyncGroupAsync(ProjectUserRole.Owner, projectDescriptorResult.Value, users.Where(u => u.IsOwner(component.ProjectId)).Select(u => GetUserDescriptor(u))),
                    SyncGroupAsync(ProjectUserRole.Member, projectDescriptorResult.Value, users.Where(u => u.IsMember(component.ProjectId)).Select(u => GetUserDescriptor(u)))

                    ).ConfigureAwait(false);

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

                        if (projectAdministratorsGroup is not null && !await graphClient.CheckMembershipExistenceAsync(group.Descriptor, projectAdministratorsGroup.Descriptor).ConfigureAwait(false))
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

                async Task SyncTeamCloudIdentityAsync()
                {
                    const string SERVICE_ENDPOINT_PREFIX = "TeamCloud";

                    // CAUTION: the REST API used by the ServiceEndpointHttpClient requires PAT auth when
                    // processing Create, Update, and Delete operations (Read is already support OAuth2 auth).

                    // Instead of throwing an exception we just skip the variable group sync and move on - assuming this is a non essential feature

                    var serviceEndpointClient = await CreateClientAsync<ServiceEndpointHttpClient>(componentDeploymentScope, patAuthRequired: true).ConfigureAwait(false);

                    if (serviceEndpointClient is null)
                    {
                        log.LogWarning($"Skipping variable group sync -> The adapter is not authorized or is not authorized using a Personal Access Token (PAT)");
                    }
                    else
                    {
                        var servicePrincipal = await GetServiceIdentityAsync(component)
                            .ConfigureAwait(false);

                        var serviceEndpoints = await serviceEndpointClient
                            .GetServiceEndpointsAsync(teamProject.Id, "AzureRM")
                            .ConfigureAwait(false);

                        if (AzureResourceIdentifier.TryParse(componentProject.ResourceId, out var projectResourceId))
                        {
                            // the corresponding TeamCloud project has already a resource group assigned and
                            // therefore has a 'home subscription' - so we are ready to create/update a service endpoint.

                            var projectResourceGroup = await azureResourceService
                                .GetResourceGroupAsync(projectResourceId.SubscriptionId, projectResourceId.ResourceGroup)
                                .ConfigureAwait(false);

                            if (projectResourceGroup is not null)
                            {
                                // ensure our service principal has at least read access on a restricted scope
                                // inside of the project's home subscription. otherwise the corresponding service
                                // endpoint won't be able to authenticate to Azure at all.

                                await projectResourceGroup
                                    .AddRoleAssignmentAsync(servicePrincipal.ObjectId.ToString(), AzureRoleDefinition.Reader)
                                    .ConfigureAwait(false);
                            }

                            var serviceEndpointName = $"{SERVICE_ENDPOINT_PREFIX} {componentProject.DisplayName}";

                            var serviceEndpoint = serviceEndpoints
                                .SingleOrDefault(se => se.Name.Equals(serviceEndpointName, StringComparison.OrdinalIgnoreCase));

                            if (serviceEndpoint is null)
                            {
                                serviceEndpoint = new ServiceEndpoint()
                                {
                                    Name = serviceEndpointName,
                                    Type = "AzureRM",
                                    Url = new Uri("https://management.azure.com/"),
                                    IsShared = false,
                                    IsReady = true,
                                    Authorization = new EndpointAuthorization()
                                    {
                                        Scheme = "ServicePrincipal",
                                        Parameters = new Dictionary<string, string>()
                                        {
                                            { "tenantid", servicePrincipal.TenantId.ToString() },
                                            { "serviceprincipalid", servicePrincipal.ApplicationId.ToString() },
                                            { "authenticationType", "spnKey" },
                                            { "serviceprincipalkey", servicePrincipal.Password }
                                        }
                                    },
                                    ServiceEndpointProjectReferences = new List<ServiceEndpointProjectReference>()
                                    {
                                        new ServiceEndpointProjectReference()
                                        {
                                            Name = serviceEndpointName,
                                            ProjectReference = new ProjectReference()
                                            {
                                                Id = teamProject.Id,
                                                Name = teamProject.Name
                                            }
                                        }
                                    },
                                    Data = new Dictionary<string, string>()
                                    {
                                        { "subscriptionId", projectResourceId.SubscriptionId.ToString() },
                                        { "subscriptionName", projectResourceId.SubscriptionId.ToString() },
                                        { "environment", "AzureCloud" },
                                        { "scopeLevel", "Subscription"},
                                        { "creationMode", "Manual" }
                                    }
                                };

                                await serviceEndpointClient
                                    .CreateServiceEndpointAsync(serviceEndpoint)
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                serviceEndpoint.Authorization = new EndpointAuthorization()
                                {
                                    Scheme = "ServicePrincipal",
                                    Parameters = new Dictionary<string, string>()
                                    {
                                        { "tenantid", servicePrincipal.TenantId.ToString() },
                                        { "serviceprincipalid", servicePrincipal.ApplicationId.ToString() },
                                        { "authenticationType", "spnKey" },
                                        { "serviceprincipalkey", servicePrincipal.Password }
                                    }
                                };

                                serviceEndpoint.ServiceEndpointProjectReferences = new List<ServiceEndpointProjectReference>()
                                {
                                    new ServiceEndpointProjectReference()
                                    {
                                        Name = serviceEndpointName,
                                        ProjectReference = new ProjectReference()
                                        {
                                            Id = teamProject.Id,
                                            Name = teamProject.Name
                                        }
                                    }
                                };

                                await serviceEndpointClient
                                    .UpdateServiceEndpointAsync(serviceEndpoint.Id, serviceEndpoint)
                                    .ConfigureAwait(false);
                            }
                        }
                    }
                }

                async Task SyncVariableGroupsAsync()
                {
                    const string VARIABLE_GROUP_PREFIX = "TeamCloud";

                    // CAUTION: the REST API used by the TaskAgentHttpClient requires PAT auth when
                    // processing Create, Update, and Delete operations (Read is already support OAuth2 auth).

                    // Instead of throwing an exception we just skip the variable group sync and move on - assuming this is a non essential feature

                    using var taskAgentClient = await CreateClientAsync<TaskAgentHttpClient>(componentDeploymentScope, patAuthRequired: true).ConfigureAwait(false);

                    if (taskAgentClient is null)
                    {
                        log.LogWarning($"Skipping variable group sync -> The adapter is not authorized or is not authorized using a Personal Access Token (PAT)");
                    }
                    else
                    {
                        var variableGroups = await taskAgentClient
                            .GetVariableGroupsAsync(teamProject.Id, $"{VARIABLE_GROUP_PREFIX}*")
                            .ConfigureAwait(false);

                        var variableLookups = new Dictionary<string, Func<IDictionary<string, VariableValue>>>()
                        {
                            { $"{VARIABLE_GROUP_PREFIX}Tags", () => componentProject.Tags.ToDictionary(k => k.Key, v => new VariableValue() { Value = v.Value, IsReadOnly = true, IsSecret = false }) },
                            { $"{VARIABLE_GROUP_PREFIX}Props", () => new Dictionary<string, VariableValue>() }
                        };

                        var tasks = new List<Task>();

                        foreach (var variableGroup in variableGroups)
                        {
                            if (variableLookups.TryGetValue(variableGroup.Name, out var variableLookup))
                            {
                                try
                                {
                                    variableGroup.Variables = variableLookup();

                                    if (variableGroup.Variables.Any())
                                    {
                                        tasks.Add(taskAgentClient.UpdateVariableGroupAsync(teamProject, variableGroup));
                                        continue; // skip any further process and avoid variable group delete
                                    }
                                }
                                finally
                                {
                                    variableLookups.Remove(variableGroup.Name);
                                }
                            }

                            tasks.Add(taskAgentClient.DeleteVariableGroupAsync(variableGroup.Id, new string[] { teamProject.Id.ToString() }));
                        }

                        foreach (var variableLookup in variableLookups)
                        {
                            var variables = variableLookup.Value();

                            if (variables.Any())
                            {
                                tasks.Add(taskAgentClient.AddVariableGroupAsync(teamProject, variableLookup.Key, variables));
                            }
                        }

                        await tasks.WhenAll().ConfigureAwait(false);
                    }
                }
            }

            return component;
        });

        protected override Task<Component> DeleteComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue)
            => ExecuteAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue, false, async (teamProject) =>
        {
            if (teamProject is null)
            {
                // as there is no AzDO project available
                // we don't need to do any cleanup work

                return component;
            }

            await using (var teamProjectLock = await AcquireLockAsync(nameof(AzureDevOpsAdapter), teamProject.Id.ToString()).ConfigureAwait(false))
            {
                var resourceId = AzureDevOpsIdentifier.Parse(component.ResourceId);

                using var gitClient = await CreateClientAsync<GitHttpClient>(componentDeploymentScope).ConfigureAwait(false);

                if (resourceId.TryGetResourceValue("repositories", true, out var repositoryValue) && Guid.TryParse(repositoryValue, out var repositoryId))
                {
                    var gitRepo = await gitClient
                        .GetRepositoryAsync(repositoryId)
                        .ConfigureAwait(false);

                    if (gitRepo is not null)
                    {
                        await gitClient
                            .DeleteRepositoryAsync(gitRepo.ProjectReference.Id, gitRepo.Id)
                            .ConfigureAwait(false);
                    }
                }

                component.ResourceId = null;
                component.ResourceUrl = null;

                component = await componentRepository
                    .SetAsync(component)
                    .ConfigureAwait(false);
            }

            return component;
        });

        public override Task<NetworkCredential> GetServiceCredentialAsync(Component component)
            => WithContextAsync(component, async (componentOrganization, componentDeploymentScope, comonentProject) =>
            {
                var token = await TokenClient
                    .GetAsync<AzureDevOpsToken>(componentDeploymentScope)
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
            });
    }
}
