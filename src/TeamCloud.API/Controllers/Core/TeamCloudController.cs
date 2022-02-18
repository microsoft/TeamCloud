/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Controllers.Core;

public abstract class TeamCloudController : ControllerBase
{
    protected TeamCloudController(IValidatorProvider validatorProvider = null, ILogger log = null)
    {
        ValidatorProvider = validatorProvider ?? NullValidatorProvider.Instance;
        Log = log ?? NullLogger.Instance;
    }

    protected string OrganizationId => RouteData.ValueOrDefault(nameof(OrganizationId));

    protected string ProjectTemplateId => RouteData.ValueOrDefault(nameof(ProjectTemplateId));

    protected string ProjectIdentityId => RouteData.ValueOrDefault(nameof(ProjectIdentityId));

    protected string DeploymentScopeId => RouteData.ValueOrDefault(nameof(DeploymentScopeId));

    protected string UserId => RouteData.ValueOrDefault(nameof(UserId));

    protected string ProjectId => RouteData.ValueOrDefault(nameof(ProjectId));

    protected string TaskId => RouteData.ValueOrDefault(nameof(TaskId));

    protected string ComponentId => RouteData.ValueOrDefault(nameof(ComponentId));

    protected string ScheduleId => RouteData.ValueOrDefault(nameof(ScheduleId));

    protected T GetService<T>()
        => (T)HttpContext.RequestServices.GetService(typeof(T));

    public UserService UserService
        => GetService<UserService>();

    public OrchestratorService Orchestrator
        => GetService<OrchestratorService>();

    protected ILogger Log { get; }

    protected IValidatorProvider ValidatorProvider { get; }

    protected async Task<IActionResult> WithContextAsync(Func<User, Task<IActionResult>> callback)
    {
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        try
        {
            var contextUser = await UserService
                .CurrentUserAsync(OrganizationId, null)
                .ConfigureAwait(false);

            return await callback(contextUser)
                .ConfigureAwait(false);
        }
        catch (ErrorResultException exc)
        {
            return exc.ToActionResult();
        }
    }

    protected async Task<IActionResult> WithContextAsync<T1>(Func<User, T1, Task<IActionResult>> callback)
        where T1 : class, IContainerDocument
    {
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        try
        {
            var tasks = new List<Task>()
                {
                    UserService.CurrentUserAsync(OrganizationId, null),
                    GetContextDocumentAsync<T1>()
                };

            await tasks.WhenAll().ConfigureAwait(false);

            var arguments = tasks
                .Select(task => (object)((dynamic)task).Result)
                .ToArray();

            return await ((Task<IActionResult>)callback.GetType().GetMethod(nameof(callback.Invoke)).Invoke(callback, arguments)).ConfigureAwait(false);
        }
        catch (ErrorResultException exc)
        {
            return exc.ToActionResult();
        }
    }

    protected async Task<IActionResult> WithContextAsync<T1, T2>(Func<User, T1, T2, Task<IActionResult>> callback)
        where T1 : class, IContainerDocument
        where T2 : class, IContainerDocument
    {
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        try
        {
            var tasks = new List<Task>()
                {
                    UserService.CurrentUserAsync(OrganizationId, null),
                    GetContextDocumentAsync<T1>(),
                    GetContextDocumentAsync<T2>()
                };

            await tasks.WhenAll().ConfigureAwait(false);

            var arguments = tasks
                .Select(task => (object)((dynamic)task).Result)
                .ToArray();

            return await ((Task<IActionResult>)callback.GetType().GetMethod(nameof(callback.Invoke)).Invoke(callback, arguments)).ConfigureAwait(false);
        }
        catch (ErrorResultException exc)
        {
            return exc.ToActionResult();
        }
    }

    protected async Task<IActionResult> WithContextAsync<T1, T2, T3>(Func<User, T1, T2, T3, Task<IActionResult>> callback)
        where T1 : class, IContainerDocument
        where T2 : class, IContainerDocument
        where T3 : class, IContainerDocument
    {
        if (callback is null)
            throw new ArgumentNullException(nameof(callback));

        try
        {
            var tasks = new List<Task>()
                {
                    UserService.CurrentUserAsync(OrganizationId, null),
                    GetContextDocumentAsync<T1>(),
                    GetContextDocumentAsync<T2>(),
                    GetContextDocumentAsync<T3>()
                };

            await tasks.WhenAll().ConfigureAwait(false);

            var arguments = tasks
                .Select(task => (object)((dynamic)task).Result)
                .ToArray();

            return await ((Task<IActionResult>)callback.GetType().GetMethod(nameof(callback.Invoke)).Invoke(callback, arguments)).ConfigureAwait(false);
        }
        catch (ErrorResultException exc)
        {
            return exc.ToActionResult();
        }

    }

    private async Task<T> GetContextDocumentAsync<T>()
                where T : class, IContainerDocument
    {
        var task = typeof(T) switch
        {
            _ when typeof(T) == typeof(Organization) => GetService<IOrganizationRepository>()
                .GetAsync(UserService.CurrentUserTenant, OrganizationId)
                .ContinueWith(task => OnNull(task.Result as T, $"A Organization with the slug or id '{OrganizationId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ when typeof(T) == typeof(Project) => GetService<IProjectRepository>()
                .GetAsync(OrganizationId, ProjectId)
                .ContinueWith(task => OnNull(task.Result as T, $"A Project with the name or id '{ProjectId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ when typeof(T) == typeof(ProjectTemplate) => GetService<IProjectTemplateRepository>()
                .GetAsync(OrganizationId, ProjectTemplateId)
                .ContinueWith(task => OnNull(task.Result as T, $"A Project Template with the name or id '{ProjectTemplateId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ when typeof(T) == typeof(ProjectIdentity) => GetService<IProjectIdentityRepository>()
                .GetAsync(ProjectId, ProjectIdentityId)
                .ContinueWith(task => OnNull(task.Result as T, $"A Project Identity with the Id '{ProjectIdentityId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ when typeof(T) == typeof(Component) => GetService<IComponentRepository>()
                .GetAsync(ProjectId, ComponentId)
                .ContinueWith(task => OnNull(task.Result as T, $"A Component with id '{ComponentId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ when typeof(T) == typeof(ComponentTask) => GetService<IComponentTaskRepository>()
                .GetAsync(ComponentId, TaskId)
                .ContinueWith(task => OnNull(task.Result as T, $"A Component task with id '{TaskId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ when typeof(T) == typeof(User) => UserService
                .GetUserIdAsync(UserId)
                .ContinueWith(task => OnNull(task.Result, $"A User with the name or id '{UserId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(task => GetService<IUserRepository>().GetAsync(OrganizationId, task.Result), TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap()
                .ContinueWith(task => OnNull(task.Result as T, $"A User with the Id '{UserId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ when typeof(T) == typeof(DeploymentScope) => GetService<IDeploymentScopeRepository>()
                .GetAsync(OrganizationId, DeploymentScopeId)
                .ContinueWith(task => OnNull(task.Result as T, $"A Deployment Scope with the name or id '{DeploymentScopeId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ when typeof(T) == typeof(Schedule) => GetService<IScheduleRepository>()
                .GetAsync(ProjectId, ScheduleId)
                .ContinueWith(task => OnNull(task.Result as T, $"A Schedule with id '{ScheduleId}' was not found."), TaskContinuationOptions.OnlyOnRanToCompletion),

            _ => throw new NotSupportedException($"Context document of type {typeof(T)} is not supported")
        };

        return await task.ConfigureAwait(false);

        static TValue OnNull<TValue>(TValue value, string errorMessage)
            => value is null ? throw ErrorResult.NotFound(errorMessage).ToException() : value;
    }
}
