/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeamCloud.API;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;

namespace TeamCloud.API.Controllers.Core
{
    public abstract class TeamCloudController : ControllerBase
    {
        protected TeamCloudController(ILogger log = null)
        {
            Log = log ?? NullLogger.Instance;
        }

        private string OrganizationId => RouteData.ValueOrDefault(nameof(OrganizationId));

        private string ProjectTemplateId => RouteData.ValueOrDefault(nameof(ProjectTemplateId));

        private string ProjectIdentityId => RouteData.ValueOrDefault(nameof(ProjectIdentityId));

        private string DeploymentScopeId => RouteData.ValueOrDefault(nameof(DeploymentScopeId));

        public string UserId => RouteData.ValueOrDefault(nameof(UserId));

        public string ProjectId => RouteData.ValueOrDefault(nameof(ProjectId));

        public string ComponentId => RouteData.ValueOrDefault(nameof(ComponentId));

        protected T GetService<T>()
            => (T)HttpContext.RequestServices.GetService(typeof(T));

        public UserService UserService
            => GetService<UserService>();

        public Orchestrator Orchestrator
            => GetService<Orchestrator>();

        protected ILogger Log { get; }

        [NonAction]
        internal async Task<IActionResult> ExecuteAsync<TContext>(Func<TContext, Task<IActionResult>> callback)
            where TContext : TeamCloudOrganizationContext, new()
        {
            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            try
            {
                var context = Activator.CreateInstance<TContext>();

                if (string.IsNullOrEmpty(OrganizationId))
                    return ErrorResult
                        .BadRequest($"Organization id or slug provided in the url path is invalid.  Must be a valid organization slug or id (guid).", ResultErrorCode.ValidationError)
                        .ToActionResult();

                context.ContextUser = await GetService<UserService>()
                    .CurrentUserAsync(OrganizationId)
                    .ConfigureAwait(false);

                context.Organization = await GetService<IOrganizationRepository>()
                    .GetAsync(GetService<UserService>().CurrentUserTenant, OrganizationId)
                    .ConfigureAwait(false);

                if (context.Organization is null)
                    return ErrorResult
                        .NotFound($"A Organization with the slug or id '{OrganizationId}' was not found.")
                        .ToActionResult();

                if (context is TeamCloudOrganizationUserContext userContext)
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

                    userContext.User = await GetService<IUserRepository>()
                        .GetAsync(OrganizationId, userId)
                        .ConfigureAwait(false);

                    if (userContext.User is null)
                        return ErrorResult
                            .NotFound($"A User with the Id '{UserId}' was not found.")
                            .ToActionResult();
                }

                if (context is TeamCloudProjectContext projectContext)
                {
                    if (string.IsNullOrEmpty(ProjectId))
                        return ErrorResult
                            .BadRequest($"Project name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    projectContext.Project = await GetService<IProjectRepository>()
                        .GetAsync(projectContext.Organization.Id, ProjectId)
                        .ConfigureAwait(false);

                    if (projectContext.Project is null)
                        return ErrorResult
                            .NotFound($"A Project with the name or id '{ProjectId}' was not found.")
                            .ToActionResult();
                }

                if (context is TeamCloudDeploymentScopeContext deploymentScopeContext)
                {
                    if (string.IsNullOrEmpty(DeploymentScopeId))
                        return ErrorResult
                            .BadRequest($"Deployemnt Scope name or id provided in the url path is invalid.  Must be a valid project name or id (guid).", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    deploymentScopeContext.DeploymentScope = await GetService<IDeploymentScopeRepository>()
                        .GetAsync(deploymentScopeContext.Organization.Id, DeploymentScopeId)
                        .ConfigureAwait(false);

                    if (deploymentScopeContext.DeploymentScope is null)
                        return ErrorResult
                            .NotFound($"A Deployment Scope with the name or id '{DeploymentScopeId}' was not found.")
                            .ToActionResult();
                }

                if (context is TeamCloudProjectTemplateContext projectTemplateContext)
                {
                    if (string.IsNullOrEmpty(ProjectTemplateId))
                        return ErrorResult
                            .BadRequest($"Project Template name or id provided in the url path is invalid.  Must be a valid project template name or id (guid).", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    projectTemplateContext.ProjectTemplate = await GetService<IProjectTemplateRepository>()
                        .GetAsync(OrganizationId, ProjectTemplateId)
                        .ConfigureAwait(false);

                    if (projectTemplateContext.ProjectTemplate is null)
                        return ErrorResult
                            .NotFound($"A Project Template with the name or id '{ProjectTemplateId}' was not found.")
                            .ToActionResult();
                }

                if (context is TeamCloudProjectUserContext projectUserContext)
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

                    projectUserContext.User = await GetService<IUserRepository>()
                        .GetAsync(OrganizationId, userId)
                        .ConfigureAwait(false);

                    if (projectUserContext.User is null)
                        return ErrorResult
                            .NotFound($"A User with the Id '{UserId}' was not found.")
                            .ToActionResult();
                }

                if (context is TeamCloudProjectIdentityContext projectIdentityContext)
                {
                    if (string.IsNullOrEmpty(ProjectIdentityId))
                        return ErrorResult
                            .BadRequest($"Project Identity Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    projectIdentityContext.ProjectIdentity = await GetService<IProjectIdentityRepository>()
                        .GetAsync(projectIdentityContext.Project.Id, ProjectIdentityId)
                        .ConfigureAwait(false);

                    if (projectIdentityContext.ProjectIdentity is null)
                        return ErrorResult
                            .NotFound($"A Project Identity with the Id '{ProjectIdentityId}' was not found.")
                            .ToActionResult();
                }

                if (context is TeamCloudProjectComponentContext projectComponentContext)
                {
                    if (string.IsNullOrEmpty(ComponentId))
                        return ErrorResult
                            .BadRequest($"Component name or id provided in the url path is invalid.  Must be a valid component name or id (guid).", ResultErrorCode.ValidationError)
                            .ToActionResult();

                    projectComponentContext.Component = await GetService<IComponentRepository>()
                        .GetAsync(projectComponentContext.Project.Id, ComponentId)
                        .ConfigureAwait(false);

                    if (projectComponentContext.Component is null)
                        return ErrorResult
                            .NotFound($"A Component with id '{ComponentId}' was not found.")
                            .ToActionResult();
                }

                return await callback(context)
                    .ConfigureAwait(false);
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
