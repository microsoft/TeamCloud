/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure;
using TeamCloud.Data;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters;

public abstract class Adapter : IAdapter
{
    private static readonly JSchema dataSchemaEmpty = new() { Type = JSchemaType.Object };
    private static readonly JObject formSchemaEmpty = new();

#pragma warning disable CS0618 // Type or member is obsolete

    // IDistributedLockManager is marked as obsolete, because it's not ready for "prime time"
    // however; it is used to managed singleton function execution within the functions fx !!!

    private readonly IAuthorizationSessionClient sessionClient;
    private readonly IAuthorizationTokenClient tokenClient;
    private readonly IDistributedLockManager distributedLockManager;
    private readonly IAzureService azure;
    private readonly IGraphService graphService;
    private readonly IOrganizationRepository organizationRepository;
    private readonly IDeploymentScopeRepository deploymentScopeRepository;
    private readonly IProjectRepository projectRepository;
    private readonly IUserRepository userRepository;

    protected Adapter(IAuthorizationSessionClient sessionClient,
                      IAuthorizationTokenClient tokenClient,
                      IDistributedLockManager distributedLockManager,
                      IAzureService azure,
                      IGraphService graphService,
                      IOrganizationRepository organizationRepository,
                      IDeploymentScopeRepository deploymentScopeRepository,
                      IProjectRepository projectRepository,
                      IUserRepository userRepository)
    {
        this.sessionClient = sessionClient ?? throw new ArgumentNullException(nameof(sessionClient));
        this.tokenClient = tokenClient ?? throw new ArgumentNullException(nameof(tokenClient));
        this.distributedLockManager = distributedLockManager ?? throw new ArgumentNullException(nameof(distributedLockManager));
        this.azure = azure ?? throw new ArgumentNullException(nameof(azure));
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
        this.organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        this.deploymentScopeRepository = deploymentScopeRepository ?? throw new ArgumentNullException(nameof(deploymentScopeRepository));
        this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

#pragma warning restore CS0618 // Type or member is obsolete

    public abstract DeploymentScopeType Type { get; }

    public abstract IEnumerable<ComponentType> ComponentTypes { get; }

    public virtual string DisplayName
        => Type.ToString(prettyPrint: true);

    protected IAuthorizationSessionClient SessionClient
        => sessionClient;

    protected IAuthorizationTokenClient TokenClient
        => tokenClient;

    protected Task<AdapterLock> AcquireLockAsync(string lockId, params string[] lockIdQualifiers)
    {
        if (string.IsNullOrWhiteSpace(lockId))
            throw new ArgumentException($"'{nameof(lockId)}' cannot be null or whitespace.", nameof(lockId));

        return AcquireLockAsync(string.Join('/', lockIdQualifiers.Prepend(lockId).Where(item => !string.IsNullOrWhiteSpace(item))));
    }

    protected async Task<AdapterLock> AcquireLockAsync(string lockId, int leaseTimeoutInSeconds = 60, int acquisitionTimeoutInSeconds = 60)
    {
        if (string.IsNullOrWhiteSpace(lockId))
            throw new ArgumentException($"'{nameof(lockId)}' cannot be null or whitespace.", nameof(lockId));

        leaseTimeoutInSeconds = Math.Max(leaseTimeoutInSeconds, 0);
        acquisitionTimeoutInSeconds = Math.Max(acquisitionTimeoutInSeconds, 0);

        var distributedLock = await distributedLockManager
            .TryLockAsync(null, lockId, null, null, TimeSpan.FromSeconds(leaseTimeoutInSeconds), CancellationToken.None)
            .ConfigureAwait(false);

        var acquisitionTimeoutElapsed = 0;

        while (distributedLock is null && acquisitionTimeoutElapsed++ < acquisitionTimeoutInSeconds)
        {
            await Task
                .Delay(1000)
                .ConfigureAwait(false);

            distributedLock = await distributedLockManager
                .TryLockAsync(null, lockId, null, null, TimeSpan.FromSeconds(leaseTimeoutInSeconds), CancellationToken.None)
                .ConfigureAwait(false);
        }

        if (distributedLock is null)
            throw new TimeoutException($"Failed to acquire lock for id '{lockId}' within {acquisitionTimeoutInSeconds} sec.");

        return new AdapterLock(distributedLockManager, distributedLock);
    }

    public virtual Task<string> GetInputDataSchemaAsync()
        => Task.FromResult(dataSchemaEmpty.ToString(Formatting.None));

    public virtual Task<string> GetInputFormSchemaAsync()
        => Task.FromResult(formSchemaEmpty.ToString(Formatting.None));

    public virtual Task<NetworkCredential> GetServiceCredentialAsync(Component component)
        => WithContextAsync(component, (componentOrganization, componentDeploymentScope, componentProject)
            => GetServiceCredentialAsync(component, componentOrganization, componentDeploymentScope, componentProject));

    protected virtual Task<NetworkCredential> GetServiceCredentialAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project)
        => Task.FromResult(default(NetworkCredential));

    public virtual Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope)
        => this is IAdapterAuthorize ? throw new NotImplementedException() : Task.FromResult(true);

    public Task<Component> CreateComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue)
        => WithContextAsync(component, (componentOrganization, componentDeploymentScope, componentProject)
            => CreateComponentAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue));

    protected abstract Task<Component> CreateComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue);

    public Task<Component> UpdateComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue)
        => WithContextAsync(component, (componentOrganization, componentDeploymentScope, componentProject)
            => UpdateComponentAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue));

    protected abstract Task<Component> UpdateComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue);

    public Task<Component> DeleteComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue)
        => WithContextAsync(component, (componentOrganization, componentDeploymentScope, componentProject)
            => DeleteComponentAsync(component, componentOrganization, componentDeploymentScope, componentProject, contextUser, commandQueue));

    protected abstract Task<Component> DeleteComponentAsync(Component component, Organization organization, DeploymentScope deploymentScope, Project project, User contextUser, IAsyncCollector<ICommand> commandQueue);

    protected async Task WithContextAsync(Component component, Func<Organization, DeploymentScope, Project, Task> callback)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var tenantId = await azure
            .GetTenantIdAsync()
            .ConfigureAwait(false);

        var organization = await organizationRepository
            .GetAsync(tenantId, component.Organization, true)
            .ConfigureAwait(false);

        var deploymentScope = await deploymentScopeRepository
            .GetAsync(component.Organization, component.DeploymentScopeId)
            .ConfigureAwait(false);

        var project = await projectRepository
            .GetAsync(component.Organization, component.ProjectId)
            .ConfigureAwait(false);

        await callback(organization, deploymentScope, project).ConfigureAwait(false);
    }

    protected async Task<T> WithContextAsync<T>(Component component, Func<Organization, DeploymentScope, Project, Task<T>> callback)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        var tenantId = await azure
            .GetTenantIdAsync()
            .ConfigureAwait(false);

        var organization = await organizationRepository
            .GetAsync(tenantId, component.Organization, true)
            .ConfigureAwait(false);

        var deploymentScope = await deploymentScopeRepository
            .GetAsync(component.Organization, component.DeploymentScopeId)
            .ConfigureAwait(false);

        var project = await projectRepository
            .GetAsync(component.Organization, component.ProjectId)
            .ConfigureAwait(false);

        return await callback(organization, deploymentScope, project).ConfigureAwait(false);
    }
}
