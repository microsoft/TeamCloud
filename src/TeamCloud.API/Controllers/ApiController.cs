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

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository)
        {
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            Orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            OrganizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectRepository projectRepository)
            : this(userService, orchestrator, organizationRepository)
        {
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        // protected ApiController(UserService userService, Orchestrator orchestrator, IProviderRepository providerRepository)
        //     : this(userService, orchestrator)
        // {
        //     ProviderRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        // }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IUserRepository userRepository)
            : this(userService, orchestrator, organizationRepository)
        {
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectTemplateRepository projectTemplateRepository)
            : this(userService, orchestrator, organizationRepository)
        {
            ProjectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
        }

        // protected ApiController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IProviderRepository providerRepository)
        //     : this(userService, orchestrator)
        // {
        //     ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        //     ProviderRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        // }

        protected ApiController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IProjectRepository projectRepository, IUserRepository userRepository)
            : this(userService, orchestrator, organizationRepository)
        {
            ProjectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        private string ProjectId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

        private string ProjectNameOrId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectNameOrId), StringComparison.OrdinalIgnoreCase)?.ToString();

        private string UserId
            => RouteData.Values.GetValueOrDefault(nameof(UserId), StringComparison.OrdinalIgnoreCase)?.ToString();

        private string UserNameOrId
            => RouteData.Values.GetValueOrDefault(nameof(UserNameOrId), StringComparison.OrdinalIgnoreCase)?.ToString();

        private string ProjectTemplateId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectTemplateId), StringComparison.OrdinalIgnoreCase)?.ToString();

        private string Organization
            => RouteData.Values.GetValueOrDefault(nameof(Organization), StringComparison.OrdinalIgnoreCase)?.ToString();

        public string OrganizationId { get; private set; }

        public string ProjectIdentifier => ProjectId ?? ProjectNameOrId;

        public string UserIdentifier => UserId ?? UserNameOrId;

        public UserService UserService { get; }

        public Orchestrator Orchestrator { get; }

        public IUserRepository UserRepository { get; }

        public IProjectRepository ProjectRepository { get; }

        public IOrganizationRepository OrganizationRepository { get; }

        public IProjectTemplateRepository ProjectTemplateRepository { get; }

        [NonAction]
        public Task<IActionResult> ResolveOrganizationIdAsync(Func<string, Task<IActionResult>> callback)
            => ResolveOrganizationIdInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> ResolveOrganizationIdAsync(Func<string, IActionResult> callback)
            => ResolveOrganizationIdInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> ResolveOrganizationIdInternalAsync(Func<string, Task<IActionResult>> asyncCallback = null, Func<string, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Organization))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrganizationId = await OrganizationRepository
                    .ResolveIdAsync(Organization)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Organization}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(OrganizationId);

                if (!(asyncCallback is null))
                    return await asyncCallback(OrganizationId)
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
        public Task<IActionResult> EnsureOrganizationAsync(Func<Organization, Task<IActionResult>> callback)
            => EnsureOrganizationInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureOrganizationAsync(Func<Organization, IActionResult> callback)
            => EnsureOrganizationInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureOrganizationInternalAsync(Func<Organization, Task<IActionResult>> asyncCallback = null, Func<Organization, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Organization))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var organization = await OrganizationRepository
                    .GetAsync(Organization)
                    .ConfigureAwait(false);

                if (organization is null)
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Organization}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(organization);

                if (!(asyncCallback is null))
                    return await asyncCallback(organization)
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
        public Task<IActionResult> EnsureProjectAsync(Func<Project, Task<IActionResult>> callback)
            => EnsureProjectInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAsync(Func<Project, IActionResult> callback)
            => EnsureProjectInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectInternalAsync(Func<Project, Task<IActionResult>> asyncCallback = null, Func<Project, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Organization))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrganizationId = await OrganizationRepository
                    .ResolveIdAsync(Organization)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Organization}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProjectIdentifier))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(OrganizationId, ProjectIdentifier)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectIdentifier}' was not found.")
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
        public Task<IActionResult> EnsureProjectTemplateAsync(Func<ProjectTemplate, Task<IActionResult>> callback)
            => EnsureProjectTemplateInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectTemplateAsync(Func<ProjectTemplate, IActionResult> callback)
            => EnsureProjectTemplateInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectTemplateInternalAsync(Func<ProjectTemplate, Task<IActionResult>> asyncCallback = null, Func<ProjectTemplate, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Organization))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrganizationId = await OrganizationRepository
                    .ResolveIdAsync(Organization)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Organization}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProjectTemplateId))
                    return ErrorResult
                        .BadRequest($"Project Template name or id provided in the url path is invalid.  Must be a valid project template name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var template = await ProjectTemplateRepository
                    .GetAsync(OrganizationId, ProjectTemplateId)
                    .ConfigureAwait(false);

                if (template is null)
                    return ErrorResult
                        .NotFound($"A Project Template with the name or id '{ProjectTemplateId}' was not found.")
                        .ToActionResult();

                if (!(callback is null))
                    return callback(template);

                if (!(asyncCallback is null))
                    return await asyncCallback(template)
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
        public Task<IActionResult> EnsureUserAsync(Func<User, Task<IActionResult>> callback)
            => EnsureUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureUserAsync(Func<User, IActionResult> callback)
            => EnsureUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureUserInternalAsync(Func<User, Task<IActionResult>> asyncCallback = null, Func<User, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Organization))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrganizationId = await OrganizationRepository
                    .ResolveIdAsync(Organization)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Organization}' was not found.")
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
                        .NotFound($"A User with the name or id '{UserIdentifier}' was not found.")
                        .ToActionResult();

                var user = await UserRepository
                    .GetAsync(OrganizationId, userId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User with the Id '{UserIdentifier}' was not found.")
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
        public Task<IActionResult> EnsureCurrentUserAsync(Func<User, Task<IActionResult>> callback)
            => EnsureCurrentUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureCurrentUserAsync(Func<User, IActionResult> callback)
            => EnsureCurrentUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureCurrentUserInternalAsync(Func<User, Task<IActionResult>> asyncCallback = null, Func<User, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Organization))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrganizationId = await OrganizationRepository
                    .ResolveIdAsync(Organization)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Organization}' was not found.")
                        .ToActionResult();

                var user = await UserService
                    .CurrentUserAsync(OrganizationId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User matching the current authenticated user was not found.")
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
        public Task<IActionResult> EnsureProjectAndUserAsync(Func<Project, User, Task<IActionResult>> callback)
            => EnsureProjectAndUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAndUserAsync(Func<Project, User, IActionResult> callback)
            => EnsureProjectAndUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectAndUserInternalAsync(Func<Project, User, Task<IActionResult>> asyncCallback = null, Func<Project, User, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Organization))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrganizationId = await OrganizationRepository
                    .ResolveIdAsync(Organization)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Organization}' was not found.")
                        .ToActionResult();

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
                        .NotFound($"A User with the name or id '{UserIdentifier}' was not found.")
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(OrganizationId, ProjectIdentifier)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectIdentifier}' was not found.")
                        .ToActionResult();

                var user = await UserRepository
                    .GetAsync(OrganizationId, userId)
                    .ConfigureAwait(false);

                if (user is null)
                    return ErrorResult
                        .NotFound($"A User with the Id '{UserIdentifier}' was not found.")
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
        public Task<IActionResult> EnsureProjectAndCurrentUserAsync(Func<Project, User, Task<IActionResult>> callback)
            => EnsureProjectAndCurrentUserInternalAsync(asyncCallback: callback);

        [NonAction]
        public Task<IActionResult> EnsureProjectAndCurrentUserAsync(Func<Project, User, IActionResult> callback)
            => EnsureProjectAndCurrentUserInternalAsync(callback: callback);

        [NonAction]
        private async Task<IActionResult> EnsureProjectAndCurrentUserInternalAsync(Func<Project, User, Task<IActionResult>> asyncCallback = null, Func<Project, User, IActionResult> callback = null)
        {
            try
            {
                if (asyncCallback is null && callback is null)
                    throw new InvalidOperationException("asyncCallback or callback must have a value");

                if (!(asyncCallback is null || callback is null))
                    throw new InvalidOperationException("Only one of asyncCallback or callback can hava a value");

                if (string.IsNullOrEmpty(Organization))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                OrganizationId = await OrganizationRepository
                    .ResolveIdAsync(Organization)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{Organization}' was not found.")
                        .ToActionResult();

                if (string.IsNullOrEmpty(ProjectIdentifier))
                    return ErrorResult
                        .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var project = await ProjectRepository
                    .GetAsync(OrganizationId, ProjectIdentifier)
                    .ConfigureAwait(false);

                if (project is null)
                    return ErrorResult
                        .NotFound($"A Project with the name or id '{ProjectIdentifier}' was not found.")
                        .ToActionResult();

                var user = await UserService
                    .CurrentUserAsync(OrganizationId)
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
