/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    public abstract class ApiController : ControllerBase
    {
        private string OrganizationId => RouteData.ValueOrDefault(nameof(OrganizationId));

        private string ProjectTemplateId => RouteData.ValueOrDefault(nameof(ProjectTemplateId));

        private string ProjectIdentityId => RouteData.ValueOrDefault(nameof(ProjectIdentityId));

        public string UserId => RouteData.ValueOrDefault(nameof(UserId));

        public string ProjectId => RouteData.ValueOrDefault(nameof(ProjectId));

        public string ComponentId => RouteData.ValueOrDefault(nameof(ComponentId));

        protected T GetService<T>()
            => (T)HttpContext.RequestServices.GetService(typeof(T));

        public UserService UserService
            => GetService<UserService>();

        public Orchestrator Orchestrator
            => GetService<Orchestrator>();

        [NonAction]
        internal async Task<IActionResult> ExecuteAsync(Func<User, Organization, Task<IActionResult>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            try
            {
                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                var organization = await GetService<IOrganizationRepository>()
                    .GetAsync(GetService<UserService>().CurrentUserTenant, OrganizationId)
                    .ConfigureAwait(false);

                if (organization is null)
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{OrganizationId}' was not found.")
                        .ToActionResult();

                var contextUser = await GetService<UserService>()
                    .CurrentUserAsync(organization.Id)
                    .ConfigureAwait(false);

                return await callback(contextUser, organization)
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
        internal Task<IActionResult> ExecuteAsync(Func<User, Organization, User, Task<IActionResult>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            return ExecuteAsync(async (contextUser, organization) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(UserId))
                        return ErrorResult
                            .BadRequest($"User name or id provided in the url path is invalid.  Must be a valid email address, service pricipal name, or GUID.", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    var userId = await GetService<UserService>()
                        .GetUserIdAsync(UserId)
                        .ConfigureAwait(false);

                    if (string.IsNullOrEmpty(userId))
                        return ErrorResult
                            .NotFound($"A User with the name or id '{UserId}' was not found.")
                            .ToActionResult();

                    var user = await GetService<IUserRepository>()
                        .GetAsync(OrganizationId, userId)
                        .ConfigureAwait(false);

                    if (user is null)
                        return ErrorResult
                            .NotFound($"A User with the Id '{UserId}' was not found.")
                            .ToActionResult();

                    return await callback(contextUser, organization, user)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    return ErrorResult
                        .ServerError(exc)
                        .ToActionResult();
                }
            });
        }

        [NonAction]
        internal Task<IActionResult> ExecuteAsync(Func<User, Organization, Project, Task<IActionResult>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            return ExecuteAsync(async (contextUser, organization) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(ProjectId))
                        return ErrorResult
                            .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    var project = await GetService<IProjectRepository>()
                        .GetAsync(organization.Id, ProjectId)
                        .ConfigureAwait(false);

                    if (project is null)
                        return ErrorResult
                            .NotFound($"A Project with the name or id '{ProjectId}' was not found.")
                            .ToActionResult();

                    return await callback(contextUser, organization, project)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    return ErrorResult
                        .ServerError(exc)
                        .ToActionResult();
                }
            });
        }


        [NonAction]
        internal Task<IActionResult> ExecuteAsync(Func<User, Organization, ProjectTemplate, Task<IActionResult>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            return ExecuteAsync(async (contextUser, organization) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(ProjectTemplateId))
                        return ErrorResult
                            .BadRequest($"Project Template name or id provided in the url path is invalid.  Must be a valid project template name or id (guid).", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    var projectTemplate = await GetService<IProjectTemplateRepository>()
                        .GetAsync(OrganizationId, ProjectTemplateId)
                        .ConfigureAwait(false);

                    if (projectTemplate is null)
                        return ErrorResult
                            .NotFound($"A Project Template with the name or id '{ProjectTemplateId}' was not found.")
                            .ToActionResult();

                    return await callback(contextUser, organization, projectTemplate)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    return ErrorResult
                        .ServerError(exc)
                        .ToActionResult();
                }
            });
        }


        [NonAction]
        internal Task<IActionResult> ExecuteAsync(Func<User, Organization, Project, User, Task<IActionResult>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            return ExecuteAsync(async (contextUser, organization, project) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(UserId))
                        return ErrorResult
                            .BadRequest($"User name or id provided in the url path is invalid.  Must be a valid email address, service pricipal name, or GUID.", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    var userId = await GetService<UserService>()
                        .GetUserIdAsync(UserId)
                        .ConfigureAwait(false);

                    if (string.IsNullOrEmpty(userId))
                        return ErrorResult
                            .NotFound($"A User with the name or id '{UserId}' was not found.")
                            .ToActionResult();

                    var user = await GetService<IUserRepository>()
                        .GetAsync(OrganizationId, userId)
                        .ConfigureAwait(false);

                    if (user is null)
                        return ErrorResult
                            .NotFound($"A User with the Id '{UserId}' was not found.")
                            .ToActionResult();

                    return await callback(contextUser, organization, project, user)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    return ErrorResult
                        .ServerError(exc)
                        .ToActionResult();
                }
            });
        }

        [NonAction]
        internal Task<IActionResult> ExecuteAsync(Func<User, Organization, Project, ProjectIdentity, Task<IActionResult>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            return ExecuteAsync(async (contextUser, organization, project) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(ProjectIdentityId))
                        return ErrorResult
                            .BadRequest($"Project Identity Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    var projectIdentity = await GetService<IProjectIdentityRepository>()
                        .GetAsync(project.Id, ProjectIdentityId)
                        .ConfigureAwait(false);

                    if (projectIdentity is null)
                        return ErrorResult
                            .NotFound($"A Project Identity with the Id '{ProjectIdentityId}' was not found.")
                            .ToActionResult();

                    return await callback(contextUser, organization, project, projectIdentity)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    return ErrorResult
                        .ServerError(exc)
                        .ToActionResult();
                }
            });
        }

        [NonAction]
        internal Task<IActionResult> ExecuteAsync(Func<User, Organization, Project, ProjectTemplate, Task<IActionResult>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            return ExecuteAsync(new Func<User, Organization, Project, Task<IActionResult>>(async (contextUser, organization, project) =>
            {
                try
                {
                    var projectTemplate = await GetService<IProjectTemplateRepository>()
                        .GetAsync(project.Organization, project.Template)
                        .ConfigureAwait(false);

                    if (projectTemplate is null)
                        return ErrorResult
                            .NotFound($"A Project Template with the name or id '{project.Template}' was not found.")
                            .ToActionResult();

                    return await callback(contextUser, organization, project, projectTemplate)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    return ErrorResult
                        .ServerError(exc)
                        .ToActionResult();
                }
            }));
        }


        [NonAction]
        internal Task<IActionResult> ExecuteAsync(Func<User, Organization, Project, Component, Task<IActionResult>> callback)
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            return ExecuteAsync(async (contextUser, organization, project) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(ComponentId))
                        return ErrorResult
                            .BadRequest($"Component name or id provided in the url path is invalid.  Must be a valid component name or id (guid).", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    var component = await GetService<IComponentRepository>()
                        .GetAsync(project.Id, ComponentId)
                        .ConfigureAwait(false);

                    if (component is null)
                        return ErrorResult
                            .NotFound($"A Component with id '{ComponentId}' was not found.")
                            .ToActionResult();

                    return await callback(contextUser, organization, project, component)
                        .ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    return ErrorResult
                        .ServerError(exc)
                        .ToActionResult();
                }
            });
        }
    }
}
