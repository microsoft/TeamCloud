import * as coreHttp from "@azure/core-http";
import { TeamCloudContext } from "./teamCloudContext";
import { TeamCloudOptionalParams, TeamCloudGetAdaptersResponse, TeamCloudGetComponentsOptionalParams, TeamCloudGetComponentsResponse, TeamCloudCreateComponentOptionalParams, TeamCloudCreateComponentResponse, TeamCloudGetComponentResponse, TeamCloudDeleteComponentResponse, TeamCloudGetComponentTasksResponse, TeamCloudCreateComponentTaskOptionalParams, TeamCloudCreateComponentTaskResponse, TeamCloudGetComponentTaskResponse, TeamCloudGetComponentTemplatesResponse, TeamCloudGetComponentTemplateResponse, TeamCloudGetDeploymentScopesResponse, TeamCloudCreateDeploymentScopeOptionalParams, TeamCloudCreateDeploymentScopeResponse, TeamCloudGetDeploymentScopeResponse, TeamCloudUpdateDeploymentScopeOptionalParams, TeamCloudUpdateDeploymentScopeResponse, TeamCloudDeleteDeploymentScopeResponse, TeamCloudAuthorizeDeploymentScopeOptionalParams, TeamCloudAuthorizeDeploymentScopeResponse, TeamCloudGetAuditEntriesOptionalParams, TeamCloudGetAuditEntriesResponse, TeamCloudGetAuditEntryOptionalParams, TeamCloudGetAuditEntryResponse, TeamCloudGetAuditCommandsResponse, TeamCloudGetOrganizationsResponse, TeamCloudCreateOrganizationOptionalParams, TeamCloudCreateOrganizationResponse, TeamCloudGetOrganizationResponse, TeamCloudDeleteOrganizationResponse, TeamCloudGetOrganizationUsersResponse, TeamCloudCreateOrganizationUserOptionalParams, TeamCloudCreateOrganizationUserResponse, TeamCloudGetOrganizationUserResponse, TeamCloudUpdateOrganizationUserOptionalParams, TeamCloudUpdateOrganizationUserResponse, TeamCloudDeleteOrganizationUserResponse, TeamCloudGetOrganizationUserMeResponse, TeamCloudUpdateOrganizationUserMeOptionalParams, TeamCloudUpdateOrganizationUserMeResponse, TeamCloudGetProjectsResponse, TeamCloudCreateProjectOptionalParams, TeamCloudCreateProjectResponse, TeamCloudGetProjectResponse, TeamCloudDeleteProjectResponse, TeamCloudGetProjectIdentitiesResponse, TeamCloudCreateProjectIdentityOptionalParams, TeamCloudCreateProjectIdentityResponse, TeamCloudGetProjectIdentityResponse, TeamCloudUpdateProjectIdentityOptionalParams, TeamCloudUpdateProjectIdentityResponse, TeamCloudDeleteProjectIdentityResponse, TeamCloudGetProjectTagsResponse, TeamCloudCreateProjectTagOptionalParams, TeamCloudCreateProjectTagResponse, TeamCloudUpdateProjectTagOptionalParams, TeamCloudUpdateProjectTagResponse, TeamCloudGetProjectTagByKeyResponse, TeamCloudDeleteProjectTagResponse, TeamCloudGetProjectTemplatesResponse, TeamCloudCreateProjectTemplateOptionalParams, TeamCloudCreateProjectTemplateResponse, TeamCloudGetProjectTemplateResponse, TeamCloudUpdateProjectTemplateOptionalParams, TeamCloudUpdateProjectTemplateResponse, TeamCloudDeleteProjectTemplateResponse, TeamCloudGetProjectUsersResponse, TeamCloudCreateProjectUserOptionalParams, TeamCloudCreateProjectUserResponse, TeamCloudGetProjectUserResponse, TeamCloudUpdateProjectUserOptionalParams, TeamCloudUpdateProjectUserResponse, TeamCloudDeleteProjectUserResponse, TeamCloudGetProjectUserMeResponse, TeamCloudUpdateProjectUserMeOptionalParams, TeamCloudUpdateProjectUserMeResponse, TeamCloudGetSchedulesResponse, TeamCloudCreateScheduleOptionalParams, TeamCloudCreateScheduleResponse, TeamCloudGetScheduleResponse, TeamCloudUpdateScheduleOptionalParams, TeamCloudUpdateScheduleResponse, TeamCloudRunScheduleResponse, TeamCloudGetStatusResponse, TeamCloudGetProjectStatusResponse, TeamCloudGetUserProjectsResponse, TeamCloudGetUserProjectsMeResponse } from "./models";
export declare class TeamCloud extends TeamCloudContext {
    /**
     * Initializes a new instance of the TeamCloud class.
     * @param credentials Subscription credentials which uniquely identify client subscription.
     * @param $host server parameter
     * @param options The parameter options
     */
    constructor(credentials: coreHttp.TokenCredential | coreHttp.ServiceClientCredentials, $host: string, options?: TeamCloudOptionalParams);
    /**
     * Gets all Adapters.
     * @param options The options parameters.
     */
    getAdapters(options?: coreHttp.OperationOptions): Promise<TeamCloudGetAdaptersResponse>;
    /**
     * Gets all Components for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponents(organizationId: string, projectId: string, options?: TeamCloudGetComponentsOptionalParams): Promise<TeamCloudGetComponentsResponse>;
    /**
     * Creates a new Project Component.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createComponent(organizationId: string, projectId: string, options?: TeamCloudCreateComponentOptionalParams): Promise<TeamCloudCreateComponentResponse>;
    /**
     * Gets a Project Component.
     * @param componentId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponent(componentId: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetComponentResponse>;
    /**
     * Deletes an existing Project Component.
     * @param componentId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteComponent(componentId: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteComponentResponse>;
    /**
     * Gets all Component Tasks.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    getComponentTasks(organizationId: string, projectId: string, componentId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetComponentTasksResponse>;
    /**
     * Creates a new Project Component Task.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    createComponentTask(organizationId: string, projectId: string, componentId: string | null, options?: TeamCloudCreateComponentTaskOptionalParams): Promise<TeamCloudCreateComponentTaskResponse>;
    /**
     * Gets the Component Task.
     * @param id
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    getComponentTask(id: string | null, organizationId: string, projectId: string, componentId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetComponentTaskResponse>;
    /**
     * Gets all Component Templates for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponentTemplates(organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetComponentTemplatesResponse>;
    /**
     * Gets the Component Template.
     * @param id
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponentTemplate(id: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetComponentTemplateResponse>;
    /**
     * Gets all Deployment Scopes.
     * @param organizationId
     * @param options The options parameters.
     */
    getDeploymentScopes(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetDeploymentScopesResponse>;
    /**
     * Creates a new Deployment Scope.
     * @param organizationId
     * @param options The options parameters.
     */
    createDeploymentScope(organizationId: string, options?: TeamCloudCreateDeploymentScopeOptionalParams): Promise<TeamCloudCreateDeploymentScopeResponse>;
    /**
     * Gets a Deployment Scope.
     * @param deploymentScopeId
     * @param organizationId
     * @param options The options parameters.
     */
    getDeploymentScope(deploymentScopeId: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetDeploymentScopeResponse>;
    /**
     * Updates an existing Deployment Scope.
     * @param deploymentScopeId
     * @param organizationId
     * @param options The options parameters.
     */
    updateDeploymentScope(deploymentScopeId: string | null, organizationId: string, options?: TeamCloudUpdateDeploymentScopeOptionalParams): Promise<TeamCloudUpdateDeploymentScopeResponse>;
    /**
     * Deletes a Deployment Scope.
     * @param deploymentScopeId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteDeploymentScope(deploymentScopeId: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteDeploymentScopeResponse>;
    /**
     * Authorize an existing Deployment Scope.
     * @param deploymentScopeId
     * @param organizationId
     * @param options The options parameters.
     */
    authorizeDeploymentScope(deploymentScopeId: string | null, organizationId: string, options?: TeamCloudAuthorizeDeploymentScopeOptionalParams): Promise<TeamCloudAuthorizeDeploymentScopeResponse>;
    /**
     * Negotiates the SignalR connection.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    negotiateSignalR(organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<coreHttp.RestResponse>;
    /**
     * Gets all audit entries.
     * @param organizationId
     * @param options The options parameters.
     */
    getAuditEntries(organizationId: string, options?: TeamCloudGetAuditEntriesOptionalParams): Promise<TeamCloudGetAuditEntriesResponse>;
    /**
     * Gets an audit entry.
     * @param commandId
     * @param organizationId
     * @param options The options parameters.
     */
    getAuditEntry(commandId: string, organizationId: string, options?: TeamCloudGetAuditEntryOptionalParams): Promise<TeamCloudGetAuditEntryResponse>;
    /**
     * Gets all auditable commands.
     * @param organizationId
     * @param options The options parameters.
     */
    getAuditCommands(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetAuditCommandsResponse>;
    /**
     * Gets all Organizations.
     * @param options The options parameters.
     */
    getOrganizations(options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationsResponse>;
    /**
     * Creates a new Organization.
     * @param options The options parameters.
     */
    createOrganization(options?: TeamCloudCreateOrganizationOptionalParams): Promise<TeamCloudCreateOrganizationResponse>;
    /**
     * Gets an Organization.
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganization(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationResponse>;
    /**
     * Deletes an existing Organization.
     * @param organizationId
     * @param options The options parameters.
     */
    deleteOrganization(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteOrganizationResponse>;
    /**
     * Gets all Users.
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganizationUsers(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationUsersResponse>;
    /**
     * Creates a new User.
     * @param organizationId
     * @param options The options parameters.
     */
    createOrganizationUser(organizationId: string, options?: TeamCloudCreateOrganizationUserOptionalParams): Promise<TeamCloudCreateOrganizationUserResponse>;
    /**
     * Gets a User.
     * @param userId
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganizationUser(userId: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationUserResponse>;
    /**
     * Updates an existing User.
     * @param userId
     * @param organizationId
     * @param options The options parameters.
     */
    updateOrganizationUser(userId: string | null, organizationId: string, options?: TeamCloudUpdateOrganizationUserOptionalParams): Promise<TeamCloudUpdateOrganizationUserResponse>;
    /**
     * Deletes an existing User.
     * @param userId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteOrganizationUser(userId: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteOrganizationUserResponse>;
    /**
     * Gets a User A User matching the current authenticated user.
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganizationUserMe(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationUserMeResponse>;
    /**
     * Updates an existing User.
     * @param organizationId
     * @param options The options parameters.
     */
    updateOrganizationUserMe(organizationId: string, options?: TeamCloudUpdateOrganizationUserMeOptionalParams): Promise<TeamCloudUpdateOrganizationUserMeResponse>;
    /**
     * Gets all Projects.
     * @param organizationId
     * @param options The options parameters.
     */
    getProjects(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectsResponse>;
    /**
     * Creates a new Project.
     * @param organizationId
     * @param options The options parameters.
     */
    createProject(organizationId: string, options?: TeamCloudCreateProjectOptionalParams): Promise<TeamCloudCreateProjectResponse>;
    /**
     * Gets a Project.
     * @param projectId
     * @param organizationId
     * @param options The options parameters.
     */
    getProject(projectId: string, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectResponse>;
    /**
     * Deletes a Project.
     * @param projectId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteProject(projectId: string, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectResponse>;
    /**
     * Gets all Project Identities.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectIdentities(organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectIdentitiesResponse>;
    /**
     * Creates a new Project Identity.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectIdentity(organizationId: string, projectId: string, options?: TeamCloudCreateProjectIdentityOptionalParams): Promise<TeamCloudCreateProjectIdentityResponse>;
    /**
     * Gets a Project Identity.
     * @param projectIdentityId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectIdentity(projectIdentityId: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectIdentityResponse>;
    /**
     * Updates an existing Project Identity.
     * @param projectIdentityId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectIdentity(projectIdentityId: string | null, organizationId: string, projectId: string, options?: TeamCloudUpdateProjectIdentityOptionalParams): Promise<TeamCloudUpdateProjectIdentityResponse>;
    /**
     * Deletes a Project Identity.
     * @param projectIdentityId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectIdentity(projectIdentityId: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectIdentityResponse>;
    /**
     * Gets all Tags for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTags(organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTagsResponse>;
    /**
     * Creates a new Project Tag.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectTag(organizationId: string, projectId: string, options?: TeamCloudCreateProjectTagOptionalParams): Promise<TeamCloudCreateProjectTagResponse>;
    /**
     * Updates an existing Project Tag.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectTag(organizationId: string, projectId: string, options?: TeamCloudUpdateProjectTagOptionalParams): Promise<TeamCloudUpdateProjectTagResponse>;
    /**
     * Gets a Project Tag by Key.
     * @param tagKey
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTagByKey(tagKey: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTagByKeyResponse>;
    /**
     * Deletes an existing Project Tag.
     * @param tagKey
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectTag(tagKey: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectTagResponse>;
    /**
     * Gets all Project Templates.
     * @param organizationId
     * @param options The options parameters.
     */
    getProjectTemplates(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTemplatesResponse>;
    /**
     * Creates a new Project Template.
     * @param organizationId
     * @param options The options parameters.
     */
    createProjectTemplate(organizationId: string, options?: TeamCloudCreateProjectTemplateOptionalParams): Promise<TeamCloudCreateProjectTemplateResponse>;
    /**
     * Gets a Project Template.
     * @param projectTemplateId
     * @param organizationId
     * @param options The options parameters.
     */
    getProjectTemplate(projectTemplateId: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTemplateResponse>;
    /**
     * Updates an existing Project Template.
     * @param projectTemplateId
     * @param organizationId
     * @param options The options parameters.
     */
    updateProjectTemplate(projectTemplateId: string | null, organizationId: string, options?: TeamCloudUpdateProjectTemplateOptionalParams): Promise<TeamCloudUpdateProjectTemplateResponse>;
    /**
     * Deletes a Project Template.
     * @param projectTemplateId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteProjectTemplate(projectTemplateId: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectTemplateResponse>;
    /**
     * Gets all Users for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUsers(organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUsersResponse>;
    /**
     * Creates a new Project User
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectUser(organizationId: string, projectId: string, options?: TeamCloudCreateProjectUserOptionalParams): Promise<TeamCloudCreateProjectUserResponse>;
    /**
     * Gets a Project User by ID or email address.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUser(userId: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUserResponse>;
    /**
     * Updates an existing Project User.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUser(userId: string | null, organizationId: string, projectId: string, options?: TeamCloudUpdateProjectUserOptionalParams): Promise<TeamCloudUpdateProjectUserResponse>;
    /**
     * Deletes an existing Project User.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectUser(userId: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectUserResponse>;
    /**
     * Gets a Project User for the calling user.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUserMe(organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUserMeResponse>;
    /**
     * Updates an existing Project User.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUserMe(organizationId: string, projectId: string, options?: TeamCloudUpdateProjectUserMeOptionalParams): Promise<TeamCloudUpdateProjectUserMeResponse>;
    /**
     * Gets all Schedule.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getSchedules(organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetSchedulesResponse>;
    /**
     * Creates a new Project Schedule.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createSchedule(organizationId: string, projectId: string, options?: TeamCloudCreateScheduleOptionalParams): Promise<TeamCloudCreateScheduleResponse>;
    /**
     * Gets the Schedule.
     * @param scheduleId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getSchedule(scheduleId: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetScheduleResponse>;
    /**
     * Updates a Project Schedule.
     * @param scheduleId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateSchedule(scheduleId: string | null, organizationId: string, projectId: string, options?: TeamCloudUpdateScheduleOptionalParams): Promise<TeamCloudUpdateScheduleResponse>;
    /**
     * Runs a Project Schedule.
     * @param scheduleId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    runSchedule(scheduleId: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudRunScheduleResponse>;
    /**
     * Gets the status of a long-running operation.
     * @param trackingId
     * @param organizationId
     * @param options The options parameters.
     */
    getStatus(trackingId: string, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetStatusResponse>;
    /**
     * Gets the status of a long-running operation.
     * @param projectId
     * @param trackingId
     * @param organizationId
     * @param options The options parameters.
     */
    getProjectStatus(projectId: string, trackingId: string, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectStatusResponse>;
    /**
     * Gets all Projects for a User.
     * @param organizationId
     * @param userId
     * @param options The options parameters.
     */
    getUserProjects(organizationId: string, userId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetUserProjectsResponse>;
    /**
     * Gets all Projects for a User.
     * @param organizationId
     * @param options The options parameters.
     */
    getUserProjectsMe(organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetUserProjectsMeResponse>;
}
//# sourceMappingURL=teamCloud.d.ts.map