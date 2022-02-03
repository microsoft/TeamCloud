/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Octokit;
using Octokit.Internal;
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
using HttpStatusCode = System.Net.HttpStatusCode;
using IHttpClientFactory = Flurl.Http.Configuration.IHttpClientFactory;
using Organization = TeamCloud.Model.Data.Organization;
using Project = TeamCloud.Model.Data.Project;
using User = TeamCloud.Model.Data.User;

namespace TeamCloud.Adapters.GitHub;

public sealed partial class GitHubAdapter : AdapterWithIdentity, IAdapterAuthorize
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

#pragma warning disable CS0618 // Type or member is obsolete

    // IDistributedLockManager is marked as obsolete, because it's not ready for "prime time"
    // however; it is used to managed singleton function execution within the functions fx !!!

    public GitHubAdapter(
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
        IFunctionsHost functionsHost = null)
        : base(sessionClient, tokenClient, distributedLockManager, secretsStoreProvider, azureSessionService, azureDirectoryService, organizationRepository, deploymentScopeRepository, projectRepository, userRepository)
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

#pragma warning restore CS0618 // Type or member is obsolete

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

    async Task<AzureServicePrincipal> IAdapterAuthorize.ResolvePrincipalAsync(DeploymentScope deploymentScope, HttpRequest request)
    {
        const string SignatureHeader = "X-Hub-Signature-256";

        if (deploymentScope is null)
            throw new ArgumentNullException(nameof(deploymentScope));

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.Headers.TryGetValue(SignatureHeader, out var signatureValue) && !string.IsNullOrEmpty(signatureValue))
        {
            var token = await TokenClient
                .GetAsync<GitHubToken>(deploymentScope)
                .ConfigureAwait(false);

            if (!string.IsNullOrEmpty(token?.WebhookSecret))
            {
                var signature = signatureValue
                    .ToString()
                    .Substring(signatureValue.ToString().IndexOf('=') + 1);

                // CAUTION - we need to read the body but leave
                // the request's body stream open so it stays
                // available for subsequent requests

                var body = await request
                    .ReadStringAsync(leaveOpen: true)
                    .ConfigureAwait(false);

                var secret = Encoding.ASCII
                    .GetBytes(token.WebhookSecret);

                using var hmac = new HMACSHA256(secret);

                var hash = hmac
                    .ComputeHash(Encoding.ASCII.GetBytes(body))
                    .ToHexString();

                if (hash.Equals(signature))
                {
                    // signature successfully validated - lets use the adapters identity to proceed
                    return await GetServiceIdentityAsync(deploymentScope).ConfigureAwait(false);
                }
            }
        }

        return null;
    }

    async Task<IActionResult> IAdapterAuthorize.HandleAuthorizeAsync(DeploymentScope deploymentScope, HttpRequest request, IAuthorizationEndpoints authorizationEndpoints)
    {
        if (deploymentScope is null)
            throw new ArgumentNullException(nameof(deploymentScope));

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (authorizationEndpoints is null)
            throw new ArgumentNullException(nameof(authorizationEndpoints));

        var queryParams = Url.ParseQueryParams(request.QueryString.ToString());
        var queryState = queryParams.GetValueOrDefault("state");
        var queryCode = queryParams.GetValueOrDefault("code");
        var queryError = queryParams.GetValueOrDefault("error");

        var session = await SessionClient
            .GetAsync<GitHubSession>(deploymentScope)
            .ConfigureAwait(false);

        session ??= await SessionClient
            .SetAsync(new GitHubSession(deploymentScope))
            .ConfigureAwait(false);

        var data = string.IsNullOrWhiteSpace(deploymentScope.InputData)
            ? default
            : TeamCloudSerialize.DeserializeObject<GitHubData>(deploymentScope.InputData);

        var token = await TokenClient
            .GetAsync<GitHubToken>(deploymentScope)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(queryError) && Guid.TryParse(queryState, out var stateId))
        {
            if (!stateId.ToString().Equals(session.SessionId, StringComparison.OrdinalIgnoreCase))
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

            if (!response.IsSuccessStatusCode())
            {
                return new RedirectResult(authorizationEndpoints.AuthorizationUrl.SetQueryParam("error", $"Failed to get application token ({response.StatusCode} - {response.ResponseMessage.ReasonPhrase}).").ToString());
            }

            var json = await response
                .GetStringAsync()
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
            session = session,
            error = queryError ?? string.Empty,
            succeeded = queryParams.Contains("succeeded")
        };
    }

    async Task<IActionResult> IAdapterAuthorize.HandleCallbackAsync(DeploymentScope deploymentScope, HttpRequest request, IAuthorizationEndpoints authorizationEndpoints)
    {
        if (deploymentScope is null)
            throw new ArgumentNullException(nameof(deploymentScope));

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (authorizationEndpoints is null)
            throw new ArgumentNullException(nameof(authorizationEndpoints));

        var json = await request
            .ReadJsonAsync()
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

            if (token is not null)
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
                    Credentials = new Credentials(token.GetPemToken(), Octokit.AuthenticationType.Bearer)
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

    private static string GetRepositoryName(Project project, Component component, int count = 0)
        => $"{project.Slug}-{component.Slug}{(count <= 0 ? string.Empty : $"-{count:D5}")}";

    private Task<Component> ExecuteAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue, Func<GitHubClient, Octokit.User, Octokit.Project, Octokit.Team, Octokit.Team, Task<Component>> callback)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        if (contextUser is null)
            throw new ArgumentNullException(nameof(contextUser));

        if (commandQueue is null)
            throw new ArgumentNullException(nameof(commandQueue));

        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        return base.WithContextAsync(component, async (componentOrganization, componentDeploymentScope, componentProject) =>
        {
            var token = await TokenClient
                .GetAsync<GitHubToken>(componentDeploymentScope)
                .ConfigureAwait(false);

            var client = await CreateClientAsync(token)
                .ConfigureAwait(false);

            var teams = await client.Organization.Team
                .GetAll(token.Owner.Login)
                .ConfigureAwait(false);

            var tasks = new List<Task>();

            var teamsByName = teams
                .ToDictionary(k => k.Name, v => v);

            var organizationTeamName = $"TeamCloud-{componentOrganization.Slug}";
            var organizationTeam = await EnsureTeamAsync(organizationTeamName, null, new NewTeam(organizationTeamName)
            {
                Description = $"TeamCloud {componentOrganization.DisplayName} organization.",
                Privacy = TeamPrivacy.Closed // Parent and nested child teams must use Closed

            }).ConfigureAwait(false);

            var organizationAdminTeamName = $"{organizationTeamName}-Admins";
            var organizationAdminTeam = await EnsureTeamAsync(organizationAdminTeamName, organizationTeam, new NewTeam(organizationAdminTeamName)
            {
                Description = $"TeamCloud {componentOrganization.DisplayName} organization admins.",
                Privacy = TeamPrivacy.Closed, // Parent and nested child teams must use Closed
                Permission = Permission.Admin

            }).ConfigureAwait(false);

            var projectTeamName = $"{organizationTeamName}-{componentProject.Slug}";
            var projectTeam = await EnsureTeamAsync(projectTeamName, organizationTeam, new NewTeam(projectTeamName)
            {
                Description = $"TeamCloud project {componentProject.DisplayName} in the {componentOrganization.DisplayName} organization.",
                Privacy = TeamPrivacy.Closed // Parent and nested child teams must use Closed

            }).ConfigureAwait(false);

            var projectAdminsTeamName = $"{projectTeamName}-Admins";
            var projectAdminsTeam = await EnsureTeamAsync(projectAdminsTeamName, projectTeam, new NewTeam(projectAdminsTeamName)
            {
                Description = $"TeamCloud project {componentProject.DisplayName} admins in the {componentOrganization.DisplayName} organization.",
                Privacy = TeamPrivacy.Closed, // Parent and nested child teams must use Closed
                Permission = Permission.Admin

            }).ConfigureAwait(false);

            var projectName = $"TeamCloud-{componentOrganization.Slug}-{componentProject.Slug}";
            var project = await EnsureProjectAsync(projectName, new NewProject(projectName)
            {
                Body = $"Project for TeamCloud project {componentProject.DisplayName} in organization {componentOrganization.DisplayName}"

            }).ConfigureAwait(false);

            var organizationOwners = await userRepository
                .ListOwnersAsync(component.Organization)
                .ToArrayAsync()
                .ConfigureAwait(false);

            var organizationAdmins = await userRepository
                .ListAdminsAsync(component.Organization)
                .ToArrayAsync()
                .ConfigureAwait(false);

            await Task.WhenAll(

                EnsurePermissionAsync(projectTeam, project, "write"),
                EnsurePermissionAsync(projectAdminsTeam, project, "admin"),

                SynchronizeTeamMembersAsync(client, organizationAdminTeam, Enumerable.Concat(organizationOwners, organizationAdmins).Distinct(), component, contextUser, commandQueue)

                ).ConfigureAwait(false);

            return await callback(client, token.Owner, project, projectTeam, projectAdminsTeam).ConfigureAwait(false);

            async Task<Team> EnsureTeamAsync(string teamName, Team parentTeam, NewTeam teamDefinition)
            {
                if (!teamsByName.TryGetValue(teamName, out var team))
                {
                    await using var adapterLock = await AcquireLockAsync(nameof(GitHubAdapter), component.DeploymentScopeId).ConfigureAwait(false);

                    teamDefinition.ParentTeamId = parentTeam?.Id;

                    try
                    {
                        team = await client.Organization.Team
                            .Create(token.Owner.Login, teamDefinition)
                            .ConfigureAwait(false);
                    }
                    catch (ApiException exc) when (exc.StatusCode == HttpStatusCode.UnprocessableEntity) // yes, thats the status code if the team already exists
                    {
                        var teams = await client.Organization.Team
                            .GetAll(token.Owner.Login)
                            .ConfigureAwait(false);

                        team = teams
                            .FirstOrDefault(t => t.Name.Equals(teamName, StringComparison.Ordinal));

                        if (team is null)
                            throw;
                    }
                }

                if (team.Parent?.Id != parentTeam?.Id)
                {
                    await using var adapterLock = await AcquireLockAsync(nameof(GitHubAdapter), component.DeploymentScopeId).ConfigureAwait(false);

                    team = await client.Organization.Team.Update(team.Id, new UpdateTeam(team.Name)
                    {
                        ParentTeamId = parentTeam?.Id ?? 0

                    }).ConfigureAwait(false);
                }


                return team;
            }

            async Task<Octokit.Project> EnsureProjectAsync(string projectName, NewProject projectDefinition)
            {
                var projects = await client.Repository.Project
                    .GetAllForOrganization(token.Owner.Login)
                    .ConfigureAwait(false);

                var project = projects
                    .FirstOrDefault(t => t.Name.Equals(projectName, StringComparison.Ordinal));

                try
                {
                    project ??= await client.Repository.Project
                        .CreateForOrganization(token.Owner.Login, projectDefinition)
                        .ConfigureAwait(false);
                }
                catch (ApiException exc) when (exc.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    // the project already exists - try to re-fetch project information

                    projects = await client.Repository.Project
                    .GetAllForOrganization(token.Owner.Login)
                    .ConfigureAwait(false);

                    project = projects
                        .FirstOrDefault(t => t.Name.Equals(projectName, StringComparison.Ordinal));

                    if (project is null)
                        throw new ApplicationException($"Duplicate project ({projectName}) under unknown ownership detected", exc);
                }
                catch (ApiException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ApplicationException($"Organization level projects disabled in {client.Connection.BaseAddress}");
                }

                return project;
            }

            async Task EnsurePermissionAsync(Octokit.Team team, Octokit.Project project, string permission)
            {
                var inertiaClient = await CreateClientAsync(component, "inertia-preview").ConfigureAwait(false);

                var url = new Uri($"/orgs/{token.Owner.Login}/teams/{team.Slug}/projects/{project.Id}", UriKind.Relative);

                _ = await inertiaClient.Connection
                    .Put<string>(url, new { Permission = permission })
                    .ConfigureAwait(false);
            }
        });
    }

    private async Task SynchronizeTeamMembersAsync(GitHubClient client, Team team, IEnumerable<User> users, Component component, User contextUser, IAsyncCollector<ICommand> commandQueue)
    {
        var userIds = new HashSet<Guid>(await users
            .ToAsyncEnumerable()
            .SelectMany(user => ResolveUserIdsAsync(user))
            .ToArrayAsync()
            .ConfigureAwait(false));

        var teamUsers = await userIds
            .Select(userId =>
            {
                var user = users.SingleOrDefault(u => userId.Equals(Guid.Parse(u.Id)));
                return user is null ? EnsureUserAsync(userId) : InitializeUserAsync(user);
            })
            .WhenAll()
            .ConfigureAwait(false);

        var logins = teamUsers
            .Select(user => user.AlternateIdentities.TryGetValue(this.Type, out var alternateIdentity) ? alternateIdentity.Login : null)
            .Where(login => !string.IsNullOrWhiteSpace(login))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var members = await client.Organization.Team
            .GetAllMembers(team.Id)
            .ConfigureAwait(false);

        var membershipTasks = new List<Task>();

        membershipTasks.AddRange(logins
            .Except(members.Select(m => m.Login), StringComparer.OrdinalIgnoreCase)
            .Select(login => client.Organization.Team.AddOrEditMembership(team.Id, login, new UpdateTeamMembership(TeamRole.Member))));

        membershipTasks.AddRange(members
            .Select(m => m.Login)
            .Except(logins, StringComparer.OrdinalIgnoreCase)
            .Select(login => client.Organization.Team.RemoveMembership(team.Id, login)));

        await membershipTasks
            .WhenAll()
            .ConfigureAwait(false);

        async Task<User> EnsureUserAsync(Guid userId)
        {
            var user = await userRepository
                .GetAsync(component.Organization, userId.ToString(), true)
                .ConfigureAwait(false);

            if (user is null)
            {
                user = new User()
                {
                    Id = userId.ToString(),
                    Role = OrganizationUserRole.None
                };

                user.AlternateIdentities[this.Type] = new AlternateIdentity();

                await commandQueue
                    .AddAsync(new OrganizationUserCreateCommand(contextUser, user))
                    .ConfigureAwait(false);
            }
            else
            {
                user = await InitializeUserAsync(user)
                    .ConfigureAwait(false);
            }

            return user;
        }

        async Task<User> InitializeUserAsync(User user)
        {
            if (user?.AlternateIdentities.TryAdd(Type, new AlternateIdentity()) ?? false)
            {
                await commandQueue
                    .AddAsync(new OrganizationUserUpdateCommand(contextUser, user))
                    .ConfigureAwait(false);
            }

            return user;
        }

        IAsyncEnumerable<Guid> ResolveUserIdsAsync(User user) => user.UserType switch
        {
            // return the given user id as async enumeration
            UserType.User => AsyncEnumerable.Repeat(new Guid(user.Id), 1),

            // return user ids based on the given user that represents a group
            UserType.Group => azureDirectoryService.GetGroupMembersAsync(user.Id, true),

            // not supported user type
            _ => AsyncEnumerable.Empty<Guid>()
        };
    }

    protected override Task<Component> CreateComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue)
        => ExecuteAsync(component, contextUser, commandQueue, async (client, owner, project, memberTeam, adminTeam) =>
    {
        Repository repository = null;

        var repositoryName = GetRepositoryName(componentProject, component);
        var repositoryDescription = $"Repository for TeamCloud project {componentProject.DisplayName}";
        var repositoryCount = 0;
        var repositoryTemplateUrl = default(string);

        while (repository is null)
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

                var data = string.IsNullOrWhiteSpace(componentDeploymentScope.InputData)
                ? default
                : TeamCloudSerialize.DeserializeObject<GitHubData>(componentDeploymentScope.InputData);

                var repoSettings = new NewRepository(repositoryName)
                {
                    Description = repositoryDescription,
                    AutoInit = string.IsNullOrWhiteSpace(repositoryTemplateUrl),
                    Private = !(data?.PublicRepository ?? false)
                };

                try
                {
                    repository = await client.Repository
                        .Create(owner.Login, repoSettings)
                        .ConfigureAwait(false);
                }
                catch (RepositoryExistsException)
                {
                    repositoryName = GetRepositoryName(componentProject, component, ++repositoryCount);
                }
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
                    .AllowHttpStatus(HttpStatusCode.UnprocessableEntity)
                    .PostJsonAsync(payload)
                    .ConfigureAwait(false);

                if (response.StatusCode == StatusCodes.Status422UnprocessableEntity)
                {
                    // the generate from template repo operation returns this
                    // status code if the target repo name already exists.
                    // lets make the repo name unique and try again.

                    repositoryName = GetRepositoryName(componentProject, component, ++repositoryCount);
                }
                else
                {
                    var json = await response
                        .GetStringAsync()
                        .ConfigureAwait(false);

                    repository = GitHubSerializer.Deserialize<Repository>(json);
                }
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
            catch (FlurlHttpException exc) when (exc.StatusCode == StatusCodes.Status403Forbidden)
            {
                // swallow and resume
            }
        }

        return await UpdateComponentAsync(component, contextUser, commandQueue).ConfigureAwait(false);

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

    protected override Task<Component> UpdateComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue)
        => ExecuteAsync(component, contextUser, commandQueue, async (client, owner, project, memberTeam, adminTeam) =>
    {
        var users = await userRepository
            .ListAsync(component.Organization, component.ProjectId)
            .ToListAsync()
            .ConfigureAwait(false);

        await Task.WhenAll(

            SyncTeamCloudIdentityAsync(),

            SynchronizeTeamMembersAsync(client, memberTeam, users.Where(user => user.IsMember()), component, contextUser, commandQueue),
            SynchronizeTeamMembersAsync(client, adminTeam, users.Where(user => user.IsAdmin()), component, contextUser, commandQueue)

            ).ConfigureAwait(false);

        return component;

        async Task SyncTeamCloudIdentityAsync()
        {
            if (AzureResourceIdentifier.TryParse(componentProject.ResourceId, out var projectResourceId) && GitHubIdentifier.TryParse(component.ResourceId, out var componentResourceId))
            {
                var servicePrincipal = await base.GetServiceIdentityAsync((Component)component, (bool)true)
                    .ConfigureAwait(false);

                var repository = await client.Repository
                    .Get(componentResourceId.Organization, componentResourceId.Repository)
                    .ConfigureAwait(false);

                if (repository is not null)
                {
                    var servicePrincipalJson = GitHubExtensions.ToJson(servicePrincipal, (Guid)projectResourceId.SubscriptionId);

                    var keyJson = await repository.Url
                        .AppendPathSegment("actions/secrets/public-key")
                        .WithGitHubHeaders()
                        .WithGitHubCredentials(client.Connection.Credentials)
                        .GetJObjectAsync()
                        .ConfigureAwait(false);

                    var key = Convert.FromBase64String(keyJson.SelectToken("key")?.ToString() ?? string.Empty);

                    var secretBuffer = Encoding.UTF8.GetBytes(servicePrincipalJson);
                    var encryptBuffer = Sodium.SealedPublicKeyBox.Create(secretBuffer, key);

                    var payload = new
                    {
                        owner = repository.Owner.Name,
                        repo = repository.Name,
                        secret_name = "TEAMCLOUD_CREDENTIAL",
                        encrypted_value = Convert.ToBase64String(encryptBuffer),
                        key_id = keyJson.SelectToken("key_id")?.ToString()
                    };

                    await repository.Url
                        .AppendPathSegment($"actions/secrets/{payload.secret_name}")
                        .WithGitHubHeaders()
                        .WithGitHubCredentials(client.Connection.Credentials)
                        .PutJsonAsync(payload)
                        .ConfigureAwait(false);
                }
            }
        }
    });

    protected override Task<Component> DeleteComponentAsync(Component component, Organization componentOrganization, DeploymentScope componentDeploymentScope, Project componentProject, User contextUser, IAsyncCollector<ICommand> commandQueue)
        => ExecuteAsync(component, contextUser, commandQueue, async (client, owner, project, memberTeam, adminTeam) =>
    {
        if (GitHubIdentifier.TryParse(component.ResourceId, out var gitHubIdentifier))
        {
            var exists = await client.Repository
                .Exists(gitHubIdentifier.Organization, gitHubIdentifier.Repository)
                .ConfigureAwait(false);

            if (exists)
            {
                await client.Repository
                    .Delete(gitHubIdentifier.Organization, gitHubIdentifier.Repository)
                    .ConfigureAwait(false);
            }

            // remove resource related informations

            component.ResourceId = null;
            component.ResourceUrl = null;

            // ensure resource state is deleted

            component.ResourceState = Model.Common.ResourceState.Deprovisioned;

            // update entity to ensure we have it's state updated in case the delete fails

            component = await componentRepository
            .SetAsync(component)
            .ConfigureAwait(false);
        }

        return component;
    });

    public override async Task<NetworkCredential> GetServiceCredentialAsync(Component component)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        var gitHubClient = await CreateClientAsync(component)
            .ConfigureAwait(false);

        if (gitHubClient is null)
            return default;

        return new NetworkCredential("bearer", gitHubClient.Credentials.Password, gitHubClient.BaseAddress.ToString());
    }
}
