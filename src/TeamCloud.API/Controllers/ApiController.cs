/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    public abstract class ApiController : ControllerBase
    {
        protected ApiController(UserService userService, Orchestrator orchestrator)
        {
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            Orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository)
            : this(userService, orchestrator)
        {
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IProviderRepository providerRepository)
            : this(userService, orchestrator)
        {
            ProviderRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IUserRepository userRepository)
            : this(userService, orchestrator)
        {
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IProviderRepository providerRepository)
            : this(userService, orchestrator)
        {
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            ProviderRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IUserRepository userRepository)
            : this(userService, orchestrator)
        {
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        private string ProjectId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

        private string ProjectNameOrId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectNameOrId), StringComparison.OrdinalIgnoreCase)?.ToString();

        public string ProviderId
            => RouteData.Values.GetValueOrDefault(nameof(ProviderId), StringComparison.OrdinalIgnoreCase)?.ToString();

        private string UserId
            => RouteData.Values.GetValueOrDefault(nameof(UserId), StringComparison.OrdinalIgnoreCase)?.ToString();

        private string UserNameOrId
            => RouteData.Values.GetValueOrDefault(nameof(UserNameOrId), StringComparison.OrdinalIgnoreCase)?.ToString();

        public string ProjectIdentifier => ProjectId ?? ProjectNameOrId;

        public string UserIdentifier => UserId ?? UserNameOrId;

        public UserService UserService { get; }

        public Orchestrator Orchestrator { get; }

        public IUserRepository UserRepository { get; }

        public IProjectRepository ProjectRepository { get; }

        public IProviderRepository ProviderRepository { get; }

        [NonAction]
        public Task<IActionResult> EnsureProjectAsync(Func<ProjectDocument, Task<IActionResult>> callback)
            => EnsureProjectInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAsync(Func<ProjectDocument, IActionResult> callback)
            => EnsureProjectInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectInternalAsync(Func<ProjectDocument, Task<IActionResult>> asyncCallback = null, Func<ProjectDocument, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(ProjectIdentifier))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(ProjectIdentifier)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectIdentifier}' could not be found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(project);

                if (!(asyncCallback is null))
                    return await asyncCallback(project)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");

            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public Task<IActionResult> EnsureProviderAsync(Func<ProviderDocument, Task<IActionResult>> callback)
            => EnsureProviderInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProviderAsync(Func<ProviderDocument, IActionResult> callback)
            => EnsureProviderInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProviderInternalAsync(Func<ProviderDocument, Task<IActionResult>> asyncCallback = null, Func<ProviderDocument, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(ProviderId))
                    return ErrorResult
                        .BadRequest($"Provider id provided in the url path is invalid.  Must be a valid non-empty string.", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var provider = await ProviderRepository
                    .GetAsync(ProviderId)
                    .ConfigureAwait(false);

                if (provider is null)
                    return ErrorResult
                        .NotFound($"A Provider with the id '{ProviderId}' could not be found..")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(provider);

                if (!(asyncCallback is null))
                    return await asyncCallback(provider)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public Task<IActionResult> EnsureUserAsync(Func<UserDocument, Task<IActionResult>> callback)
            => EnsureUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureUserAsync(Func<UserDocument, IActionResult> callback)
            => EnsureUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureUserInternalAsync(Func<UserDocument, Task<IActionResult>> asyncCallback = null, Func<UserDocument, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(UserIdentifier))
                    return ErrorResult
                        .BadRequest($"User name or id provided in the url path is invalid.  Must be a valid email address, service pricipal name, or GUID.", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var userId = await UserService
                    .GetUserIdAsync(UserIdentifier)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(userId))
                    return ErrorResult
                        .NotFound($"A User with the name or id '{UserIdentifier}' could not be found.")
                        .ToActionResult();

                var user = await UserRepository
                    .GetAsync(userId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User with the Id '{UserIdentifier}' could not be found..")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(user);

                if (!(asyncCallback is null))
                    return await asyncCallback(user)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public Task<IActionResult> EnsureCurrentUserAsync(Func<UserDocument, Task<IActionResult>> callback)
            => EnsureCurrentUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureCurrentUserAsync(Func<UserDocument, IActionResult> callback)
            => EnsureCurrentUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureCurrentUserInternalAsync(Func<UserDocument, Task<IActionResult>> asyncCallback = null, Func<UserDocument, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                var user = await UserService
                    .CurrentUserAsync()
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User matching the current authenticated user was not found..")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(user);

                if (!(asyncCallback is null))
                    return await asyncCallback(user)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public async Task<IActionResult> EnsureProjectAndProviderAsync(Func<ProjectDocument, ProviderDocument, Task<IActionResult>> callback)
        {
            try
            {
                if (callback is null)
                    throw new ArgumentNullException(nameof(callback));

                if (string.IsNullOrEmpty(ProjectIdentifier))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProviderId))
                    return ErrorResult
                        .BadRequest($"Provider id provided in the url path is invalid.  Must be a valid non-empty string.", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(ProjectIdentifier)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectIdentifier}' could not be found.")
                        .ToActionResult();

                var provider = await ProviderRepository
                    .GetAsync(ProviderId)
                    .ConfigureAwait(false);

                if (provider is null)
                    return ErrorResult
                        .NotFound($"A Provider with the id '{ProviderId}' could not be found..")
                        .ToActionResult();

                return await callback(project, provider)
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public Task<IActionResult> EnsureProjectAndUserAsync(Func<ProjectDocument, UserDocument, Task<IActionResult>> callback)
            => EnsureProjectAndUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAndUserAsync(Func<ProjectDocument, UserDocument, IActionResult> callback)
            => EnsureProjectAndUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectAndUserInternalAsync(Func<ProjectDocument, UserDocument, Task<IActionResult>> asyncCallback = null, Func<ProjectDocument, UserDocument, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(ProjectIdentifier))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                if (string.IsNullOrEmpty(UserIdentifier))
                    return ErrorResult
                        .BadRequest($"User name or id provided in the url path is invalid.  Must be a valid email address, service pricipal name, or GUID.", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var userId = await UserService
                    .GetUserIdAsync(UserIdentifier)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(userId))
                    return ErrorResult
                        .NotFound($"A User with the name or id '{UserIdentifier}' could not be found.")
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(ProjectIdentifier)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectIdentifier}' could not be found.")
                        .ToActionResult();

                var user = await UserRepository
                    .GetAsync(userId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User with the Id '{UserIdentifier}' could not be found..")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(project, user);

                if (!(asyncCallback is null))
                    return await asyncCallback(project, user)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [NonAction]
        public Task<IActionResult> EnsureProjectAndCurrentUserAsync(Func<ProjectDocument, UserDocument, Task<IActionResult>> callback)
            => EnsureProjectAndCurrentUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAndCurrentUserAsync(Func<ProjectDocument, UserDocument, IActionResult> callback)
            => EnsureProjectAndCurrentUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectAndCurrentUserInternalAsync(Func<ProjectDocument, UserDocument, Task<IActionResult>> asyncCallback = null, Func<ProjectDocument, UserDocument, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(ProjectIdentifier))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(ProjectIdentifier)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectIdentifier}' could not be found.")
                        .ToActionResult();

                var user = await UserService
                    .CurrentUserAsync()
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User matching the current authenticated user was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(project, user);

                if (!(asyncCallback is null))
                    return await asyncCallback(project, user)
                        .ConfigureAwait(false);

                throw new InvalidOperationException("asyncCallback or callback must have a value");
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }
    }
}
