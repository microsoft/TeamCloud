/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Octokit;
using Octokit.Internal;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Azure.Directory;
using TeamCloud.Azure.Resources;
using TeamCloud.Data;
using TeamCloud.Http;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;
using TeamCloud.Serialization.Forms;
using TeamCloud.Templates;
using IHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;
using User = TeamCloud.Model.Data.User;

namespace TeamCloud.Adapters.GitHub
{
    public sealed partial class GitHubAdapter : Adapter, IAdapterAuthorize
    {
        private static readonly IJsonSerializer GitHubSerializer = new SimpleJsonSerializer();

        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOrganizationRepository organizationRepository;
        private readonly IUserRepository userRepository;
        private readonly IDeploymentScopeRepository deploymentScopeRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentTemplateRepository componentTemplateRepository;
        private readonly IAzureSessionService azureSessionService;
        private readonly IAzureResourceService azureResourceService;
        private readonly IAzureDirectoryService azureDirectoryService;
        private readonly IFunctionsHost functionsHost;

        public GitHubAdapter(
            IAuthorizationSessionClient sessionClient,
            IAuthorizationTokenClient tokenClient,
            IDistributedLockManager distributedLockManager,
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
            IFunctionsHost functionsHost = null)
            : base(sessionClient, tokenClient, distributedLockManager)
        {
            this.httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();
            this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentTemplateRepository = componentTemplateRepository ?? throw new ArgumentNullException(nameof(componentTemplateRepository));
            this.azureSessionService = azureSessionService ?? throw new ArgumentNullException(nameof(azureSessionService));
            this.azureResourceService = azureResourceService ?? throw new ArgumentNullException(nameof(azureResourceService));
            this.azureDirectoryService = azureDirectoryService ?? throw new ArgumentNullException(nameof(azureDirectoryService));
            this.functionsHost = functionsHost ?? FunctionsHost.Default;
        }

        public override DeploymentScopeType Type
            => DeploymentScopeType.GitHub;

        public override IEnumerable<ComponentType> ComponentTypes
            => new ComponentType[] { ComponentType.Repository };

        public override string DisplayName
            => base.DisplayName.Replace(" ", "", StringComparison.OrdinalIgnoreCase);

        public override Task<string> GetInputDataSchemaAsync()
            => TeamCloudForm.GetDataSchemaAsync<GitHubData>()
            .ContinueWith(t => t.Result.ToString(), TaskContinuationOptions.OnlyOnRanToCompletion);

        public override Task<string> GetInputFormSchemaAsync()
            => TeamCloudForm.GetFormSchemaAsync<GitHubData>()
            .ContinueWith(t => t.Result.ToString(), TaskContinuationOptions.OnlyOnRanToCompletion);

        public override async Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var token = await TokenClient
                .GetAsync<GitHubToken>(deploymentScope)
                .ConfigureAwait(false);

            return !(token is null);
        }

        Task IAdapterAuthorize.CreateSessionAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            if (deploymentScope.Type != Type)
                throw new ArgumentException("Argument value can not be handled", nameof(deploymentScope));

            return SessionClient.SetAsync<GitHubSession>(new GitHubSession(deploymentScope));
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

            var queryParams = Url.ParseQueryParams(requestMessage.RequestUri.Query);
            var queryState = queryParams.GetValueOrDefault("state", StringComparison.OrdinalIgnoreCase);
            var queryCode = queryParams.GetValueOrDefault("code", StringComparison.OrdinalIgnoreCase);
            var queryError = queryParams.GetValueOrDefault("error", StringComparison.OrdinalIgnoreCase);

            var authorizationSession = await SessionClient
                .GetAsync<GitHubSession>(deploymentScope)
                .ConfigureAwait(false);

            var data = string.IsNullOrWhiteSpace(deploymentScope.InputData)
                ? default
                : TeamCloudSerialize.DeserializeObject<GitHubData>(deploymentScope.InputData);

            var token = await TokenClient
                .GetAsync<GitHubToken>(deploymentScope)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(queryError) && Guid.TryParse(queryState, out var stateId))
            {

                if (!stateId.ToString().Equals(authorizationSession.SessionId, StringComparison.OrdinalIgnoreCase))
                {
                    return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", "Session timed out.").ToString());
                }
                else if (string.IsNullOrWhiteSpace(queryCode))
                {
                    return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", "Missing GitHub handshake information.").ToString());
                }

                token ??= new GitHubToken(deploymentScope);

                // Using Flurl as Octokit doesn't support this API yet
                // https://github.com/octokit/octokit.net/issues/2138

                var url = $"https://api.github.com/app-manifests/{queryCode}/conversions";

                var response = await url
                    .WithHeader("User-Agent", GitHubConstants.ProductHeader.ToString())
                    .PostStringAsync(string.Empty)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", $"Failed to get application token ({response.StatusCode} - {response.ReasonPhrase}).").ToString());
                }

                var json = await response.Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                TeamCloudSerialize.PopulateObject(json, token);

                token = await TokenClient
                    .SetAsync(token, true)
                    .ConfigureAwait(false);

                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    ContentType = "text/html",
                    Content = Assembly.GetExecutingAssembly().GetManifestResourceTemplate($"{GetType().FullName}_Install.html", GetFormContext())
                };
            }
            else
            {
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    ContentType = "text/html",
                    Content = Assembly.GetExecutingAssembly().GetManifestResourceTemplate($"{GetType().FullName}_Register.html", GetFormContext())
                };
            }

            object GetFormContext() => new
            {
                deploymentScope = deploymentScope,
                authorizationEndpoints = authorizationEndpoints,
                token = token,
                data = data,
                session = authorizationSession,
                error = queryError ?? string.Empty,
                succeeded = queryParams.ContainsKey("succeeded")
            };
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

            var json = await requestMessage.Content
                .ReadAsJsonAsync()
                .ConfigureAwait(false);

            var appId = json
                .SelectToken("installation.app_id")?
                .ToString();

            var token = await TokenClient
                .GetAsync<GitHubToken>(deploymentScope)
                .ConfigureAwait(false);

            if (token?.ApplicationId?.Equals(appId, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var action = json
                    .SelectToken("action")?
                    .ToString();

                switch (action)
                {
                    case "created":

                        token.InstallationId = json
                            .SelectToken("installation.id")?
                            .ToString();

                        break;

                    case "deleted":

                        token = new GitHubToken(deploymentScope);
                        break;

                    case "suspend":

                        token.Suspended = true;
                        break;

                    case "unsuspend":

                        token.Suspended = false;
                        break;

                    default:

                        token = null;
                        break;
                }

                if (token != null)
                {
                    _ = await TokenClient
                        .SetAsync(token)
                        .ConfigureAwait(false);

                    return new OkResult();
                }
            }

            return new NotFoundResult();

        }

        private async Task<GitHubClient> CreateClientAsync(Component component, string acceptHeader = null)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId, true)
                .ConfigureAwait(false);

            return await CreateClientAsync(deploymentScope, acceptHeader)
                .ConfigureAwait(false);
        }

        private async Task<GitHubClient> CreateClientAsync(DeploymentScope deploymentScope, string acceptHeader = null)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var token = await TokenClient
                .GetAsync<GitHubToken>(deploymentScope)
                .ConfigureAwait(false);

            return await CreateClientAsync(token, acceptHeader)
                .ConfigureAwait(false);
        }

        private async Task<GitHubClient> CreateClientAsync(GitHubToken token, string acceptHeader = null)
        {
            if (token is null)
                throw new ArgumentNullException(nameof(token));

            GitHubClient gitHubClient = null;

            if (token.Enabled)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope

                // this is a factory methods - we must not dispose the http client instance here as this would make the client unusable for the caller

                var gitHubHttpClient = new GitHubInterceptor(new HttpClientAdapter(HttpMessageHandlerFactory.CreateDefault), acceptHeader);

#pragma warning restore CA2000 // Dispose objects before losing scope

                if (token.AccessTokenExpired)
                {
                    gitHubClient = new GitHubClient(new Connection(GitHubConstants.ProductHeader, gitHubHttpClient)
                    {
                        Credentials = new Credentials(token.GetPemToken(), AuthenticationType.Bearer)
                    });

                    try
                    {
                        var accessToken = await gitHubClient.GitHubApps
                            .CreateInstallationToken(long.Parse(token.InstallationId, CultureInfo.InvariantCulture))
                            .ConfigureAwait(false);

                        token.AccessToken = accessToken.Token;
                        token.AccessTokenExpires = accessToken.ExpiresAt.UtcDateTime;

                        token = await TokenClient
                            .SetAsync(token, true)
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        // swallow
                    }
                }

                gitHubClient = token.AccessTokenExpired ? null : new GitHubClient(new Connection(GitHubConstants.ProductHeader, gitHubHttpClient)
                {
                    Credentials = new Credentials(token.AccessToken)
                });
            }

            return gitHubClient;
        }

        private async Task<Component> ExecuteAsync(Component component, IAsyncCollector<ICommand> commandQueue, ILogger log, Func<GitHubClient, Octokit.User, Task<Component>> callback)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (commandQueue is null)
                throw new ArgumentNullException(nameof(commandQueue));

            if (log is null)
                throw new ArgumentNullException(nameof(log));

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            var deploymentScope = await deploymentScopeRepository
                .GetAsync(component.Organization, component.DeploymentScopeId, true)
                .ConfigureAwait(false);

            var token = await TokenClient
                .GetAsync<GitHubToken>(deploymentScope)
                .ConfigureAwait(false);

            var gitHubClient = await CreateClientAsync(token)
                .ConfigureAwait(false);

            return await callback(gitHubClient, token.Owner).ConfigureAwait(false);
        }

        public override Task<Component> CreateComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log) => ExecuteAsync(component, commandQueue, log, async (client, owner) =>
        {
            var project = await projectRepository
                .GetAsync(component.Organization, component.ProjectId)
                .ConfigureAwait(false);

            var repositoryName = $"{project.DisplayName} {component.DisplayName}";
            var repositoryDescription = $"Repository for TeamCloud project {project.DisplayName}";

            Repository repository = null;

            try
            {
                if (GitHubIdentifier.TryParse(component.ResourceId, out var resourceId))
                {
                    repository = await client.Repository
                        .Get(resourceId.Organization, resourceId.Repository)
                        .ConfigureAwait(false);
                }
                else
                {
                    repository = await client.Repository
                        .Get(owner.Login, repositoryName)
                        .ConfigureAwait(false);
                }
            }
            catch (ApiException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
            {
                repository = null;
            }

            string repositoryTemplateUrl = null;

            if (repository is null)
            {
                repositoryTemplateUrl ??= await GetTemplateRepositoryUrlAsync().ConfigureAwait(false);

                var repositoryTemplate = string.IsNullOrEmpty(repositoryTemplateUrl)
                    ? null // as there is no repository template url available we won't find a repo
                    : await GetTemplateRepositoryAsync(repositoryTemplateUrl).ConfigureAwait(false);

                if (repositoryTemplate is null)
                {
                    // the fact that there is no repository template just means that
                    // we can't use the repo templating factory provided by GitHub, but
                    // need to fall back to a classic repository import later on if a
                    // template repository git url is given.

                    var repoSettings = new NewRepository(repositoryName)
                    {
                        Description = repositoryDescription,
                        AutoInit = string.IsNullOrWhiteSpace(repositoryTemplateUrl)
                    };

                    repository = await client.Repository
                        .Create(owner.Login, repoSettings)
                        .ConfigureAwait(false);
                }
                else
                {
                    // use the GitHub template repository as a blueprint to create a new
                    // repository. as this API call isn't supported by the Octokit client
                    // library we need to fall back to a raw API call via Flurl.

                    var payload = new
                    {
                        owner = owner.Login,
                        name = repositoryName,
                        description = repositoryDescription
                    };

                    var response = await repositoryTemplate.Url.ToString()
                        .AppendPathSegment("generate")
                        .WithGitHubHeaders("baptiste-preview")
                        .WithGitHubCredentials(client.Credentials)
                        .PostJsonAsync(payload)
                        .ConfigureAwait(false);

                    var json = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);

                    repository = GitHubSerializer.Deserialize<Repository>(json);
                }
            }

            if (!GitHubIdentifier.FromUrl(repository.HtmlUrl).ToString().Equals(component.ResourceId, StringComparison.OrdinalIgnoreCase))
            {
                component.ResourceId = GitHubIdentifier.FromUrl(repository.HtmlUrl).ToString();
                component.ResourceUrl = repository.HtmlUrl;

                component = await componentRepository
                    .SetAsync(component)
                    .ConfigureAwait(false);
            }

            repositoryTemplateUrl ??= await GetTemplateRepositoryUrlAsync().ConfigureAwait(false);

            if (Uri.TryCreate(repositoryTemplateUrl, UriKind.Absolute, out var templateUrl)
                && await client.Repository.IsEmpty(repository.Id).ConfigureAwait(false))
            {
                var payload = new
                {
                    vcs = "git",
                    vcs_url = templateUrl.ToString()
                };

                try
                {
                    var response = await repository.Url
                        .AppendPathSegment("import")
                        .WithGitHubHeaders()
                        .WithGitHubCredentials(client.Connection.Credentials)
                        .PutJsonAsync(payload)
                        .ConfigureAwait(false);
                }
                catch (FlurlHttpException exc) when (exc.Call.HttpStatus == HttpStatusCode.Forbidden)
                {
                    // foo
                }
            }

            return await UpdateComponentAsync(component, commandUser, commandQueue, log).ConfigureAwait(false);

            async Task<string> GetTemplateRepositoryUrlAsync()
            {
                var componentTemplate = await componentTemplateRepository
                    .GetAsync(component.Organization, component.ProjectId, component.TemplateId)
                    .ConfigureAwait(false) as ComponentRepositoryTemplate;

                return componentTemplate?.Configuration?.TemplateRepository;
            }

            async Task<Repository> GetTemplateRepositoryAsync(string url = null)
            {
                url ??= await GetTemplateRepositoryUrlAsync().ConfigureAwait(false);

                if (Uri.TryCreate(url, UriKind.Absolute, out var repositoryUrl)
                    && client.BaseAddress.Host.EndsWith($".{repositoryUrl.Host}", StringComparison.OrdinalIgnoreCase))
                {
                    var identifier = GitHubIdentifier.FromUrl(url);

                    try
                    {
                        // we need to get a new client that explicitly fetches the reqository using
                        // the Baptiste preview version of the GitHub REST API as this is the only
                        // version delivering the IsTemplate information as of today!

                        var client = await CreateClientAsync(component, "baptiste-preview")
                            .ConfigureAwait(false);

                        var repository = await client.Repository
                            .Get(identifier.Organization, identifier.Repository)
                            .ConfigureAwait(false);

                        return (repository?.IsTemplate ?? false) ? repository : null;
                    }
                    catch
                    {
                        // swallow
                    }
                }

                return null;
            }
        });

        public override Task<Component> UpdateComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log) => ExecuteAsync(component, commandQueue, log, async (client, owner) =>
        {



            return component;

        });

        public override Task<Component> DeleteComponentAsync(Component component, User commandUser, IAsyncCollector<ICommand> commandQueue, ILogger log)
        {
            return Task.FromResult(component);
        }

        public override async Task<NetworkCredential> GetServiceCredentialAsync(Component component)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            var gitHubClient = await CreateClientAsync(component)
                .ConfigureAwait(false);

            return gitHubClient is null
                ? null
                : new NetworkCredential("bearer", gitHubClient.Credentials.Password, gitHubClient.BaseAddress.ToString());
        }
    }
}
