import * as coreHttp from '@azure/core-http';

export declare interface Component {
    href?: string | null;
    organization: string;
    templateId: string;
    projectId: string;
    providerId?: string | null;
    requestedBy: string;
    displayName?: string | null;
    description?: string | null;
    inputJson?: string | null;
    valueJson?: string | null;
    scope: ComponentScope;
    type: ComponentType;
    id: string;
}

export declare interface ComponentDataResult {
    code?: number;
    status?: string | null;
    data?: Component;
    location?: string | null;
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

export declare interface ComponentRequest {
    templateId: string;
    inputJson?: string | null;
}

/**
 * Defines values for ComponentScope.
 */
export declare type ComponentScope = "System" | "Project" | "All" | string;

export declare interface ComponentTemplate {
    organization: string;
    parentId: string;
    providerId?: string | null;
    displayName?: string | null;
    description?: string | null;
    repository: RepositoryReference;
    inputJsonSchema?: string | null;
    scope: ComponentTemplateScope;
    type: ComponentTemplateType;
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
 * Defines values for ComponentTemplateScope.
 */
export declare type ComponentTemplateScope = "System" | "Project" | "All" | string;

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
    managementGroupId: string;
    isDefault: boolean;
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
    managementGroupId: string;
    isDefault?: boolean;
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
    /**
     * Dictionary of <string>
     */
    tags?: {
        [propertyName: string]: string;
    } | null;
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
export declare type ProjectMembershipRole = "None" | "Provider" | "Member" | "Owner" | string;

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
     * Gets all Deployment Scopes.
     * @param org
     * @param options The options parameters.
     */
    getDeploymentScopes(org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetDeploymentScopesResponse>;
    /**
     * Creates a new Deployment Scope.
     * @param org
     * @param options The options parameters.
     */
    createDeploymentScope(org: string, options?: TeamCloudCreateDeploymentScopeOptionalParams): Promise<TeamCloudCreateDeploymentScopeResponse>;
    /**
     * Gets a Deployment Scope.
     * @param id
     * @param org
     * @param options The options parameters.
     */
    getDeploymentScope(id: string | null, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetDeploymentScopeResponse>;
    /**
     * Updates an existing Deployment Scope.
     * @param id
     * @param org
     * @param options The options parameters.
     */
    updateDeploymentScope(id: string | null, org: string, options?: TeamCloudUpdateDeploymentScopeOptionalParams): Promise<TeamCloudUpdateDeploymentScopeResponse>;
    /**
     * Deletes a Deployment Scope.
     * @param id
     * @param org
     * @param options The options parameters.
     */
    deleteDeploymentScope(id: string | null, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteDeploymentScopeResponse>;
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
     * @param options The options parameters.
     */
    getOrganization(org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationResponse>;
    /**
     * Deletes an existing Organization.
     * @param org
     * @param options The options parameters.
     */
    deleteOrganization(org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteOrganizationResponse>;
    /**
     * Gets all Users.
     * @param org
     * @param options The options parameters.
     */
    getOrganizationUsers(org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationUsersResponse>;
    /**
     * Creates a new User.
     * @param org
     * @param options The options parameters.
     */
    createOrganizationUser(org: string, options?: TeamCloudCreateOrganizationUserOptionalParams): Promise<TeamCloudCreateOrganizationUserResponse>;
    /**
     * Gets a User.
     * @param userId
     * @param org
     * @param options The options parameters.
     */
    getOrganizationUser(userId: string | null, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationUserResponse>;
    /**
     * Updates an existing User.
     * @param userId
     * @param org
     * @param options The options parameters.
     */
    updateOrganizationUser(userId: string | null, org: string, options?: TeamCloudUpdateOrganizationUserOptionalParams): Promise<TeamCloudUpdateOrganizationUserResponse>;
    /**
     * Deletes an existing User.
     * @param userId
     * @param org
     * @param options The options parameters.
     */
    deleteOrganizationUser(userId: string | null, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteOrganizationUserResponse>;
    /**
     * Gets a User A User matching the current authenticated user.
     * @param org
     * @param options The options parameters.
     */
    getOrganizationUserMe(org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetOrganizationUserMeResponse>;
    /**
     * Updates an existing User.
     * @param org
     * @param options The options parameters.
     */
    updateOrganizationUserMe(org: string, options?: TeamCloudUpdateOrganizationUserMeOptionalParams): Promise<TeamCloudUpdateOrganizationUserMeResponse>;
    /**
     * Gets all Projects.
     * @param org
     * @param options The options parameters.
     */
    getProjects(org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectsResponse>;
    /**
     * Creates a new Project.
     * @param org
     * @param options The options parameters.
     */
    createProject(org: string, options?: TeamCloudCreateProjectOptionalParams): Promise<TeamCloudCreateProjectResponse>;
    /**
     * Gets a Project.
     * @param projectId
     * @param org
     * @param options The options parameters.
     */
    getProject(projectId: string | null, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectResponse>;
    /**
     * Deletes a Project.
     * @param projectId
     * @param org
     * @param options The options parameters.
     */
    deleteProject(projectId: string | null, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectResponse>;
    /**
     * Gets all Components for a Project.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponents(org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentsResponse>;
    /**
     * Creates a new Project Component.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    createProjectComponent(org: string, projectId: string | null, options?: TeamCloudCreateProjectComponentOptionalParams): Promise<TeamCloudCreateProjectComponentResponse>;
    /**
     * Gets a Project Component.
     * @param id
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponent(id: string | null, org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentResponse>;
    /**
     * Deletes an existing Project Component.
     * @param id
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectComponent(id: string | null, org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectComponentResponse>;
    /**
     * Gets all Project Component Templates.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponentTemplates(org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentTemplatesResponse>;
    /**
     * Gets the Component Template.
     * @param id
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponentTemplate(id: string | null, org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentTemplateResponse>;
    /**
     * Gets all Tags for a Project.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTags(org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTagsResponse>;
    /**
     * Creates a new Project Tag.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    createProjectTag(org: string, projectId: string | null, options?: TeamCloudCreateProjectTagOptionalParams): Promise<TeamCloudCreateProjectTagResponse>;
    /**
     * Updates an existing Project Tag.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectTag(org: string, projectId: string | null, options?: TeamCloudUpdateProjectTagOptionalParams): Promise<TeamCloudUpdateProjectTagResponse>;
    /**
     * Gets a Project Tag by Key.
     * @param tagKey
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTagByKey(tagKey: string | null, org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTagByKeyResponse>;
    /**
     * Deletes an existing Project Tag.
     * @param tagKey
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectTag(tagKey: string | null, org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectTagResponse>;
    /**
     * Gets all Project Templates.
     * @param org
     * @param options The options parameters.
     */
    getProjectTemplates(org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTemplatesResponse>;
    /**
     * Creates a new Project Template.
     * @param org
     * @param options The options parameters.
     */
    createProjectTemplate(org: string, options?: TeamCloudCreateProjectTemplateOptionalParams): Promise<TeamCloudCreateProjectTemplateResponse>;
    /**
     * Gets a Project Template.
     * @param projectTemplateId
     * @param org
     * @param options The options parameters.
     */
    getProjectTemplate(projectTemplateId: string | null, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTemplateResponse>;
    /**
     * Updates an existing Project Template.
     * @param projectTemplateId
     * @param org
     * @param options The options parameters.
     */
    updateProjectTemplate(projectTemplateId: string | null, org: string, options?: TeamCloudUpdateProjectTemplateOptionalParams): Promise<TeamCloudUpdateProjectTemplateResponse>;
    /**
     * Deletes a Project Template.
     * @param projectTemplateId
     * @param org
     * @param options The options parameters.
     */
    deleteProjectTemplate(projectTemplateId: string | null, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectTemplateResponse>;
    /**
     * Gets all Users for a Project.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUsers(org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUsersResponse>;
    /**
     * Creates a new Project User
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    createProjectUser(org: string, projectId: string | null, options?: TeamCloudCreateProjectUserOptionalParams): Promise<TeamCloudCreateProjectUserResponse>;
    /**
     * Gets a Project User by ID or email address.
     * @param userId
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUser(userId: string | null, org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUserResponse>;
    /**
     * Updates an existing Project User.
     * @param userId
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUser(userId: string | null, org: string, projectId: string | null, options?: TeamCloudUpdateProjectUserOptionalParams): Promise<TeamCloudUpdateProjectUserResponse>;
    /**
     * Deletes an existing Project User.
     * @param userId
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectUser(userId: string | null, org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectUserResponse>;
    /**
     * Gets a Project User for the calling user.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUserMe(org: string, projectId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUserMeResponse>;
    /**
     * Updates an existing Project User.
     * @param org
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUserMe(org: string, projectId: string | null, options?: TeamCloudUpdateProjectUserMeOptionalParams): Promise<TeamCloudUpdateProjectUserMeResponse>;
    /**
     * Gets the status of a long-running operation.
     * @param trackingId
     * @param org
     * @param options The options parameters.
     */
    getStatus(trackingId: string, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetStatusResponse>;
    /**
     * Gets the status of a long-running operation.
     * @param projectId
     * @param trackingId
     * @param org
     * @param options The options parameters.
     */
    getProjectStatus(projectId: string | null, trackingId: string, org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectStatusResponse>;
    /**
     * Gets all Projects for a User.
     * @param org
     * @param userId
     * @param options The options parameters.
     */
    getUserProjects(org: string, userId: string | null, options?: coreHttp.OperationOptions): Promise<TeamCloudGetUserProjectsResponse>;
    /**
     * Gets all Projects for a User.
     * @param org
     * @param options The options parameters.
     */
    getUserProjectsMe(org: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetUserProjectsMeResponse>;
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
export declare interface TeamCloudCreateProjectComponentOptionalParams extends coreHttp.OperationOptions {
    body?: ComponentRequest;
}

/**
 * Contains response data for the createProjectComponent operation.
 */
export declare type TeamCloudCreateProjectComponentResponse = ComponentDataResult & {
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
 * Contains response data for the deleteProjectComponent operation.
 */
export declare type TeamCloudDeleteProjectComponentResponse = StatusResult & {
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
 * Contains response data for the getProjectComponent operation.
 */
export declare type TeamCloudGetProjectComponentResponse = ComponentDataResult & {
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
 * Contains response data for the getProjectComponents operation.
 */
export declare type TeamCloudGetProjectComponentsResponse = ComponentListDataResult & {
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
 * Contains response data for the getProjectComponentTemplate operation.
 */
export declare type TeamCloudGetProjectComponentTemplateResponse = ComponentTemplateDataResult & {
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
 * Contains response data for the getProjectComponentTemplates operation.
 */
export declare type TeamCloudGetProjectComponentTemplatesResponse = ComponentTemplateListDataResult & {
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
export declare type UserRole = "None" | "Provider" | "Creator" | "Admin" | string;

/**
 * Defines values for UserType.
 */
export declare type UserType = "User" | "System" | "Provider" | "Application" | string;

export declare interface ValidationError {
    field?: string | null;
    message?: string | null;
}

export { }
