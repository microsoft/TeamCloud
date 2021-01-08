import * as coreHttp from '@azure/core-http';

export declare interface Component {
    href?: string | null;
    organization: string;
    templateId: string;
    projectId: string;
    provider: string;
    creator: string;
    displayName?: string | null;
    description?: string | null;
    inputJson?: string | null;
    valueJson?: string | null;
    type: ComponentType;
    resourceId?: string | null;
    resourceState?: ComponentResourceState;
    deploymentScopeId?: string | null;
    identityId?: string | null;
    storageId?: string | null;
    deleted?: Date | null;
    ttl?: number | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly slug: string;
    id: string;
}

export declare interface ComponentDataResult {
    code?: number;
    status?: string | null;
    data?: Component;
    location?: string | null;
}

export declare interface ComponentDefinition {
    templateId: string;
    displayName: string;
    inputJson?: string | null;
    deploymentScopeId?: string | null;
}

export declare interface ComponentListDataResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: Component[] | null;
    location?: string | null;
}

/**
 * Defines values for ComponentResourceState.
 */
export declare type ComponentResourceState = "Pending" | "Initializing" | "Provisioning" | "Succeeded" | "Failed" | string;

export declare interface ComponentTask {
    organization: string;
    componentId: string;
    projectId: string;
    storageId?: string | null;
    requestedBy?: string | null;
    type?: Enum3;
    typeName?: string | null;
    created?: Date;
    started?: Date | null;
    finished?: Date | null;
    inputJson?: string | null;
    output?: string | null;
    resourceId?: string | null;
    resourceState?: ComponentTaskResourceState;
    exitCode?: number | null;
    id: string;
}

export declare interface ComponentTaskDataResult {
    code?: number;
    status?: string | null;
    data?: ComponentTask;
    location?: string | null;
}

export declare interface ComponentTaskDefinition {
    taskId: string;
    inputJson?: string | null;
}

export declare interface ComponentTaskListDataResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: ComponentTask[] | null;
    location?: string | null;
}

/**
 * Defines values for ComponentTaskResourceState.
 */
export declare type ComponentTaskResourceState = "Pending" | "Initializing" | "Provisioning" | "Succeeded" | "Failed" | string;

export declare interface ComponentTaskTemplate {
    id?: string | null;
    displayName?: string | null;
    description?: string | null;
    inputJsonSchema?: string | null;
    type: Enum3;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly typeName?: string | null;
}

export declare interface ComponentTemplate {
    organization: string;
    parentId: string;
    provider?: string | null;
    displayName?: string | null;
    description?: string | null;
    repository: RepositoryReference;
    inputJsonSchema?: string | null;
    tasks?: ComponentTaskTemplate[] | null;
    type: ComponentTemplateType;
    folder?: string | null;
    id: string;
}

export declare interface ComponentTemplateDataResult {
    code?: number;
    status?: string | null;
    data?: ComponentTemplate;
    location?: string | null;
}

export declare interface ComponentTemplateListDataResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: ComponentTemplate[] | null;
    location?: string | null;
}

/**
 * Defines values for ComponentTemplateType.
 */
export declare type ComponentTemplateType = "Custom" | "AzureResource" | "Environment" | "GitRepository" | string;

/**
 * Defines values for ComponentType.
 */
export declare type ComponentType = "Custom" | "AzureResource" | "Environment" | "GitRepository" | string;

export declare interface DeploymentScope {
    organization: string;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly slug: string;
    displayName: string;
    isDefault: boolean;
    managementGroupId?: string | null;
    subscriptionIds?: string[] | null;
    id: string;
}

export declare interface DeploymentScopeDataResult {
    code?: number;
    status?: string | null;
    data?: DeploymentScope;
    location?: string | null;
}

export declare interface DeploymentScopeDefinition {
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly slug?: string | null;
    displayName: string;
    isDefault?: boolean;
    managementGroupId?: string | null;
    subscriptionIds?: string[] | null;
}

export declare interface DeploymentScopeListDataResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: DeploymentScope[] | null;
    location?: string | null;
}

/**
 * Defines values for Enum3.
 */
export declare type Enum3 = 0 | 1 | 2 | number;

export declare interface ErrorResult {
    code?: number;
    status?: string | null;
    errors?: ResultError[] | null;
}

export declare interface Organization {
    tenant: string;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly slug: string;
    displayName: string;
    subscriptionId: string;
    location: string;
    /**
     * Dictionary of <string>
     */
    tags?: {
        [propertyName: string]: string;
    } | null;
    resourceId?: string | null;
    resourceState?: OrganizationResourceState;
    id: string;
}

export declare interface OrganizationDataResult {
    code?: number;
    status?: string | null;
    data?: Organization;
    location?: string | null;
}

export declare interface OrganizationDefinition {
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly slug?: string | null;
    displayName: string;
    subscriptionId: string;
    location: string;
}

export declare interface OrganizationListDataResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: Organization[] | null;
    location?: string | null;
}

/**
 * Defines values for OrganizationResourceState.
 */
export declare type OrganizationResourceState = "Pending" | "Initializing" | "Provisioning" | "Succeeded" | "Failed" | string;

export declare interface Project {
    organization: string;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly slug: string;
    displayName: string;
    template: string;
    templateInput?: string | null;
    users?: User[] | null;
    /**
     * Dictionary of <string>
     */
    tags?: {
        [propertyName: string]: string;
    } | null;
    resourceId?: string | null;
    resourceState?: ProjectResourceState;
    id: string;
}

export declare interface ProjectDataResult {
    code?: number;
    status?: string | null;
    data?: Project;
    location?: string | null;
}

export declare interface ProjectDefinition {
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly slug?: string | null;
    displayName: string;
    template: string;
    templateInput: string;
    users?: UserDefinition[] | null;
}

export declare interface ProjectListDataResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: Project[] | null;
    location?: string | null;
}

export declare interface ProjectMembership {
    projectId: string;
    role: ProjectMembershipRole;
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    } | null;
}

/**
 * Defines values for ProjectMembershipRole.
 */
export declare type ProjectMembershipRole = "None" | "Member" | "Admin" | "Owner" | string;

/**
 * Defines values for ProjectResourceState.
 */
export declare type ProjectResourceState = "Pending" | "Initializing" | "Provisioning" | "Succeeded" | "Failed" | string;

export declare interface ProjectTemplate {
    organization: string;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly slug: string;
    name?: string | null;
    displayName: string;
    components?: string[] | null;
    repository: RepositoryReference;
    description?: string | null;
    isDefault: boolean;
    inputJsonSchema?: string | null;
    id: string;
}

export declare interface ProjectTemplateDataResult {
    code?: number;
    status?: string | null;
    data?: ProjectTemplate;
    location?: string | null;
}

export declare interface ProjectTemplateDefinition {
    displayName: string;
    repository: RepositoryDefinition;
}

export declare interface ProjectTemplateListDataResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: ProjectTemplate[] | null;
    location?: string | null;
}

export declare interface RepositoryDefinition {
    url: string;
    token?: string | null;
    version?: string | null;
}

export declare interface RepositoryReference {
    url: string;
    token?: string | null;
    version?: string | null;
    baselUrl?: string | null;
    mountUrl?: string | null;
    ref?: string | null;
    provider: RepositoryReferenceProvider;
    type: RepositoryReferenceType;
    organization?: string | null;
    repository?: string | null;
    project?: string | null;
}

/**
 * Defines values for RepositoryReferenceProvider.
 */
export declare type RepositoryReferenceProvider = "Unknown" | "GitHub" | "DevOps" | string;

/**
 * Defines values for RepositoryReferenceType.
 */
export declare type RepositoryReferenceType = "Unknown" | "Tag" | "Branch" | "Hash" | string;

export declare interface ResultError {
    code?: ResultErrorCode;
    message?: string | null;
    errors?: ValidationError[] | null;
}

/**
 * Defines values for ResultErrorCode.
 */
export declare type ResultErrorCode = "Unknown" | "Failed" | "Conflict" | "NotFound" | "ServerError" | "ValidationError" | "Unauthorized" | "Forbidden" | string;

export declare interface StatusResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly state?: string | null;
    stateMessage?: string | null;
    location?: string | null;
    errors?: ResultError[] | null;
    trackingId?: string | null;
}

export declare interface StringDictionaryDataResult {
    code?: number;
    status?: string | null;
    /**
     * Dictionary of <string>
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: {
        [propertyName: string]: string;
    } | null;
    location?: string | null;
}

export declare class TeamCloud extends TeamCloudContext {
    /**
     * Initializes a new instance of the TeamCloud class.
     * @param credentials Subscription credentials which uniquely identify client subscription.
     * @param $host server parameter
     * @param options The parameter options
     */
    constructor(credentials: coreHttp.TokenCredential | coreHttp.ServiceClientCredentials, $host: string, options?: TeamCloudOptionalParams);
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
     * @param id
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    getComponent(id: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetComponentResponse>;
    /**
     * Deletes an existing Project Component.
     * @param id
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    deleteComponent(id: string | null, organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteComponentResponse>;
    /**
     * Gets all Component Tasks.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    getComponentTasks(organizationId: string, projectId: string, componentId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetComponentTasksResponse>;
    /**
     * Creates a new Project Component Task.
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    createComponentTask(organizationId: string, projectId: string, componentId: string, options?: TeamCloudCreateComponentTaskOptionalParams): Promise<TeamCloudCreateComponentTaskResponse>;
    /**
     * Gets the Component Task.
     * @param id
     * @param organizationId
     * @param projectId
     * @param componentId
     * @param options The options parameters.
     */
    getComponentTask(id: string | null, organizationId: string, projectId: string, componentId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetComponentTaskResponse>;
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
    getProject(projectId: string, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectResponse>;
    /**
     * Deletes a Project.
     * @param projectId
     * @param organizationId
     * @param options The options parameters.
     */
    deleteProject(projectId: string, organizationId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectResponse>;
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

export declare class TeamCloudContext extends coreHttp.ServiceClient {
    $host: string;
    /**
     * Initializes a new instance of the TeamCloudContext class.
     * @param credentials Subscription credentials which uniquely identify client subscription.
     * @param $host server parameter
     * @param options The parameter options
     */
    constructor(credentials: coreHttp.TokenCredential | coreHttp.ServiceClientCredentials, $host: string, options?: TeamCloudOptionalParams);
}

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateComponentOptionalParams extends coreHttp.OperationOptions {
    body?: ComponentDefinition;
}

/**
 * Contains response data for the createComponent operation.
 */
export declare type TeamCloudCreateComponentResponse = ComponentDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ComponentDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateComponentTaskOptionalParams extends coreHttp.OperationOptions {
    body?: ComponentTaskDefinition;
}

/**
 * Contains response data for the createComponentTask operation.
 */
export declare type TeamCloudCreateComponentTaskResponse = ComponentTaskDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ComponentTaskDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateDeploymentScopeOptionalParams extends coreHttp.OperationOptions {
    body?: DeploymentScopeDefinition;
}

/**
 * Contains response data for the createDeploymentScope operation.
 */
export declare type TeamCloudCreateDeploymentScopeResponse = DeploymentScopeDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: DeploymentScopeDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateOrganizationOptionalParams extends coreHttp.OperationOptions {
    body?: OrganizationDefinition;
}

/**
 * Contains response data for the createOrganization operation.
 */
export declare type TeamCloudCreateOrganizationResponse = OrganizationDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: OrganizationDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateOrganizationUserOptionalParams extends coreHttp.OperationOptions {
    body?: UserDefinition;
}

/**
 * Contains response data for the createOrganizationUser operation.
 */
export declare type TeamCloudCreateOrganizationUserResponse = UserDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateProjectOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectDefinition;
}

/**
 * Contains response data for the createProject operation.
 */
export declare type TeamCloudCreateProjectResponse = ProjectDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateProjectTagOptionalParams extends coreHttp.OperationOptions {
    /**
     * Dictionary of <string>
     */
    body?: {
        [propertyName: string]: string;
    };
}

/**
 * Contains response data for the createProjectTag operation.
 */
export declare type TeamCloudCreateProjectTagResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateProjectTemplateOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectTemplateDefinition;
}

/**
 * Contains response data for the createProjectTemplate operation.
 */
export declare type TeamCloudCreateProjectTemplateResponse = ProjectTemplateDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectTemplateDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateProjectUserOptionalParams extends coreHttp.OperationOptions {
    body?: UserDefinition;
}

/**
 * Contains response data for the createProjectUser operation.
 */
export declare type TeamCloudCreateProjectUserResponse = UserDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserDataResult;
    };
};

/**
 * Contains response data for the deleteComponent operation.
 */
export declare type TeamCloudDeleteComponentResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Contains response data for the deleteDeploymentScope operation.
 */
export declare type TeamCloudDeleteDeploymentScopeResponse = DeploymentScopeDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: DeploymentScopeDataResult;
    };
};

/**
 * Contains response data for the deleteOrganization operation.
 */
export declare type TeamCloudDeleteOrganizationResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Contains response data for the deleteOrganizationUser operation.
 */
export declare type TeamCloudDeleteOrganizationUserResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Contains response data for the deleteProject operation.
 */
export declare type TeamCloudDeleteProjectResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Contains response data for the deleteProjectTag operation.
 */
export declare type TeamCloudDeleteProjectTagResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Contains response data for the deleteProjectTemplate operation.
 */
export declare type TeamCloudDeleteProjectTemplateResponse = ProjectTemplateDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectTemplateDataResult;
    };
};

/**
 * Contains response data for the deleteProjectUser operation.
 */
export declare type TeamCloudDeleteProjectUserResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Contains response data for the getComponent operation.
 */
export declare type TeamCloudGetComponentResponse = ComponentDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ComponentDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudGetComponentsOptionalParams extends coreHttp.OperationOptions {
    deleted?: boolean;
}

/**
 * Contains response data for the getComponents operation.
 */
export declare type TeamCloudGetComponentsResponse = ComponentListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ComponentListDataResult;
    };
};

/**
 * Contains response data for the getComponentTask operation.
 */
export declare type TeamCloudGetComponentTaskResponse = ComponentTaskDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ComponentTaskDataResult;
    };
};

/**
 * Contains response data for the getComponentTasks operation.
 */
export declare type TeamCloudGetComponentTasksResponse = ComponentTaskListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ComponentTaskListDataResult;
    };
};

/**
 * Contains response data for the getComponentTemplate operation.
 */
export declare type TeamCloudGetComponentTemplateResponse = ComponentTemplateDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ComponentTemplateDataResult;
    };
};

/**
 * Contains response data for the getComponentTemplates operation.
 */
export declare type TeamCloudGetComponentTemplatesResponse = ComponentTemplateListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ComponentTemplateListDataResult;
    };
};

/**
 * Contains response data for the getDeploymentScope operation.
 */
export declare type TeamCloudGetDeploymentScopeResponse = DeploymentScopeDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: DeploymentScopeDataResult;
    };
};

/**
 * Contains response data for the getDeploymentScopes operation.
 */
export declare type TeamCloudGetDeploymentScopesResponse = DeploymentScopeListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: DeploymentScopeListDataResult;
    };
};

/**
 * Contains response data for the getOrganization operation.
 */
export declare type TeamCloudGetOrganizationResponse = OrganizationDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: OrganizationDataResult;
    };
};

/**
 * Contains response data for the getOrganizations operation.
 */
export declare type TeamCloudGetOrganizationsResponse = OrganizationListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: OrganizationListDataResult;
    };
};

/**
 * Contains response data for the getOrganizationUserMe operation.
 */
export declare type TeamCloudGetOrganizationUserMeResponse = UserDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserDataResult;
    };
};

/**
 * Contains response data for the getOrganizationUser operation.
 */
export declare type TeamCloudGetOrganizationUserResponse = UserDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserDataResult;
    };
};

/**
 * Contains response data for the getOrganizationUsers operation.
 */
export declare type TeamCloudGetOrganizationUsersResponse = UserListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserListDataResult;
    };
};

/**
 * Contains response data for the getProject operation.
 */
export declare type TeamCloudGetProjectResponse = ProjectDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectDataResult;
    };
};

/**
 * Contains response data for the getProjects operation.
 */
export declare type TeamCloudGetProjectsResponse = ProjectListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectListDataResult;
    };
};

/**
 * Contains response data for the getProjectStatus operation.
 */
export declare type TeamCloudGetProjectStatusResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Contains response data for the getProjectTagByKey operation.
 */
export declare type TeamCloudGetProjectTagByKeyResponse = StringDictionaryDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StringDictionaryDataResult;
    };
};

/**
 * Contains response data for the getProjectTags operation.
 */
export declare type TeamCloudGetProjectTagsResponse = StringDictionaryDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StringDictionaryDataResult;
    };
};

/**
 * Contains response data for the getProjectTemplate operation.
 */
export declare type TeamCloudGetProjectTemplateResponse = ProjectTemplateDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectTemplateDataResult;
    };
};

/**
 * Contains response data for the getProjectTemplates operation.
 */
export declare type TeamCloudGetProjectTemplatesResponse = ProjectTemplateListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectTemplateListDataResult;
    };
};

/**
 * Contains response data for the getProjectUserMe operation.
 */
export declare type TeamCloudGetProjectUserMeResponse = UserDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserDataResult;
    };
};

/**
 * Contains response data for the getProjectUser operation.
 */
export declare type TeamCloudGetProjectUserResponse = UserDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserDataResult;
    };
};

/**
 * Contains response data for the getProjectUsers operation.
 */
export declare type TeamCloudGetProjectUsersResponse = UserListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserListDataResult;
    };
};

/**
 * Contains response data for the getStatus operation.
 */
export declare type TeamCloudGetStatusResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Contains response data for the getUserProjectsMe operation.
 */
export declare type TeamCloudGetUserProjectsMeResponse = ProjectListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectListDataResult;
    };
};

/**
 * Contains response data for the getUserProjects operation.
 */
export declare type TeamCloudGetUserProjectsResponse = ProjectListDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectListDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudOptionalParams extends coreHttp.ServiceClientOptions {
    /**
     * Overrides client endpoint.
     */
    endpoint?: string;
}

/**
 * Optional parameters.
 */
export declare interface TeamCloudUpdateDeploymentScopeOptionalParams extends coreHttp.OperationOptions {
    body?: DeploymentScope;
}

/**
 * Contains response data for the updateDeploymentScope operation.
 */
export declare type TeamCloudUpdateDeploymentScopeResponse = DeploymentScopeDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: DeploymentScopeDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudUpdateOrganizationUserMeOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}

/**
 * Contains response data for the updateOrganizationUserMe operation.
 */
export declare type TeamCloudUpdateOrganizationUserMeResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudUpdateOrganizationUserOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}

/**
 * Contains response data for the updateOrganizationUser operation.
 */
export declare type TeamCloudUpdateOrganizationUserResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProjectTagOptionalParams extends coreHttp.OperationOptions {
    /**
     * Dictionary of <string>
     */
    body?: {
        [propertyName: string]: string;
    };
}

/**
 * Contains response data for the updateProjectTag operation.
 */
export declare type TeamCloudUpdateProjectTagResponse = StatusResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: StatusResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProjectTemplateOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectTemplate;
}

/**
 * Contains response data for the updateProjectTemplate operation.
 */
export declare type TeamCloudUpdateProjectTemplateResponse = ProjectTemplateDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: ProjectTemplateDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProjectUserMeOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}

/**
 * Contains response data for the updateProjectUserMe operation.
 */
export declare type TeamCloudUpdateProjectUserMeResponse = UserDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserDataResult;
    };
};

/**
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProjectUserOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}

/**
 * Contains response data for the updateProjectUser operation.
 */
export declare type TeamCloudUpdateProjectUserResponse = UserDataResult & {
    /**
     * The underlying HTTP response.
     */
    _response: coreHttp.HttpResponse & {
        /**
         * The response body as text (string format)
         */
        bodyAsText: string;
        /**
         * The response body as parsed JSON or XML
         */
        parsedBody: UserDataResult;
    };
};

export declare interface User {
    organization: string;
    userType: UserType;
    role: UserRole;
    projectMemberships?: ProjectMembership[] | null;
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    } | null;
    id: string;
}

export declare interface UserDataResult {
    code?: number;
    status?: string | null;
    data?: User;
    location?: string | null;
}

export declare interface UserDefinition {
    identifier: string;
    role: string;
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    } | null;
}

export declare interface UserListDataResult {
    code?: number;
    status?: string | null;
    /**
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: User[] | null;
    location?: string | null;
}

/**
 * Defines values for UserRole.
 */
export declare type UserRole = "None" | "Member" | "Admin" | "Owner" | string;

/**
 * Defines values for UserType.
 */
export declare type UserType = "User" | "System" | "Provider" | "Application" | string;

export declare interface ValidationError {
    field?: string | null;
    message?: string | null;
}

export { }
