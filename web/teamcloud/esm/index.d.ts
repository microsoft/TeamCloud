import * as coreHttp from '@azure/core-http';

export declare interface Component {
    href?: string | null;
    organization: string;
    templateId: string;
    projectId: string;
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
    deleted?: Date | null;
    ttl?: number | null;
    slug: string;
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
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Component[] | null;
    location?: string | null;
}

/**
 * Defines values for ComponentResourceState. \
 * {@link KnownComponentResourceState} can be used interchangeably with ComponentResourceState,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Pending** \
 * **Initializing** \
 * **Provisioning** \
 * **Succeeded** \
 * **Failed**
 */
export declare type ComponentResourceState = string;

export declare interface ComponentTask {
    organization: string;
    componentId: string;
    projectId: string;
    requestedBy?: string | null;
    type?: ComponentTaskType;
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
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ComponentTask[] | null;
    location?: string | null;
}

export declare interface ComponentTaskReference {
    componentId?: string | null;
    componentTaskTemplateId?: string | null;
    inputJson?: string | null;
}

/**
 * Defines values for ComponentTaskResourceState. \
 * {@link KnownComponentTaskResourceState} can be used interchangeably with ComponentTaskResourceState,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Pending** \
 * **Initializing** \
 * **Provisioning** \
 * **Succeeded** \
 * **Failed**
 */
export declare type ComponentTaskResourceState = string;

export declare interface ComponentTaskRunner {
    id?: string | null;
    /** Dictionary of <string> */
    with?: {
        [propertyName: string]: string;
    } | null;
}

export declare interface ComponentTaskTemplate {
    id?: string | null;
    displayName?: string | null;
    description?: string | null;
    inputJsonSchema?: string | null;
    type?: ComponentTaskTemplateType;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly typeName?: string | null;
}

/**
 * Defines values for ComponentTaskTemplateType. \
 * {@link KnownComponentTaskTemplateType} can be used interchangeably with ComponentTaskTemplateType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Custom** \
 * **Create** \
 * **Delete**
 */
export declare type ComponentTaskTemplateType = string;

/**
 * Defines values for ComponentTaskType. \
 * {@link KnownComponentTaskType} can be used interchangeably with ComponentTaskType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Custom** \
 * **Create** \
 * **Delete**
 */
export declare type ComponentTaskType = string;

export declare interface ComponentTemplate {
    organization: string;
    parentId: string;
    displayName?: string | null;
    description?: string | null;
    repository: RepositoryReference;
    permissions?: ComponentTemplatePermissions | null;
    inputJsonSchema?: string | null;
    tasks?: ComponentTaskTemplate[] | null;
    taskRunner?: ComponentTaskRunner;
    type: ComponentTemplateType;
    folder?: string | null;
    /** Any object */
    configuration?: any | null;
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
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ComponentTemplate[] | null;
    location?: string | null;
}

export declare interface ComponentTemplatePermissions {
    none?: string[];
    member?: string[];
    admin?: string[];
    owner?: string[];
}

/**
 * Defines values for ComponentTemplateType. \
 * {@link KnownComponentTemplateType} can be used interchangeably with ComponentTemplateType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Environment** \
 * **Repository**
 */
export declare type ComponentTemplateType = string;

/**
 * Defines values for ComponentType. \
 * {@link KnownComponentType} can be used interchangeably with ComponentType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Environment** \
 * **Repository**
 */
export declare type ComponentType = string;

export declare interface DeploymentScope {
    organization: string;
    displayName: string;
    slug: string;
    isDefault: boolean;
    type: DeploymentScopeType;
    managementGroupId?: string | null;
    subscriptionIds?: string[] | null;
    authorizable?: boolean;
    authorized?: boolean;
    authorizeUrl?: string | null;
    id: string;
}

export declare interface DeploymentScopeDataResult {
    code?: number;
    status?: string | null;
    data?: DeploymentScope;
    location?: string | null;
}

export declare interface DeploymentScopeDefinition {
    displayName: string;
    type: DeploymentScopeDefinitionType;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string | null;
    isDefault?: boolean;
    managementGroupId?: string | null;
    subscriptionIds?: string[] | null;
}

/**
 * Defines values for DeploymentScopeDefinitionType. \
 * {@link KnownDeploymentScopeDefinitionType} can be used interchangeably with DeploymentScopeDefinitionType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub**
 */
export declare type DeploymentScopeDefinitionType = string;

export declare interface DeploymentScopeListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: DeploymentScope[] | null;
    location?: string | null;
}

/**
 * Defines values for DeploymentScopeType. \
 * {@link KnownDeploymentScopeType} can be used interchangeably with DeploymentScopeType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub**
 */
export declare type DeploymentScopeType = string;

export declare interface ErrorResult {
    code?: number;
    status?: string | null;
    errors?: ResultError[] | null;
}

/** Known values of {@link ComponentResourceState} that the service accepts. */
export declare const enum KnownComponentResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Succeeded = "Succeeded",
    Failed = "Failed"
}

/** Known values of {@link ComponentTaskResourceState} that the service accepts. */
export declare const enum KnownComponentTaskResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Succeeded = "Succeeded",
    Failed = "Failed"
}

/** Known values of {@link ComponentTaskTemplateType} that the service accepts. */
export declare const enum KnownComponentTaskTemplateType {
    Custom = "Custom",
    Create = "Create",
    Delete = "Delete"
}

/** Known values of {@link ComponentTaskType} that the service accepts. */
export declare const enum KnownComponentTaskType {
    Custom = "Custom",
    Create = "Create",
    Delete = "Delete"
}

/** Known values of {@link ComponentTemplateType} that the service accepts. */
export declare const enum KnownComponentTemplateType {
    Environment = "Environment",
    Repository = "Repository"
}

/** Known values of {@link ComponentType} that the service accepts. */
export declare const enum KnownComponentType {
    Environment = "Environment",
    Repository = "Repository"
}

/** Known values of {@link DeploymentScopeDefinitionType} that the service accepts. */
export declare const enum KnownDeploymentScopeDefinitionType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub"
}

/** Known values of {@link DeploymentScopeType} that the service accepts. */
export declare const enum KnownDeploymentScopeType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub"
}

/** Known values of {@link OrganizationResourceState} that the service accepts. */
export declare const enum KnownOrganizationResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Succeeded = "Succeeded",
    Failed = "Failed"
}

/** Known values of {@link ProjectMembershipRole} that the service accepts. */
export declare const enum KnownProjectMembershipRole {
    None = "None",
    Member = "Member",
    Admin = "Admin",
    Owner = "Owner"
}

/** Known values of {@link ProjectResourceState} that the service accepts. */
export declare const enum KnownProjectResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Succeeded = "Succeeded",
    Failed = "Failed"
}

/** Known values of {@link RepositoryReferenceProvider} that the service accepts. */
export declare const enum KnownRepositoryReferenceProvider {
    Unknown = "Unknown",
    GitHub = "GitHub",
    DevOps = "DevOps"
}

/** Known values of {@link RepositoryReferenceType} that the service accepts. */
export declare const enum KnownRepositoryReferenceType {
    Unknown = "Unknown",
    Tag = "Tag",
    Branch = "Branch",
    Hash = "Hash"
}

/** Known values of {@link ResultErrorCode} that the service accepts. */
export declare const enum KnownResultErrorCode {
    Unknown = "Unknown",
    Failed = "Failed",
    Conflict = "Conflict",
    NotFound = "NotFound",
    ServerError = "ServerError",
    ValidationError = "ValidationError",
    Unauthorized = "Unauthorized",
    Forbidden = "Forbidden"
}

/** Known values of {@link ScheduleDaysOfWeekItem} that the service accepts. */
export declare const enum KnownScheduleDaysOfWeekItem {
    Sunday = "Sunday",
    Monday = "Monday",
    Tuesday = "Tuesday",
    Wednesday = "Wednesday",
    Thursday = "Thursday",
    Friday = "Friday",
    Saturday = "Saturday"
}

/** Known values of {@link ScheduleDefinitionDaysOfWeekItem} that the service accepts. */
export declare const enum KnownScheduleDefinitionDaysOfWeekItem {
    Sunday = "Sunday",
    Monday = "Monday",
    Tuesday = "Tuesday",
    Wednesday = "Wednesday",
    Thursday = "Thursday",
    Friday = "Friday",
    Saturday = "Saturday"
}

/** Known values of {@link UserRole} that the service accepts. */
export declare const enum KnownUserRole {
    None = "None",
    Member = "Member",
    Admin = "Admin",
    Owner = "Owner"
}

/** Known values of {@link UserType} that the service accepts. */
export declare const enum KnownUserType {
    User = "User",
    System = "System",
    Provider = "Provider",
    Application = "Application"
}

export declare interface Organization {
    tenant: string;
    slug: string;
    displayName: string;
    subscriptionId: string;
    location: string;
    /** Dictionary of <string> */
    tags?: {
        [propertyName: string]: string;
    } | null;
    resourceId?: string | null;
    resourceState?: OrganizationResourceState;
    galleryId?: string | null;
    registryId?: string | null;
    storageId?: string | null;
    id: string;
}

export declare interface OrganizationDataResult {
    code?: number;
    status?: string | null;
    data?: Organization;
    location?: string | null;
}

export declare interface OrganizationDefinition {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string | null;
    displayName: string;
    subscriptionId: string;
    location: string;
}

export declare interface OrganizationListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Organization[] | null;
    location?: string | null;
}

/**
 * Defines values for OrganizationResourceState. \
 * {@link KnownOrganizationResourceState} can be used interchangeably with OrganizationResourceState,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Pending** \
 * **Initializing** \
 * **Provisioning** \
 * **Succeeded** \
 * **Failed**
 */
export declare type OrganizationResourceState = string;

export declare interface Project {
    organization: string;
    slug: string;
    displayName: string;
    template: string;
    templateInput?: string | null;
    users?: User[] | null;
    /** Dictionary of <string> */
    tags?: {
        [propertyName: string]: string;
    } | null;
    resourceId?: string | null;
    resourceState?: ProjectResourceState;
    vaultId?: string | null;
    storageId?: string | null;
    id: string;
}

export declare interface ProjectDataResult {
    code?: number;
    status?: string | null;
    data?: Project;
    location?: string | null;
}

export declare interface ProjectDefinition {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string | null;
    displayName: string;
    template: string;
    templateInput: string;
    users?: UserDefinition[] | null;
}

export declare interface ProjectIdentity {
    projectId: string;
    organization: string;
    displayName: string;
    deploymentScopeId: string;
    tenantId?: string;
    clientId?: string;
    clientSecret?: string | null;
    redirectUrls?: string[] | null;
    objectId?: string;
    id: string;
}

export declare interface ProjectIdentityDataResult {
    code?: number;
    status?: string | null;
    data?: ProjectIdentity;
    location?: string | null;
}

export declare interface ProjectIdentityDefinition {
    displayName: string;
    deploymentScopeId: string;
}

export declare interface ProjectIdentityListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ProjectIdentity[] | null;
    location?: string | null;
}

export declare interface ProjectListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Project[] | null;
    location?: string | null;
}

export declare interface ProjectMembership {
    projectId: string;
    role: ProjectMembershipRole;
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    } | null;
}

/**
 * Defines values for ProjectMembershipRole. \
 * {@link KnownProjectMembershipRole} can be used interchangeably with ProjectMembershipRole,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **None** \
 * **Member** \
 * **Admin** \
 * **Owner**
 */
export declare type ProjectMembershipRole = string;

/**
 * Defines values for ProjectResourceState. \
 * {@link KnownProjectResourceState} can be used interchangeably with ProjectResourceState,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Pending** \
 * **Initializing** \
 * **Provisioning** \
 * **Succeeded** \
 * **Failed**
 */
export declare type ProjectResourceState = string;

export declare interface ProjectTemplate {
    organization: string;
    slug: string;
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
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
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
 * Defines values for RepositoryReferenceProvider. \
 * {@link KnownRepositoryReferenceProvider} can be used interchangeably with RepositoryReferenceProvider,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Unknown** \
 * **GitHub** \
 * **DevOps**
 */
export declare type RepositoryReferenceProvider = string;

/**
 * Defines values for RepositoryReferenceType. \
 * {@link KnownRepositoryReferenceType} can be used interchangeably with RepositoryReferenceType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Unknown** \
 * **Tag** \
 * **Branch** \
 * **Hash**
 */
export declare type RepositoryReferenceType = string;

export declare interface ResultError {
    code?: ResultErrorCode;
    message?: string | null;
    errors?: ValidationError[] | null;
}

/**
 * Defines values for ResultErrorCode. \
 * {@link KnownResultErrorCode} can be used interchangeably with ResultErrorCode,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Unknown** \
 * **Failed** \
 * **Conflict** \
 * **NotFound** \
 * **ServerError** \
 * **ValidationError** \
 * **Unauthorized** \
 * **Forbidden**
 */
export declare type ResultErrorCode = string;

export declare interface Schedule {
    organization: string;
    projectId: string;
    enabled?: boolean;
    recurring?: boolean;
    daysOfWeek?: ScheduleDaysOfWeekItem[] | null;
    utcHour?: number;
    utcMinute?: number;
    creator?: string | null;
    created?: Date;
    lastRun?: Date | null;
    componentTasks?: ComponentTaskReference[] | null;
    id: string;
}

export declare interface ScheduleDataResult {
    code?: number;
    status?: string | null;
    data?: Schedule;
    location?: string | null;
}

/**
 * Defines values for ScheduleDaysOfWeekItem. \
 * {@link KnownScheduleDaysOfWeekItem} can be used interchangeably with ScheduleDaysOfWeekItem,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Sunday** \
 * **Monday** \
 * **Tuesday** \
 * **Wednesday** \
 * **Thursday** \
 * **Friday** \
 * **Saturday**
 */
export declare type ScheduleDaysOfWeekItem = string;

export declare interface ScheduleDefinition {
    enabled?: boolean;
    recurring?: boolean;
    daysOfWeek?: ScheduleDefinitionDaysOfWeekItem[] | null;
    utcHour?: number;
    utcMinute?: number;
    componentTasks?: ComponentTaskReference[] | null;
}

/**
 * Defines values for ScheduleDefinitionDaysOfWeekItem. \
 * {@link KnownScheduleDefinitionDaysOfWeekItem} can be used interchangeably with ScheduleDefinitionDaysOfWeekItem,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Sunday** \
 * **Monday** \
 * **Tuesday** \
 * **Wednesday** \
 * **Thursday** \
 * **Friday** \
 * **Saturday**
 */
export declare type ScheduleDefinitionDaysOfWeekItem = string;

export declare interface ScheduleListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Schedule[] | null;
    location?: string | null;
}

export declare interface StatusResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
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
     * Authorize an existing Deployment Scope.
     * @param id
     * @param organizationId
     * @param options The options parameters.
     */
    authorizeDeploymentScope(id: string | null, organizationId: string, options?: TeamCloudAuthorizeDeploymentScopeOptionalParams): Promise<TeamCloudAuthorizeDeploymentScopeResponse>;
    /**
     * Negotiates the SignalR connection.
     * @param organizationId
     * @param projectId
     * @param options The options parameters.
     */
    negotiateSignalR(organizationId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<coreHttp.RestResponse>;
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

/** Optional parameters. */
export declare interface TeamCloudAuthorizeDeploymentScopeOptionalParams extends coreHttp.OperationOptions {
    body?: DeploymentScope;
}

/** Contains response data for the authorizeDeploymentScope operation. */
export declare type TeamCloudAuthorizeDeploymentScopeResponse = DeploymentScopeDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: DeploymentScopeDataResult;
    };
};

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

/** Optional parameters. */
export declare interface TeamCloudCreateComponentOptionalParams extends coreHttp.OperationOptions {
    body?: ComponentDefinition;
}

/** Contains response data for the createComponent operation. */
export declare type TeamCloudCreateComponentResponse = ComponentDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ComponentDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateComponentTaskOptionalParams extends coreHttp.OperationOptions {
    body?: ComponentTaskDefinition;
}

/** Contains response data for the createComponentTask operation. */
export declare type TeamCloudCreateComponentTaskResponse = ComponentTaskDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ComponentTaskDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateDeploymentScopeOptionalParams extends coreHttp.OperationOptions {
    body?: DeploymentScopeDefinition;
}

/** Contains response data for the createDeploymentScope operation. */
export declare type TeamCloudCreateDeploymentScopeResponse = DeploymentScopeDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: DeploymentScopeDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateOrganizationOptionalParams extends coreHttp.OperationOptions {
    body?: OrganizationDefinition;
}

/** Contains response data for the createOrganization operation. */
export declare type TeamCloudCreateOrganizationResponse = OrganizationDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: OrganizationDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateOrganizationUserOptionalParams extends coreHttp.OperationOptions {
    body?: UserDefinition;
}

/** Contains response data for the createOrganizationUser operation. */
export declare type TeamCloudCreateOrganizationUserResponse = UserDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateProjectIdentityOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectIdentityDefinition;
}

/** Contains response data for the createProjectIdentity operation. */
export declare type TeamCloudCreateProjectIdentityResponse = ProjectIdentityDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectIdentityDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateProjectOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectDefinition;
}

/** Contains response data for the createProject operation. */
export declare type TeamCloudCreateProjectResponse = ProjectDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateProjectTagOptionalParams extends coreHttp.OperationOptions {
    /** Dictionary of <string> */
    body?: {
        [propertyName: string]: string;
    };
}

/** Contains response data for the createProjectTag operation. */
export declare type TeamCloudCreateProjectTagResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateProjectTemplateOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectTemplateDefinition;
}

/** Contains response data for the createProjectTemplate operation. */
export declare type TeamCloudCreateProjectTemplateResponse = ProjectTemplateDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectTemplateDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateProjectUserOptionalParams extends coreHttp.OperationOptions {
    body?: UserDefinition;
}

/** Contains response data for the createProjectUser operation. */
export declare type TeamCloudCreateProjectUserResponse = UserDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudCreateScheduleOptionalParams extends coreHttp.OperationOptions {
    body?: ScheduleDefinition;
}

/** Contains response data for the createSchedule operation. */
export declare type TeamCloudCreateScheduleResponse = ScheduleDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ScheduleDataResult;
    };
};

/** Contains response data for the deleteComponent operation. */
export declare type TeamCloudDeleteComponentResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Contains response data for the deleteDeploymentScope operation. */
export declare type TeamCloudDeleteDeploymentScopeResponse = DeploymentScopeDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: DeploymentScopeDataResult;
    };
};

/** Contains response data for the deleteOrganization operation. */
export declare type TeamCloudDeleteOrganizationResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Contains response data for the deleteOrganizationUser operation. */
export declare type TeamCloudDeleteOrganizationUserResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Contains response data for the deleteProjectIdentity operation. */
export declare type TeamCloudDeleteProjectIdentityResponse = ProjectIdentityDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectIdentityDataResult;
    };
};

/** Contains response data for the deleteProject operation. */
export declare type TeamCloudDeleteProjectResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Contains response data for the deleteProjectTag operation. */
export declare type TeamCloudDeleteProjectTagResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Contains response data for the deleteProjectTemplate operation. */
export declare type TeamCloudDeleteProjectTemplateResponse = ProjectTemplateDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectTemplateDataResult;
    };
};

/** Contains response data for the deleteProjectUser operation. */
export declare type TeamCloudDeleteProjectUserResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Contains response data for the getComponent operation. */
export declare type TeamCloudGetComponentResponse = ComponentDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ComponentDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudGetComponentsOptionalParams extends coreHttp.OperationOptions {
    deleted?: boolean;
}

/** Contains response data for the getComponents operation. */
export declare type TeamCloudGetComponentsResponse = ComponentListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ComponentListDataResult;
    };
};

/** Contains response data for the getComponentTask operation. */
export declare type TeamCloudGetComponentTaskResponse = ComponentTaskDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ComponentTaskDataResult;
    };
};

/** Contains response data for the getComponentTasks operation. */
export declare type TeamCloudGetComponentTasksResponse = ComponentTaskListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ComponentTaskListDataResult;
    };
};

/** Contains response data for the getComponentTemplate operation. */
export declare type TeamCloudGetComponentTemplateResponse = ComponentTemplateDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ComponentTemplateDataResult;
    };
};

/** Contains response data for the getComponentTemplates operation. */
export declare type TeamCloudGetComponentTemplatesResponse = ComponentTemplateListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ComponentTemplateListDataResult;
    };
};

/** Contains response data for the getDeploymentScope operation. */
export declare type TeamCloudGetDeploymentScopeResponse = DeploymentScopeDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: DeploymentScopeDataResult;
    };
};

/** Contains response data for the getDeploymentScopes operation. */
export declare type TeamCloudGetDeploymentScopesResponse = DeploymentScopeListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: DeploymentScopeListDataResult;
    };
};

/** Contains response data for the getOrganization operation. */
export declare type TeamCloudGetOrganizationResponse = OrganizationDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: OrganizationDataResult;
    };
};

/** Contains response data for the getOrganizations operation. */
export declare type TeamCloudGetOrganizationsResponse = OrganizationListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: OrganizationListDataResult;
    };
};

/** Contains response data for the getOrganizationUserMe operation. */
export declare type TeamCloudGetOrganizationUserMeResponse = UserDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserDataResult;
    };
};

/** Contains response data for the getOrganizationUser operation. */
export declare type TeamCloudGetOrganizationUserResponse = UserDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserDataResult;
    };
};

/** Contains response data for the getOrganizationUsers operation. */
export declare type TeamCloudGetOrganizationUsersResponse = UserListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserListDataResult;
    };
};

/** Contains response data for the getProjectIdentities operation. */
export declare type TeamCloudGetProjectIdentitiesResponse = ProjectIdentityListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectIdentityListDataResult;
    };
};

/** Contains response data for the getProjectIdentity operation. */
export declare type TeamCloudGetProjectIdentityResponse = ProjectIdentityDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectIdentityDataResult;
    };
};

/** Contains response data for the getProject operation. */
export declare type TeamCloudGetProjectResponse = ProjectDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectDataResult;
    };
};

/** Contains response data for the getProjects operation. */
export declare type TeamCloudGetProjectsResponse = ProjectListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectListDataResult;
    };
};

/** Contains response data for the getProjectStatus operation. */
export declare type TeamCloudGetProjectStatusResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Contains response data for the getProjectTagByKey operation. */
export declare type TeamCloudGetProjectTagByKeyResponse = StringDictionaryDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StringDictionaryDataResult;
    };
};

/** Contains response data for the getProjectTags operation. */
export declare type TeamCloudGetProjectTagsResponse = StringDictionaryDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StringDictionaryDataResult;
    };
};

/** Contains response data for the getProjectTemplate operation. */
export declare type TeamCloudGetProjectTemplateResponse = ProjectTemplateDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectTemplateDataResult;
    };
};

/** Contains response data for the getProjectTemplates operation. */
export declare type TeamCloudGetProjectTemplatesResponse = ProjectTemplateListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectTemplateListDataResult;
    };
};

/** Contains response data for the getProjectUserMe operation. */
export declare type TeamCloudGetProjectUserMeResponse = UserDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserDataResult;
    };
};

/** Contains response data for the getProjectUser operation. */
export declare type TeamCloudGetProjectUserResponse = UserDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserDataResult;
    };
};

/** Contains response data for the getProjectUsers operation. */
export declare type TeamCloudGetProjectUsersResponse = UserListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserListDataResult;
    };
};

/** Contains response data for the getSchedule operation. */
export declare type TeamCloudGetScheduleResponse = ScheduleDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ScheduleDataResult;
    };
};

/** Contains response data for the getSchedules operation. */
export declare type TeamCloudGetSchedulesResponse = ScheduleListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ScheduleListDataResult;
    };
};

/** Contains response data for the getStatus operation. */
export declare type TeamCloudGetStatusResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Contains response data for the getUserProjectsMe operation. */
export declare type TeamCloudGetUserProjectsMeResponse = ProjectListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectListDataResult;
    };
};

/** Contains response data for the getUserProjects operation. */
export declare type TeamCloudGetUserProjectsResponse = ProjectListDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectListDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudOptionalParams extends coreHttp.ServiceClientOptions {
    /** Overrides client endpoint. */
    endpoint?: string;
}

/** Contains response data for the runSchedule operation. */
export declare type TeamCloudRunScheduleResponse = ScheduleDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ScheduleDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudUpdateDeploymentScopeOptionalParams extends coreHttp.OperationOptions {
    body?: DeploymentScope;
}

/** Contains response data for the updateDeploymentScope operation. */
export declare type TeamCloudUpdateDeploymentScopeResponse = DeploymentScopeDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: DeploymentScopeDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudUpdateOrganizationUserMeOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}

/** Contains response data for the updateOrganizationUserMe operation. */
export declare type TeamCloudUpdateOrganizationUserMeResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudUpdateOrganizationUserOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}

/** Contains response data for the updateOrganizationUser operation. */
export declare type TeamCloudUpdateOrganizationUserResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudUpdateProjectIdentityOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectIdentity;
}

/** Contains response data for the updateProjectIdentity operation. */
export declare type TeamCloudUpdateProjectIdentityResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudUpdateProjectTagOptionalParams extends coreHttp.OperationOptions {
    /** Dictionary of <string> */
    body?: {
        [propertyName: string]: string;
    };
}

/** Contains response data for the updateProjectTag operation. */
export declare type TeamCloudUpdateProjectTagResponse = StatusResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: StatusResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudUpdateProjectTemplateOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectTemplate;
}

/** Contains response data for the updateProjectTemplate operation. */
export declare type TeamCloudUpdateProjectTemplateResponse = ProjectTemplateDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: ProjectTemplateDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudUpdateProjectUserMeOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}

/** Contains response data for the updateProjectUserMe operation. */
export declare type TeamCloudUpdateProjectUserMeResponse = UserDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserDataResult;
    };
};

/** Optional parameters. */
export declare interface TeamCloudUpdateProjectUserOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}

/** Contains response data for the updateProjectUser operation. */
export declare type TeamCloudUpdateProjectUserResponse = UserDataResult & {
    /** The underlying HTTP response. */
    _response: coreHttp.HttpResponse & {
        /** The response body as text (string format) */
        bodyAsText: string;
        /** The response body as parsed JSON or XML */
        parsedBody: UserDataResult;
    };
};

export declare interface User {
    organization: string;
    displayName?: string | null;
    loginName?: string | null;
    mailAddress?: string | null;
    userType: UserType;
    role: UserRole;
    projectMemberships?: ProjectMembership[] | null;
    /** Dictionary of <string> */
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
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    } | null;
}

export declare interface UserListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: User[] | null;
    location?: string | null;
}

/**
 * Defines values for UserRole. \
 * {@link KnownUserRole} can be used interchangeably with UserRole,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **None** \
 * **Member** \
 * **Admin** \
 * **Owner**
 */
export declare type UserRole = string;

/**
 * Defines values for UserType. \
 * {@link KnownUserType} can be used interchangeably with UserType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **User** \
 * **System** \
 * **Provider** \
 * **Application**
 */
export declare type UserType = string;

export declare interface ValidationError {
    field?: string | null;
    message?: string | null;
}

export { }
