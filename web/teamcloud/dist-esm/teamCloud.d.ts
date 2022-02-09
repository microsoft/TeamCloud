import * as coreAuth from "@azure/core-auth";
import { TeamCloudContext } from "./teamCloudContext";
import { TeamCloudOptionalParams, GetAdaptersOptionalParams, GetAdaptersResponse, GetComponentsOptionalParams, GetComponentsResponse, CreateComponentOptionalParams, CreateComponentResponse, GetComponentOptionalParams, GetComponentResponse, DeleteComponentOptionalParams, DeleteComponentResponse, GetComponentTasksOptionalParams, GetComponentTasksResponse, CreateComponentTaskOptionalParams, CreateComponentTaskResponse, GetComponentTaskOptionalParams, GetComponentTaskResponse, CancelComponentTaskOptionalParams, CancelComponentTaskResponse, ReRunComponentTaskOptionalParams, ReRunComponentTaskResponse, GetComponentTemplatesOptionalParams, GetComponentTemplatesResponse, GetComponentTemplateOptionalParams, GetComponentTemplateResponse, GetDeploymentScopesOptionalParams, GetDeploymentScopesResponse, CreateDeploymentScopeOptionalParams, CreateDeploymentScopeResponse, GetDeploymentScopeOptionalParams, GetDeploymentScopeResponse, UpdateDeploymentScopeOptionalParams, UpdateDeploymentScopeResponse, DeleteDeploymentScopeOptionalParams, DeleteDeploymentScopeResponse, InitializeAuthorizationOptionalParams, InitializeAuthorizationResponse, NegotiateSignalROptionalParams, GetAuditEntriesOptionalParams, GetAuditEntriesResponse, GetAuditEntryOptionalParams, GetAuditEntryResponse, GetAuditCommandsOptionalParams, GetAuditCommandsResponse, GetOrganizationsOptionalParams, GetOrganizationsResponse, CreateOrganizationOptionalParams, CreateOrganizationResponse, GetOrganizationOptionalParams, GetOrganizationResponse, DeleteOrganizationOptionalParams, DeleteOrganizationResponse, GetOrganizationUsersOptionalParams, GetOrganizationUsersResponse, CreateOrganizationUserOptionalParams, CreateOrganizationUserResponse, GetOrganizationUserOptionalParams, GetOrganizationUserResponse, UpdateOrganizationUserOptionalParams, UpdateOrganizationUserResponse, DeleteOrganizationUserOptionalParams, DeleteOrganizationUserResponse, GetOrganizationUserMeOptionalParams, GetOrganizationUserMeResponse, UpdateOrganizationUserMeOptionalParams, UpdateOrganizationUserMeResponse, GetProjectsOptionalParams, GetProjectsResponse, CreateProjectOptionalParams, CreateProjectResponse, GetProjectOptionalParams, GetProjectResponse, DeleteProjectOptionalParams, DeleteProjectResponse, GetProjectIdentitiesOptionalParams, GetProjectIdentitiesResponse, CreateProjectIdentityOptionalParams, CreateProjectIdentityResponse, GetProjectIdentityOptionalParams, GetProjectIdentityResponse, UpdateProjectIdentityOptionalParams, UpdateProjectIdentityResponse, DeleteProjectIdentityOptionalParams, DeleteProjectIdentityResponse, GetProjectTagsOptionalParams, GetProjectTagsResponse, CreateProjectTagOptionalParams, CreateProjectTagResponse, UpdateProjectTagOptionalParams, UpdateProjectTagResponse, GetProjectTagByKeyOptionalParams, GetProjectTagByKeyResponse, DeleteProjectTagOptionalParams, DeleteProjectTagResponse, GetProjectTemplatesOptionalParams, GetProjectTemplatesResponse, CreateProjectTemplateOptionalParams, CreateProjectTemplateResponse, GetProjectTemplateOptionalParams, GetProjectTemplateResponse, UpdateProjectTemplateOptionalParams, UpdateProjectTemplateResponse, DeleteProjectTemplateOptionalParams, DeleteProjectTemplateResponse, GetProjectUsersOptionalParams, GetProjectUsersResponse, CreateProjectUserOptionalParams, CreateProjectUserResponse, GetProjectUserOptionalParams, GetProjectUserResponse, UpdateProjectUserOptionalParams, UpdateProjectUserResponse, DeleteProjectUserOptionalParams, DeleteProjectUserResponse, GetProjectUserMeOptionalParams, GetProjectUserMeResponse, UpdateProjectUserMeOptionalParams, UpdateProjectUserMeResponse, GetInfoOptionalParams, GetInfoResponse, GetSchedulesOptionalParams, GetSchedulesResponse, CreateScheduleOptionalParams, CreateScheduleResponse, GetScheduleOptionalParams, GetScheduleResponse, UpdateScheduleOptionalParams, UpdateScheduleResponse, RunScheduleOptionalParams, RunScheduleResponse, GetStatusOptionalParams, GetStatusResponse, GetProjectStatusOptionalParams, GetProjectStatusResponse, GetUserProjectsOptionalParams, GetUserProjectsResponse, GetUserProjectsMeOptionalParams, GetUserProjectsMeResponse } from "./models";
export declare class TeamCloud extends TeamCloudContext {
    /**
     * Initializes a new instance of the TeamCloud class.
     * @param credentials Subscription credentials which uniquely identify client subscription.
     * @param $host server parameter
     * @param options The parameter options
     */
    constructor(credentials: coreAuth.TokenCredential, $host: string, options?: TeamCloudOptionalParams);
    /**
     * Gets all Adapters.
     * @param options The options parameters.
     */
    getAdapters(options?: GetAdaptersOptionalParams): Promise<GetAdaptersResponse>;
    /**
     * Gets all Components for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponents(organizationId: string, projectId: string, options?: GetComponentsOptionalParams): Promise<GetComponentsResponse>;
    /**
     * Creates a new Project Component.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createComponent(organizationId: string, projectId: string, options?: CreateComponentOptionalParams): Promise<CreateComponentResponse>;
    /**
     * Gets a Project Component.
     * @param componentId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponent(componentId: string, organizationId: string, projectId: string, options?: GetComponentOptionalParams): Promise<GetComponentResponse>;
    /**
     * Deletes an existing Project Component.
     * @param componentId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteComponent(componentId: string, organizationId: string, projectId: string, options?: DeleteComponentOptionalParams): Promise<DeleteComponentResponse>;
    /**
     * Gets all Component Tasks.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    getComponentTasks(organizationId: string, projectId: string, componentId: string, options?: GetComponentTasksOptionalParams): Promise<GetComponentTasksResponse>;
    /**
     * Creates a new Project Component Task.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    createComponentTask(organizationId: string, projectId: string, componentId: string, options?: CreateComponentTaskOptionalParams): Promise<CreateComponentTaskResponse>;
    /**
     * Gets the Component Task.
     * @param taskId
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    getComponentTask(taskId: string, organizationId: string, projectId: string, componentId: string, options?: GetComponentTaskOptionalParams): Promise<GetComponentTaskResponse>;
    /**
     * Rerun a Project Component Task.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param taskId
     * @param options The options parameters.
     */
    cancelComponentTask(organizationId: string, projectId: string, componentId: string, taskId: string, options?: CancelComponentTaskOptionalParams): Promise<CancelComponentTaskResponse>;
    /**
     * Cancel an active Project Component Task.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param taskId
     * @param options The options parameters.
     */
    reRunComponentTask(organizationId: string, projectId: string, componentId: string, taskId: string, options?: ReRunComponentTaskOptionalParams): Promise<ReRunComponentTaskResponse>;
    /**
     * Gets all Component Templates for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponentTemplates(organizationId: string, projectId: string, options?: GetComponentTemplatesOptionalParams): Promise<GetComponentTemplatesResponse>;
    /**
     * Gets the Component Template.
     * @param id
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponentTemplate(id: string, organizationId: string, projectId: string, options?: GetComponentTemplateOptionalParams): Promise<GetComponentTemplateResponse>;
    /**
     * Gets all Deployment Scopes.
     * @param organizationId
     * @param options The options parameters.
     */
    getDeploymentScopes(organizationId: string, options?: GetDeploymentScopesOptionalParams): Promise<GetDeploymentScopesResponse>;
    /**
     * Creates a new Deployment Scope.
     * @param organizationId
     * @param options The options parameters.
     */
    createDeploymentScope(organizationId: string, options?: CreateDeploymentScopeOptionalParams): Promise<CreateDeploymentScopeResponse>;
    /**
     * Gets a Deployment Scope.
     * @param organizationId
     * @param deploymentScopeId
     * @param options The options parameters.
     */
    getDeploymentScope(organizationId: string, deploymentScopeId: string, options?: GetDeploymentScopeOptionalParams): Promise<GetDeploymentScopeResponse>;
    /**
     * Updates an existing Deployment Scope.
     * @param organizationId
     * @param deploymentScopeId
     * @param options The options parameters.
     */
    updateDeploymentScope(organizationId: string, deploymentScopeId: string, options?: UpdateDeploymentScopeOptionalParams): Promise<UpdateDeploymentScopeResponse>;
    /**
     * Deletes a Deployment Scope.
     * @param organizationId
     * @param deploymentScopeId
     * @param options The options parameters.
     */
    deleteDeploymentScope(organizationId: string, deploymentScopeId: string, options?: DeleteDeploymentScopeOptionalParams): Promise<DeleteDeploymentScopeResponse>;
    /**
     * Initialize a new authorization session for a deployment scope.
     * @param organizationId
     * @param deploymentScopeId
     * @param options The options parameters.
     */
    initializeAuthorization(organizationId: string, deploymentScopeId: string, options?: InitializeAuthorizationOptionalParams): Promise<InitializeAuthorizationResponse>;
    /**
     * Negotiates the SignalR connection.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    negotiateSignalR(organizationId: string, projectId: string, options?: NegotiateSignalROptionalParams): Promise<void>;
    /**
     * Gets all audit entries.
     * @param organizationId
     * @param options The options parameters.
     */
    getAuditEntries(organizationId: string, options?: GetAuditEntriesOptionalParams): Promise<GetAuditEntriesResponse>;
    /**
     * Gets an audit entry.
     * @param commandId
     * @param organizationId
     * @param options The options parameters.
     */
    getAuditEntry(commandId: string, organizationId: string, options?: GetAuditEntryOptionalParams): Promise<GetAuditEntryResponse>;
    /**
     * Gets all auditable commands.
     * @param organizationId
     * @param options The options parameters.
     */
    getAuditCommands(organizationId: string, options?: GetAuditCommandsOptionalParams): Promise<GetAuditCommandsResponse>;
    /**
     * Gets all Organizations.
     * @param options The options parameters.
     */
    getOrganizations(options?: GetOrganizationsOptionalParams): Promise<GetOrganizationsResponse>;
    /**
     * Creates a new Organization.
     * @param options The options parameters.
     */
    createOrganization(options?: CreateOrganizationOptionalParams): Promise<CreateOrganizationResponse>;
    /**
     * Gets an Organization.
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganization(organizationId: string, options?: GetOrganizationOptionalParams): Promise<GetOrganizationResponse>;
    /**
     * Deletes an existing Organization.
     * @param organizationId
     * @param options The options parameters.
     */
    deleteOrganization(organizationId: string, options?: DeleteOrganizationOptionalParams): Promise<DeleteOrganizationResponse>;
    /**
     * Gets all Users.
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganizationUsers(organizationId: string, options?: GetOrganizationUsersOptionalParams): Promise<GetOrganizationUsersResponse>;
    /**
     * Creates a new User.
     * @param organizationId
     * @param options The options parameters.
     */
    createOrganizationUser(organizationId: string, options?: CreateOrganizationUserOptionalParams): Promise<CreateOrganizationUserResponse>;
    /**
     * Gets a User.
     * @param userId
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganizationUser(userId: string, organizationId: string, options?: GetOrganizationUserOptionalParams): Promise<GetOrganizationUserResponse>;
    /**
     * Updates an existing User.
     * @param userId
     * @param organizationId
     * @param options The options parameters.
     */
    updateOrganizationUser(userId: string, organizationId: string, options?: UpdateOrganizationUserOptionalParams): Promise<UpdateOrganizationUserResponse>;
    /**
     * Deletes an existing User.
     * @param userId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteOrganizationUser(userId: string, organizationId: string, options?: DeleteOrganizationUserOptionalParams): Promise<DeleteOrganizationUserResponse>;
    /**
     * Gets a User A User matching the current authenticated user.
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganizationUserMe(organizationId: string, options?: GetOrganizationUserMeOptionalParams): Promise<GetOrganizationUserMeResponse>;
    /**
     * Updates an existing User.
     * @param organizationId
     * @param options The options parameters.
     */
    updateOrganizationUserMe(organizationId: string, options?: UpdateOrganizationUserMeOptionalParams): Promise<UpdateOrganizationUserMeResponse>;
    /**
     * Gets all Projects.
     * @param organizationId
     * @param options The options parameters.
     */
    getProjects(organizationId: string, options?: GetProjectsOptionalParams): Promise<GetProjectsResponse>;
    /**
     * Creates a new Project.
     * @param organizationId
     * @param options The options parameters.
     */
    createProject(organizationId: string, options?: CreateProjectOptionalParams): Promise<CreateProjectResponse>;
    /**
     * Gets a Project.
     * @param projectId
     * @param organizationId
     * @param options The options parameters.
     */
    getProject(projectId: string, organizationId: string, options?: GetProjectOptionalParams): Promise<GetProjectResponse>;
    /**
     * Deletes a Project.
     * @param projectId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteProject(projectId: string, organizationId: string, options?: DeleteProjectOptionalParams): Promise<DeleteProjectResponse>;
    /**
     * Gets all Project Identities.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectIdentities(organizationId: string, projectId: string, options?: GetProjectIdentitiesOptionalParams): Promise<GetProjectIdentitiesResponse>;
    /**
     * Creates a new Project Identity.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectIdentity(organizationId: string, projectId: string, options?: CreateProjectIdentityOptionalParams): Promise<CreateProjectIdentityResponse>;
    /**
     * Gets a Project Identity.
     * @param projectIdentityId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectIdentity(projectIdentityId: string, organizationId: string, projectId: string, options?: GetProjectIdentityOptionalParams): Promise<GetProjectIdentityResponse>;
    /**
     * Updates an existing Project Identity.
     * @param projectIdentityId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectIdentity(projectIdentityId: string, organizationId: string, projectId: string, options?: UpdateProjectIdentityOptionalParams): Promise<UpdateProjectIdentityResponse>;
    /**
     * Deletes a Project Identity.
     * @param projectIdentityId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectIdentity(projectIdentityId: string, organizationId: string, projectId: string, options?: DeleteProjectIdentityOptionalParams): Promise<DeleteProjectIdentityResponse>;
    /**
     * Gets all Tags for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTags(organizationId: string, projectId: string, options?: GetProjectTagsOptionalParams): Promise<GetProjectTagsResponse>;
    /**
     * Creates a new Project Tag.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectTag(organizationId: string, projectId: string, options?: CreateProjectTagOptionalParams): Promise<CreateProjectTagResponse>;
    /**
     * Updates an existing Project Tag.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectTag(organizationId: string, projectId: string, options?: UpdateProjectTagOptionalParams): Promise<UpdateProjectTagResponse>;
    /**
     * Gets a Project Tag by Key.
     * @param tagKey
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTagByKey(tagKey: string, organizationId: string, projectId: string, options?: GetProjectTagByKeyOptionalParams): Promise<GetProjectTagByKeyResponse>;
    /**
     * Deletes an existing Project Tag.
     * @param tagKey
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectTag(tagKey: string, organizationId: string, projectId: string, options?: DeleteProjectTagOptionalParams): Promise<DeleteProjectTagResponse>;
    /**
     * Gets all Project Templates.
     * @param organizationId
     * @param options The options parameters.
     */
    getProjectTemplates(organizationId: string, options?: GetProjectTemplatesOptionalParams): Promise<GetProjectTemplatesResponse>;
    /**
     * Creates a new Project Template.
     * @param organizationId
     * @param options The options parameters.
     */
    createProjectTemplate(organizationId: string, options?: CreateProjectTemplateOptionalParams): Promise<CreateProjectTemplateResponse>;
    /**
     * Gets a Project Template.
     * @param projectTemplateId
     * @param organizationId
     * @param options The options parameters.
     */
    getProjectTemplate(projectTemplateId: string, organizationId: string, options?: GetProjectTemplateOptionalParams): Promise<GetProjectTemplateResponse>;
    /**
     * Updates an existing Project Template.
     * @param projectTemplateId
     * @param organizationId
     * @param options The options parameters.
     */
    updateProjectTemplate(projectTemplateId: string, organizationId: string, options?: UpdateProjectTemplateOptionalParams): Promise<UpdateProjectTemplateResponse>;
    /**
     * Deletes a Project Template.
     * @param projectTemplateId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteProjectTemplate(projectTemplateId: string, organizationId: string, options?: DeleteProjectTemplateOptionalParams): Promise<DeleteProjectTemplateResponse>;
    /**
     * Gets all Users for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUsers(organizationId: string, projectId: string, options?: GetProjectUsersOptionalParams): Promise<GetProjectUsersResponse>;
    /**
     * Creates a new Project User
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectUser(organizationId: string, projectId: string, options?: CreateProjectUserOptionalParams): Promise<CreateProjectUserResponse>;
    /**
     * Gets a Project User by ID or email address.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUser(userId: string, organizationId: string, projectId: string, options?: GetProjectUserOptionalParams): Promise<GetProjectUserResponse>;
    /**
     * Updates an existing Project User.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUser(userId: string, organizationId: string, projectId: string, options?: UpdateProjectUserOptionalParams): Promise<UpdateProjectUserResponse>;
    /**
     * Deletes an existing Project User.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectUser(userId: string, organizationId: string, projectId: string, options?: DeleteProjectUserOptionalParams): Promise<DeleteProjectUserResponse>;
    /**
     * Gets a Project User for the calling user.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUserMe(organizationId: string, projectId: string, options?: GetProjectUserMeOptionalParams): Promise<GetProjectUserMeResponse>;
    /**
     * Updates an existing Project User.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUserMe(organizationId: string, projectId: string, options?: UpdateProjectUserMeOptionalParams): Promise<UpdateProjectUserMeResponse>;
    /**
     * Gets information about this TeamCloud deployment.
     * @param options The options parameters.
     */
    getInfo(options?: GetInfoOptionalParams): Promise<GetInfoResponse>;
    /**
     * Gets all Schedule.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getSchedules(organizationId: string, projectId: string, options?: GetSchedulesOptionalParams): Promise<GetSchedulesResponse>;
    /**
     * Creates a new Project Schedule.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createSchedule(organizationId: string, projectId: string, options?: CreateScheduleOptionalParams): Promise<CreateScheduleResponse>;
    /**
     * Gets the Schedule.
     * @param scheduleId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getSchedule(scheduleId: string, organizationId: string, projectId: string, options?: GetScheduleOptionalParams): Promise<GetScheduleResponse>;
    /**
     * Updates a Project Schedule.
     * @param scheduleId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateSchedule(scheduleId: string, organizationId: string, projectId: string, options?: UpdateScheduleOptionalParams): Promise<UpdateScheduleResponse>;
    /**
     * Runs a Project Schedule.
     * @param scheduleId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    runSchedule(scheduleId: string, organizationId: string, projectId: string, options?: RunScheduleOptionalParams): Promise<RunScheduleResponse>;
    /**
     * Gets the status of a long-running operation.
     * @param trackingId
     * @param organizationId
     * @param options The options parameters.
     */
    getStatus(trackingId: string, organizationId: string, options?: GetStatusOptionalParams): Promise<GetStatusResponse>;
    /**
     * Gets the status of a long-running operation.
     * @param projectId
     * @param trackingId
     * @param organizationId
     * @param options The options parameters.
     */
    getProjectStatus(projectId: string, trackingId: string, organizationId: string, options?: GetProjectStatusOptionalParams): Promise<GetProjectStatusResponse>;
    /**
     * Gets all Projects for a User.
     * @param organizationId
     * @param userId
     * @param options The options parameters.
     */
    getUserProjects(organizationId: string, userId: string, options?: GetUserProjectsOptionalParams): Promise<GetUserProjectsResponse>;
    /**
     * Gets all Projects for a User.
     * @param organizationId
     * @param options The options parameters.
     */
    getUserProjectsMe(organizationId: string, options?: GetUserProjectsMeOptionalParams): Promise<GetUserProjectsMeResponse>;
}
//# sourceMappingURL=teamCloud.d.ts.map