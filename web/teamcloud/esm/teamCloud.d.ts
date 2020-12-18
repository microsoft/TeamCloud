import * as coreHttp from "@azure/core-http";
import { TeamCloudContext } from "./teamCloudContext";
import { TeamCloudOptionalParams, TeamCloudGetDeploymentScopesResponse, TeamCloudCreateDeploymentScopeOptionalParams, TeamCloudCreateDeploymentScopeResponse, TeamCloudGetDeploymentScopeResponse, TeamCloudUpdateDeploymentScopeOptionalParams, TeamCloudUpdateDeploymentScopeResponse, TeamCloudDeleteDeploymentScopeResponse, TeamCloudGetOrganizationsResponse, TeamCloudCreateOrganizationOptionalParams, TeamCloudCreateOrganizationResponse, TeamCloudGetOrganizationResponse, TeamCloudDeleteOrganizationResponse, TeamCloudGetOrganizationUsersResponse, TeamCloudCreateOrganizationUserOptionalParams, TeamCloudCreateOrganizationUserResponse, TeamCloudGetOrganizationUserResponse, TeamCloudUpdateOrganizationUserOptionalParams, TeamCloudUpdateOrganizationUserResponse, TeamCloudDeleteOrganizationUserResponse, TeamCloudGetOrganizationUserMeResponse, TeamCloudUpdateOrganizationUserMeOptionalParams, TeamCloudUpdateOrganizationUserMeResponse, TeamCloudGetProjectsResponse, TeamCloudCreateProjectOptionalParams, TeamCloudCreateProjectResponse, TeamCloudGetProjectResponse, TeamCloudDeleteProjectResponse, TeamCloudGetProjectComponentsResponse, TeamCloudCreateProjectComponentOptionalParams, TeamCloudCreateProjectComponentResponse, TeamCloudGetProjectComponentResponse, TeamCloudDeleteProjectComponentResponse, TeamCloudResetProjectComponentResponse, TeamCloudClearProjectComponentResponse, TeamCloudGetProjectComponentTemplatesResponse, TeamCloudGetProjectComponentTemplateResponse, TeamCloudGetProjectDeploymentsResponse, TeamCloudGetProjectDeploymentResponse, TeamCloudGetProjectTagsResponse, TeamCloudCreateProjectTagOptionalParams, TeamCloudCreateProjectTagResponse, TeamCloudUpdateProjectTagOptionalParams, TeamCloudUpdateProjectTagResponse, TeamCloudGetProjectTagByKeyResponse, TeamCloudDeleteProjectTagResponse, TeamCloudGetProjectTemplatesResponse, TeamCloudCreateProjectTemplateOptionalParams, TeamCloudCreateProjectTemplateResponse, TeamCloudGetProjectTemplateResponse, TeamCloudUpdateProjectTemplateOptionalParams, TeamCloudUpdateProjectTemplateResponse, TeamCloudDeleteProjectTemplateResponse, TeamCloudGetProjectUsersResponse, TeamCloudCreateProjectUserOptionalParams, TeamCloudCreateProjectUserResponse, TeamCloudGetProjectUserResponse, TeamCloudUpdateProjectUserOptionalParams, TeamCloudUpdateProjectUserResponse, TeamCloudDeleteProjectUserResponse, TeamCloudGetProjectUserMeResponse, TeamCloudUpdateProjectUserMeOptionalParams, TeamCloudUpdateProjectUserMeResponse, TeamCloudGetStatusResponse, TeamCloudGetProjectStatusResponse, TeamCloudGetUserProjectsResponse, TeamCloudGetUserProjectsMeResponse } from "./models";
export declare class TeamCloud extends TeamCloudContext {
    /**
     * Initializes a new instance of the TeamCloud class.
     * @param credentials Subscription credentials which uniquely identify client subscription.
     * @param $host server parameter
     * @param options The parameter options
     */
    constructor(credentials: coreHttp.TokenCredential | coreHttp.ServiceClientCredentials, $host: string, options?: TeamCloudOptionalParams);
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
     * @param id
     * @param organizationId
     * @param options The options parameters.
     */
    getDeploymentScope(id: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetDeploymentScopeResponse>;
    /**
     * Updates an existing Deployment Scope.
     * @param id
     * @param organizationId
     * @param options The options parameters.
     */
    updateDeploymentScope(id: string | null, organizationId: string, options?: TeamCloudUpdateDeploymentScopeOptionalParams): Promise<TeamCloudUpdateDeploymentScopeResponse>;
    /**
     * Deletes a Deployment Scope.
     * @param id
     * @param organizationId
     * @param options The options parameters.
     */
    deleteDeploymentScope(id: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteDeploymentScopeResponse>;
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
     * @param org
     * @param organizationId
     * @param options The options parameters.
     */
    getOrganization(org: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationResponse>;
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
    getProject(projectId: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectResponse>;
    /**
     * Deletes a Project.
     * @param projectId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteProject(projectId: string | null, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectResponse>;
    /**
     * Gets all Components for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponents(organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentsResponse>;
    /**
     * Creates a new Project Component.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectComponent(organizationId: string, projectId: string | null, options?: TeamCloudCreateProjectComponentOptionalParams): Promise<TeamCloudCreateProjectComponentResponse>;
    /**
     * Gets a Project Component.
     * @param id
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponent(id: string | null, organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentResponse>;
    /**
     * Deletes an existing Project Component.
     * @param id
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectComponent(id: string | null, organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectComponentResponse>;
    /**
     * Reset a Project Component.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    resetProjectComponent(organizationId: string, projectId: string | null, componentId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudResetProjectComponentResponse>;
    /**
     * Clear a Project Component.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    clearProjectComponent(organizationId: string, projectId: string | null, componentId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudClearProjectComponentResponse>;
    /**
     * Gets all Project Component Templates.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponentTemplates(organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentTemplatesResponse>;
    /**
     * Gets the Component Template.
     * @param id
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponentTemplate(id: string | null, organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentTemplateResponse>;
    /**
     * Gets all Project Component Deployments.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    getProjectDeployments(organizationId: string, projectId: string | null, componentId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectDeploymentsResponse>;
    /**
     * Gets the Component Template.
     * @param id
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    getProjectDeployment(id: string | null, organizationId: string, projectId: string | null, componentId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectDeploymentResponse>;
    /**
     * Gets all Tags for a Project.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTags(organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTagsResponse>;
    /**
     * Creates a new Project Tag.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectTag(organizationId: string, projectId: string | null, options?: TeamCloudCreateProjectTagOptionalParams): Promise<TeamCloudCreateProjectTagResponse>;
    /**
     * Updates an existing Project Tag.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectTag(organizationId: string, projectId: string | null, options?: TeamCloudUpdateProjectTagOptionalParams): Promise<TeamCloudUpdateProjectTagResponse>;
    /**
     * Gets a Project Tag by Key.
     * @param tagKey
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTagByKey(tagKey: string | null, organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTagByKeyResponse>;
    /**
     * Deletes an existing Project Tag.
     * @param tagKey
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectTag(tagKey: string | null, organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectTagResponse>;
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
    getProjectUsers(organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUsersResponse>;
    /**
     * Creates a new Project User
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    createProjectUser(organizationId: string, projectId: string | null, options?: TeamCloudCreateProjectUserOptionalParams): Promise<TeamCloudCreateProjectUserResponse>;
    /**
     * Gets a Project User by ID or email address.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUser(userId: string | null, organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUserResponse>;
    /**
     * Updates an existing Project User.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUser(userId: string | null, organizationId: string, projectId: string | null, options?: TeamCloudUpdateProjectUserOptionalParams): Promise<TeamCloudUpdateProjectUserResponse>;
    /**
     * Deletes an existing Project User.
     * @param userId
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectUser(userId: string | null, organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectUserResponse>;
    /**
     * Gets a Project User for the calling user.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUserMe(organizationId: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUserMeResponse>;
    /**
     * Updates an existing Project User.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUserMe(organizationId: string, projectId: string | null, options?: TeamCloudUpdateProjectUserMeOptionalParams): Promise<TeamCloudUpdateProjectUserMeResponse>;
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
    getProjectStatus(projectId: string | null, trackingId: string, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectStatusResponse>;
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