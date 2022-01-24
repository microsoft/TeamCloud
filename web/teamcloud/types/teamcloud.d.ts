import * as coreAuth from '@azure/core-auth';
import * as coreClient from '@azure/core-client';

export declare interface AdapterInformation {
    type?: AdapterInformationType;
    displayName?: string;
    inputDataSchema?: string;
    inputDataForm?: string;
}

export declare interface AdapterInformationListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: AdapterInformation[];
    location?: string;
}

/**
 * Defines values for AdapterInformationType. \
 * {@link KnownAdapterInformationType} can be used interchangeably with AdapterInformationType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub** \
 * **Kubernetes**
 */
export declare type AdapterInformationType = string;

export declare interface AlternateIdentity {
    login?: string;
}

/** Optional parameters. */
export declare interface CancelComponentTaskOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the cancelComponentTask operation. */
export declare type CancelComponentTaskResponse = ComponentTaskDataResult;

export declare interface CommandAuditEntity {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly commandId?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly organizationId?: string;
    commandJson?: string;
    resultJson?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly projectId?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly userId?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly parentId?: string;
    command?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly componentTask?: string;
    runtimeStatus?: CommandAuditEntityRuntimeStatus;
    customStatus?: string;
    errors?: string;
    created?: Date;
    updated?: Date;
}

export declare interface CommandAuditEntityDataResult {
    code?: number;
    status?: string;
    data?: CommandAuditEntity;
    location?: string;
}

export declare interface CommandAuditEntityListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: CommandAuditEntity[];
    location?: string;
}

/**
 * Defines values for CommandAuditEntityRuntimeStatus. \
 * {@link KnownCommandAuditEntityRuntimeStatus} can be used interchangeably with CommandAuditEntityRuntimeStatus,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Running** \
 * **Completed** \
 * **ContinuedAsNew** \
 * **Failed** \
 * **Canceled** \
 * **Terminated** \
 * **Pending** \
 * **Unknown**
 */
export declare type CommandAuditEntityRuntimeStatus = string;

export declare interface Component {
    href?: string;
    organization: string;
    templateId: string;
    projectId: string;
    creator: string;
    displayName?: string;
    description?: string;
    inputJson?: string;
    valueJson?: string;
    type: ComponentType;
    resourceId?: string;
    resourceUrl?: string;
    resourceState?: ComponentResourceState;
    deploymentScopeId?: string;
    identityId?: string;
    deleted?: Date;
    ttl?: number;
    slug: string;
    id: string;
}

export declare interface ComponentDataResult {
    code?: number;
    status?: string;
    data?: Component;
    location?: string;
}

export declare interface ComponentDefinition {
    templateId: string;
    displayName: string;
    inputJson?: string;
    deploymentScopeId?: string;
}

export declare interface ComponentListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Component[];
    location?: string;
}

/**
 * Defines values for ComponentResourceState. \
 * {@link KnownComponentResourceState} can be used interchangeably with ComponentResourceState,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Pending** \
 * **Initializing** \
 * **Provisioning** \
 * **Provisioned** \
 * **Deprovisioning** \
 * **Deprovisioned** \
 * **Failed**
 */
export declare type ComponentResourceState = string;

export declare interface ComponentTask {
    organization: string;
    componentId: string;
    projectId: string;
    requestedBy?: string;
    scheduleId?: string;
    type?: ComponentTaskType;
    typeName?: string;
    created?: Date;
    started?: Date;
    finished?: Date;
    inputJson?: string;
    output?: string;
    resourceId?: string;
    taskState?: ComponentTaskState;
    exitCode?: number;
    id: string;
}

export declare interface ComponentTaskDataResult {
    code?: number;
    status?: string;
    data?: ComponentTask;
    location?: string;
}

export declare interface ComponentTaskDefinition {
    taskId: string;
    inputJson?: string;
}

export declare interface ComponentTaskListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ComponentTask[];
    location?: string;
}

export declare interface ComponentTaskReference {
    componentId?: string;
    componentTaskTemplateId?: string;
    inputJson?: string;
}

export declare interface ComponentTaskRunner {
    id?: string;
    /** Dictionary of <string> */
    with?: {
        [propertyName: string]: string;
    };
}

/**
 * Defines values for ComponentTaskState. \
 * {@link KnownComponentTaskState} can be used interchangeably with ComponentTaskState,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Pending** \
 * **Initializing** \
 * **Processing** \
 * **Succeeded** \
 * **Canceled** \
 * **Failed**
 */
export declare type ComponentTaskState = string;

export declare interface ComponentTaskTemplate {
    id?: string;
    displayName?: string;
    description?: string;
    inputJsonSchema?: string;
    type?: ComponentTaskTemplateType;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly typeName?: string;
}

/**
 * Defines values for ComponentTaskTemplateType. \
 * {@link KnownComponentTaskTemplateType} can be used interchangeably with ComponentTaskTemplateType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Custom** \
 * **Create** \
 * **Delete**
 */
export declare type ComponentTaskTemplateType = string;

/**
 * Defines values for ComponentTaskType. \
 * {@link KnownComponentTaskType} can be used interchangeably with ComponentTaskType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Custom** \
 * **Create** \
 * **Delete**
 */
export declare type ComponentTaskType = string;

export declare interface ComponentTemplate {
    organization: string;
    parentId: string;
    displayName?: string;
    description?: string;
    repository: RepositoryReference;
    permissions?: ComponentTemplatePermissions;
    inputJsonSchema?: string;
    tasks?: ComponentTaskTemplate[];
    taskRunner?: ComponentTaskRunner;
    type: ComponentTemplateType;
    folder?: string;
    /** Anything */
    configuration?: any;
    id: string;
}

export declare interface ComponentTemplateDataResult {
    code?: number;
    status?: string;
    data?: ComponentTemplate;
    location?: string;
}

export declare interface ComponentTemplateListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ComponentTemplate[];
    location?: string;
}

export declare interface ComponentTemplatePermissions {
    none?: string[];
    member?: string[];
    admin?: string[];
    owner?: string[];
    adapter?: string[];
}

/**
 * Defines values for ComponentTemplateType. \
 * {@link KnownComponentTemplateType} can be used interchangeably with ComponentTemplateType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Environment** \
 * **Repository** \
 * **Namespace**
 */
export declare type ComponentTemplateType = string;

/**
 * Defines values for ComponentType. \
 * {@link KnownComponentType} can be used interchangeably with ComponentType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Environment** \
 * **Repository** \
 * **Namespace**
 */
export declare type ComponentType = string;

/** Optional parameters. */
export declare interface CreateComponentOptionalParams extends coreClient.OperationOptions {
    body?: ComponentDefinition;
}

/** Contains response data for the createComponent operation. */
export declare type CreateComponentResponse = ComponentDataResult;

/** Optional parameters. */
export declare interface CreateComponentTaskOptionalParams extends coreClient.OperationOptions {
    body?: ComponentTaskDefinition;
}

/** Contains response data for the createComponentTask operation. */
export declare type CreateComponentTaskResponse = ComponentTaskDataResult;

/** Optional parameters. */
export declare interface CreateDeploymentScopeOptionalParams extends coreClient.OperationOptions {
    body?: DeploymentScopeDefinition;
}

/** Contains response data for the createDeploymentScope operation. */
export declare type CreateDeploymentScopeResponse = DeploymentScopeDataResult;

/** Optional parameters. */
export declare interface CreateOrganizationOptionalParams extends coreClient.OperationOptions {
    body?: OrganizationDefinition;
}

/** Contains response data for the createOrganization operation. */
export declare type CreateOrganizationResponse = OrganizationDataResult;

/** Optional parameters. */
export declare interface CreateOrganizationUserOptionalParams extends coreClient.OperationOptions {
    body?: UserDefinition;
}

/** Contains response data for the createOrganizationUser operation. */
export declare type CreateOrganizationUserResponse = UserDataResult;

/** Optional parameters. */
export declare interface CreateProjectIdentityOptionalParams extends coreClient.OperationOptions {
    body?: ProjectIdentityDefinition;
}

/** Contains response data for the createProjectIdentity operation. */
export declare type CreateProjectIdentityResponse = ProjectIdentityDataResult;

/** Optional parameters. */
export declare interface CreateProjectOptionalParams extends coreClient.OperationOptions {
    body?: ProjectDefinition;
}

/** Contains response data for the createProject operation. */
export declare type CreateProjectResponse = ProjectDataResult;

/** Optional parameters. */
export declare interface CreateProjectTagOptionalParams extends coreClient.OperationOptions {
    /** Dictionary of <string> */
    body?: {
        [propertyName: string]: string;
    };
}

/** Contains response data for the createProjectTag operation. */
export declare type CreateProjectTagResponse = StatusResult;

/** Optional parameters. */
export declare interface CreateProjectTemplateOptionalParams extends coreClient.OperationOptions {
    body?: ProjectTemplateDefinition;
}

/** Contains response data for the createProjectTemplate operation. */
export declare type CreateProjectTemplateResponse = ProjectTemplateDataResult;

/** Optional parameters. */
export declare interface CreateProjectUserOptionalParams extends coreClient.OperationOptions {
    body?: UserDefinition;
}

/** Contains response data for the createProjectUser operation. */
export declare type CreateProjectUserResponse = UserDataResult;

/** Optional parameters. */
export declare interface CreateScheduleOptionalParams extends coreClient.OperationOptions {
    body?: ScheduleDefinition;
}

/** Contains response data for the createSchedule operation. */
export declare type CreateScheduleResponse = ScheduleDataResult;

/** Optional parameters. */
export declare interface DeleteComponentOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteComponent operation. */
export declare type DeleteComponentResponse = StatusResult;

/** Optional parameters. */
export declare interface DeleteDeploymentScopeOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteDeploymentScope operation. */
export declare type DeleteDeploymentScopeResponse = DeploymentScopeDataResult;

/** Optional parameters. */
export declare interface DeleteOrganizationOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteOrganization operation. */
export declare type DeleteOrganizationResponse = StatusResult;

/** Optional parameters. */
export declare interface DeleteOrganizationUserOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteOrganizationUser operation. */
export declare type DeleteOrganizationUserResponse = StatusResult;

/** Optional parameters. */
export declare interface DeleteProjectIdentityOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteProjectIdentity operation. */
export declare type DeleteProjectIdentityResponse = ProjectIdentityDataResult;

/** Optional parameters. */
export declare interface DeleteProjectOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteProject operation. */
export declare type DeleteProjectResponse = StatusResult;

/** Optional parameters. */
export declare interface DeleteProjectTagOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteProjectTag operation. */
export declare type DeleteProjectTagResponse = StatusResult;

/** Optional parameters. */
export declare interface DeleteProjectTemplateOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteProjectTemplate operation. */
export declare type DeleteProjectTemplateResponse = ProjectTemplateDataResult;

/** Optional parameters. */
export declare interface DeleteProjectUserOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the deleteProjectUser operation. */
export declare type DeleteProjectUserResponse = StatusResult;

export declare interface DeploymentScope {
    organization: string;
    displayName: string;
    slug: string;
    isDefault: boolean;
    type: DeploymentScopeType;
    inputDataSchema?: string;
    inputData?: string;
    managementGroupId?: string;
    subscriptionIds?: string[];
    authorizable?: boolean;
    authorized?: boolean;
    authorizeUrl?: string;
    componentTypes?: DeploymentScopeComponentTypesItem[];
    id: string;
}

/**
 * Defines values for DeploymentScopeComponentTypesItem. \
 * {@link KnownDeploymentScopeComponentTypesItem} can be used interchangeably with DeploymentScopeComponentTypesItem,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Environment** \
 * **Repository** \
 * **Namespace**
 */
export declare type DeploymentScopeComponentTypesItem = string;

export declare interface DeploymentScopeDataResult {
    code?: number;
    status?: string;
    data?: DeploymentScope;
    location?: string;
}

export declare interface DeploymentScopeDefinition {
    displayName: string;
    type: DeploymentScopeDefinitionType;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string;
    inputData?: string;
    isDefault?: boolean;
}

/**
 * Defines values for DeploymentScopeDefinitionType. \
 * {@link KnownDeploymentScopeDefinitionType} can be used interchangeably with DeploymentScopeDefinitionType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub** \
 * **Kubernetes**
 */
export declare type DeploymentScopeDefinitionType = string;

export declare interface DeploymentScopeListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: DeploymentScope[];
    location?: string;
}

/**
 * Defines values for DeploymentScopeType. \
 * {@link KnownDeploymentScopeType} can be used interchangeably with DeploymentScopeType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub** \
 * **Kubernetes**
 */
export declare type DeploymentScopeType = string;

export declare interface ErrorResult {
    code?: number;
    status?: string;
    errors?: ResultError[];
}

/** Optional parameters. */
export declare interface GetAdaptersOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getAdapters operation. */
export declare type GetAdaptersResponse = AdapterInformationListDataResult;

/** Optional parameters. */
export declare interface GetAuditCommandsOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getAuditCommands operation. */
export declare type GetAuditCommandsResponse = StringListDataResult;

/** Optional parameters. */
export declare interface GetAuditEntriesOptionalParams extends coreClient.OperationOptions {
    timeRange?: string;
    /** Array of Get1ItemsItem */
    commands?: string[];
}

/** Contains response data for the getAuditEntries operation. */
export declare type GetAuditEntriesResponse = CommandAuditEntityListDataResult;

/** Optional parameters. */
export declare interface GetAuditEntryOptionalParams extends coreClient.OperationOptions {
    expand?: boolean;
}

/** Contains response data for the getAuditEntry operation. */
export declare type GetAuditEntryResponse = CommandAuditEntityDataResult;

/** Optional parameters. */
export declare interface GetComponentOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getComponent operation. */
export declare type GetComponentResponse = ComponentDataResult;

/** Optional parameters. */
export declare interface GetComponentsOptionalParams extends coreClient.OperationOptions {
    deleted?: boolean;
}

/** Contains response data for the getComponents operation. */
export declare type GetComponentsResponse = ComponentListDataResult;

/** Optional parameters. */
export declare interface GetComponentTaskOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getComponentTask operation. */
export declare type GetComponentTaskResponse = ComponentTaskDataResult;

/** Optional parameters. */
export declare interface GetComponentTasksOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getComponentTasks operation. */
export declare type GetComponentTasksResponse = ComponentTaskListDataResult;

/** Optional parameters. */
export declare interface GetComponentTemplateOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getComponentTemplate operation. */
export declare type GetComponentTemplateResponse = ComponentTemplateDataResult;

/** Optional parameters. */
export declare interface GetComponentTemplatesOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getComponentTemplates operation. */
export declare type GetComponentTemplatesResponse = ComponentTemplateListDataResult;

/** Optional parameters. */
export declare interface GetDeploymentScopeOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getDeploymentScope operation. */
export declare type GetDeploymentScopeResponse = DeploymentScopeDataResult;

/** Optional parameters. */
export declare interface GetDeploymentScopesOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getDeploymentScopes operation. */
export declare type GetDeploymentScopesResponse = DeploymentScopeListDataResult;

/** Optional parameters. */
export declare interface GetOrganizationOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getOrganization operation. */
export declare type GetOrganizationResponse = OrganizationDataResult;

/** Optional parameters. */
export declare interface GetOrganizationsOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getOrganizations operation. */
export declare type GetOrganizationsResponse = OrganizationListDataResult;

/** Optional parameters. */
export declare interface GetOrganizationUserMeOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getOrganizationUserMe operation. */
export declare type GetOrganizationUserMeResponse = UserDataResult;

/** Optional parameters. */
export declare interface GetOrganizationUserOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getOrganizationUser operation. */
export declare type GetOrganizationUserResponse = UserDataResult;

/** Optional parameters. */
export declare interface GetOrganizationUsersOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getOrganizationUsers operation. */
export declare type GetOrganizationUsersResponse = UserListDataResult;

/** Optional parameters. */
export declare interface GetProjectIdentitiesOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectIdentities operation. */
export declare type GetProjectIdentitiesResponse = ProjectIdentityListDataResult;

/** Optional parameters. */
export declare interface GetProjectIdentityOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectIdentity operation. */
export declare type GetProjectIdentityResponse = ProjectIdentityDataResult;

/** Optional parameters. */
export declare interface GetProjectOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProject operation. */
export declare type GetProjectResponse = ProjectDataResult;

/** Optional parameters. */
export declare interface GetProjectsOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjects operation. */
export declare type GetProjectsResponse = ProjectListDataResult;

/** Optional parameters. */
export declare interface GetProjectStatusOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectStatus operation. */
export declare type GetProjectStatusResponse = StatusResult;

/** Optional parameters. */
export declare interface GetProjectTagByKeyOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectTagByKey operation. */
export declare type GetProjectTagByKeyResponse = StringDictionaryDataResult;

/** Optional parameters. */
export declare interface GetProjectTagsOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectTags operation. */
export declare type GetProjectTagsResponse = StringDictionaryDataResult;

/** Optional parameters. */
export declare interface GetProjectTemplateOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectTemplate operation. */
export declare type GetProjectTemplateResponse = ProjectTemplateDataResult;

/** Optional parameters. */
export declare interface GetProjectTemplatesOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectTemplates operation. */
export declare type GetProjectTemplatesResponse = ProjectTemplateListDataResult;

/** Optional parameters. */
export declare interface GetProjectUserMeOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectUserMe operation. */
export declare type GetProjectUserMeResponse = UserDataResult;

/** Optional parameters. */
export declare interface GetProjectUserOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectUser operation. */
export declare type GetProjectUserResponse = UserDataResult;

/** Optional parameters. */
export declare interface GetProjectUsersOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getProjectUsers operation. */
export declare type GetProjectUsersResponse = UserListDataResult;

/** Optional parameters. */
export declare interface GetScheduleOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getSchedule operation. */
export declare type GetScheduleResponse = ScheduleDataResult;

/** Optional parameters. */
export declare interface GetSchedulesOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getSchedules operation. */
export declare type GetSchedulesResponse = ScheduleListDataResult;

/** Optional parameters. */
export declare interface GetStatusOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getStatus operation. */
export declare type GetStatusResponse = StatusResult;

/** Optional parameters. */
export declare interface GetUserProjectsMeOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getUserProjectsMe operation. */
export declare type GetUserProjectsMeResponse = ProjectListDataResult;

/** Optional parameters. */
export declare interface GetUserProjectsOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the getUserProjects operation. */
export declare type GetUserProjectsResponse = ProjectListDataResult;

/** Optional parameters. */
export declare interface InitializeAuthorizationOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the initializeAuthorization operation. */
export declare type InitializeAuthorizationResponse = DeploymentScopeDataResult;

/** Known values of {@link AdapterInformationType} that the service accepts. */
export declare enum KnownAdapterInformationType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub",
    Kubernetes = "Kubernetes"
}

/** Known values of {@link CommandAuditEntityRuntimeStatus} that the service accepts. */
export declare enum KnownCommandAuditEntityRuntimeStatus {
    Running = "Running",
    Completed = "Completed",
    ContinuedAsNew = "ContinuedAsNew",
    Failed = "Failed",
    Canceled = "Canceled",
    Terminated = "Terminated",
    Pending = "Pending",
    Unknown = "Unknown"
}

/** Known values of {@link ComponentResourceState} that the service accepts. */
export declare enum KnownComponentResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Provisioned = "Provisioned",
    Deprovisioning = "Deprovisioning",
    Deprovisioned = "Deprovisioned",
    Failed = "Failed"
}

/** Known values of {@link ComponentTaskState} that the service accepts. */
export declare enum KnownComponentTaskState {
    Pending = "Pending",
    Initializing = "Initializing",
    Processing = "Processing",
    Succeeded = "Succeeded",
    Canceled = "Canceled",
    Failed = "Failed"
}

/** Known values of {@link ComponentTaskTemplateType} that the service accepts. */
export declare enum KnownComponentTaskTemplateType {
    Custom = "Custom",
    Create = "Create",
    Delete = "Delete"
}

/** Known values of {@link ComponentTaskType} that the service accepts. */
export declare enum KnownComponentTaskType {
    Custom = "Custom",
    Create = "Create",
    Delete = "Delete"
}

/** Known values of {@link ComponentTemplateType} that the service accepts. */
export declare enum KnownComponentTemplateType {
    Environment = "Environment",
    Repository = "Repository",
    Namespace = "Namespace"
}

/** Known values of {@link ComponentType} that the service accepts. */
export declare enum KnownComponentType {
    Environment = "Environment",
    Repository = "Repository",
    Namespace = "Namespace"
}

/** Known values of {@link DeploymentScopeComponentTypesItem} that the service accepts. */
export declare enum KnownDeploymentScopeComponentTypesItem {
    Environment = "Environment",
    Repository = "Repository",
    Namespace = "Namespace"
}

/** Known values of {@link DeploymentScopeDefinitionType} that the service accepts. */
export declare enum KnownDeploymentScopeDefinitionType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub",
    Kubernetes = "Kubernetes"
}

/** Known values of {@link DeploymentScopeType} that the service accepts. */
export declare enum KnownDeploymentScopeType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub",
    Kubernetes = "Kubernetes"
}

/** Known values of {@link OrganizationResourceState} that the service accepts. */
export declare enum KnownOrganizationResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Provisioned = "Provisioned",
    Deprovisioning = "Deprovisioning",
    Deprovisioned = "Deprovisioned",
    Failed = "Failed"
}

/** Known values of {@link ProjectMembershipRole} that the service accepts. */
export declare enum KnownProjectMembershipRole {
    None = "None",
    Member = "Member",
    Admin = "Admin",
    Owner = "Owner",
    Adapter = "Adapter"
}

/** Known values of {@link ProjectResourceState} that the service accepts. */
export declare enum KnownProjectResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Provisioned = "Provisioned",
    Deprovisioning = "Deprovisioning",
    Deprovisioned = "Deprovisioned",
    Failed = "Failed"
}

/** Known values of {@link RepositoryReferenceProvider} that the service accepts. */
export declare enum KnownRepositoryReferenceProvider {
    Unknown = "Unknown",
    GitHub = "GitHub",
    DevOps = "DevOps"
}

/** Known values of {@link RepositoryReferenceType} that the service accepts. */
export declare enum KnownRepositoryReferenceType {
    Unknown = "Unknown",
    Tag = "Tag",
    Branch = "Branch",
    Hash = "Hash"
}

/** Known values of {@link ResultErrorCode} that the service accepts. */
export declare enum KnownResultErrorCode {
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
export declare enum KnownScheduleDaysOfWeekItem {
    Sunday = "Sunday",
    Monday = "Monday",
    Tuesday = "Tuesday",
    Wednesday = "Wednesday",
    Thursday = "Thursday",
    Friday = "Friday",
    Saturday = "Saturday"
}

/** Known values of {@link ScheduleDefinitionDaysOfWeekItem} that the service accepts. */
export declare enum KnownScheduleDefinitionDaysOfWeekItem {
    Sunday = "Sunday",
    Monday = "Monday",
    Tuesday = "Tuesday",
    Wednesday = "Wednesday",
    Thursday = "Thursday",
    Friday = "Friday",
    Saturday = "Saturday"
}

/** Known values of {@link UserRole} that the service accepts. */
export declare enum KnownUserRole {
    None = "None",
    Member = "Member",
    Admin = "Admin",
    Owner = "Owner",
    Adapter = "Adapter"
}

/** Known values of {@link UserType} that the service accepts. */
export declare enum KnownUserType {
    User = "User",
    Group = "Group",
    System = "System",
    Service = "Service"
}

/** Optional parameters. */
export declare interface NegotiateSignalROptionalParams extends coreClient.OperationOptions {
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
    };
    resourceId?: string;
    resourceState?: OrganizationResourceState;
    secretsVaultId?: string;
    galleryId?: string;
    registryId?: string;
    storageId?: string;
    id: string;
}

export declare interface OrganizationDataResult {
    code?: number;
    status?: string;
    data?: Organization;
    location?: string;
}

export declare interface OrganizationDefinition {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string;
    displayName: string;
    subscriptionId: string;
    location: string;
}

export declare interface OrganizationListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Organization[];
    location?: string;
}

/**
 * Defines values for OrganizationResourceState. \
 * {@link KnownOrganizationResourceState} can be used interchangeably with OrganizationResourceState,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Pending** \
 * **Initializing** \
 * **Provisioning** \
 * **Provisioned** \
 * **Deprovisioning** \
 * **Deprovisioned** \
 * **Failed**
 */
export declare type OrganizationResourceState = string;

export declare interface Project {
    organization: string;
    slug: string;
    displayName: string;
    template: string;
    templateInput?: string;
    users?: User[];
    /** Dictionary of <string> */
    tags?: {
        [propertyName: string]: string;
    };
    resourceId?: string;
    resourceState?: ProjectResourceState;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly vaultId?: string;
    sharedVaultId?: string;
    secretsVaultId?: string;
    storageId?: string;
    deleted?: Date;
    ttl?: number;
    id: string;
}

export declare interface ProjectDataResult {
    code?: number;
    status?: string;
    data?: Project;
    location?: string;
}

export declare interface ProjectDefinition {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string;
    displayName: string;
    template: string;
    templateInput: string;
    users?: UserDefinition[];
}

export declare interface ProjectIdentity {
    projectId: string;
    organization: string;
    displayName: string;
    deploymentScopeId: string;
    tenantId?: string;
    clientId?: string;
    clientSecret?: string;
    redirectUrls?: string[];
    objectId?: string;
    id: string;
}

export declare interface ProjectIdentityDataResult {
    code?: number;
    status?: string;
    data?: ProjectIdentity;
    location?: string;
}

export declare interface ProjectIdentityDefinition {
    displayName: string;
    deploymentScopeId: string;
}

export declare interface ProjectIdentityListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ProjectIdentity[];
    location?: string;
}

export declare interface ProjectListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Project[];
    location?: string;
}

export declare interface ProjectMembership {
    projectId: string;
    role: ProjectMembershipRole;
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    };
}

/**
 * Defines values for ProjectMembershipRole. \
 * {@link KnownProjectMembershipRole} can be used interchangeably with ProjectMembershipRole,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **None** \
 * **Member** \
 * **Admin** \
 * **Owner** \
 * **Adapter**
 */
export declare type ProjectMembershipRole = string;

/**
 * Defines values for ProjectResourceState. \
 * {@link KnownProjectResourceState} can be used interchangeably with ProjectResourceState,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Pending** \
 * **Initializing** \
 * **Provisioning** \
 * **Provisioned** \
 * **Deprovisioning** \
 * **Deprovisioned** \
 * **Failed**
 */
export declare type ProjectResourceState = string;

export declare interface ProjectTemplate {
    organization: string;
    slug: string;
    name?: string;
    displayName: string;
    components?: string[];
    repository: RepositoryReference;
    description?: string;
    isDefault: boolean;
    inputJsonSchema?: string;
    id: string;
}

export declare interface ProjectTemplateDataResult {
    code?: number;
    status?: string;
    data?: ProjectTemplate;
    location?: string;
}

export declare interface ProjectTemplateDefinition {
    displayName: string;
    repository: RepositoryDefinition;
}

export declare interface ProjectTemplateListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ProjectTemplate[];
    location?: string;
}

export declare interface RepositoryDefinition {
    url: string;
    token?: string;
    version?: string;
}

export declare interface RepositoryReference {
    url: string;
    token?: string;
    version?: string;
    baselUrl?: string;
    mountUrl?: string;
    ref?: string;
    provider: RepositoryReferenceProvider;
    type: RepositoryReferenceType;
    organization?: string;
    repository?: string;
    project?: string;
}

/**
 * Defines values for RepositoryReferenceProvider. \
 * {@link KnownRepositoryReferenceProvider} can be used interchangeably with RepositoryReferenceProvider,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Unknown** \
 * **GitHub** \
 * **DevOps**
 */
export declare type RepositoryReferenceProvider = string;

/**
 * Defines values for RepositoryReferenceType. \
 * {@link KnownRepositoryReferenceType} can be used interchangeably with RepositoryReferenceType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Unknown** \
 * **Tag** \
 * **Branch** \
 * **Hash**
 */
export declare type RepositoryReferenceType = string;

/** Optional parameters. */
export declare interface ReRunComponentTaskOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the reRunComponentTask operation. */
export declare type ReRunComponentTaskResponse = ComponentTaskDataResult;

export declare interface ResultError {
    code?: ResultErrorCode;
    message?: string;
    errors?: ValidationError[];
}

/**
 * Defines values for ResultErrorCode. \
 * {@link KnownResultErrorCode} can be used interchangeably with ResultErrorCode,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
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

/** Optional parameters. */
export declare interface RunScheduleOptionalParams extends coreClient.OperationOptions {
}

/** Contains response data for the runSchedule operation. */
export declare type RunScheduleResponse = ScheduleDataResult;

export declare interface Schedule {
    organization: string;
    projectId: string;
    enabled?: boolean;
    recurring?: boolean;
    daysOfWeek?: ScheduleDaysOfWeekItem[];
    utcHour?: number;
    utcMinute?: number;
    creator?: string;
    created?: Date;
    lastUpdatedBy?: string;
    lastUpdated?: Date;
    lastRun?: Date;
    componentTasks?: ComponentTaskReference[];
    id: string;
}

export declare interface ScheduleDataResult {
    code?: number;
    status?: string;
    data?: Schedule;
    location?: string;
}

/**
 * Defines values for ScheduleDaysOfWeekItem. \
 * {@link KnownScheduleDaysOfWeekItem} can be used interchangeably with ScheduleDaysOfWeekItem,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
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
    daysOfWeek?: ScheduleDefinitionDaysOfWeekItem[];
    utcHour?: number;
    utcMinute?: number;
    componentTasks?: ComponentTaskReference[];
}

/**
 * Defines values for ScheduleDefinitionDaysOfWeekItem. \
 * {@link KnownScheduleDefinitionDaysOfWeekItem} can be used interchangeably with ScheduleDefinitionDaysOfWeekItem,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
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
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Schedule[];
    location?: string;
}

export declare interface StatusResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly state?: string;
    stateMessage?: string;
    location?: string;
    errors?: ResultError[];
    trackingId?: string;
}

export declare interface StringDictionaryDataResult {
    code?: number;
    status?: string;
    /**
     * Dictionary of <string>
     * NOTE: This property will not be serialized. It can only be populated by the server.
     */
    readonly data?: {
        [propertyName: string]: string;
    };
    location?: string;
}

export declare interface StringListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: string[];
    location?: string;
}

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

export declare class TeamCloudContext extends coreClient.ServiceClient {
    $host: string;
    /**
     * Initializes a new instance of the TeamCloudContext class.
     * @param credentials Subscription credentials which uniquely identify client subscription.
     * @param $host server parameter
     * @param options The parameter options
     */
    constructor(credentials: coreAuth.TokenCredential, $host: string, options?: TeamCloudOptionalParams);
}

/** Optional parameters. */
export declare interface TeamCloudOptionalParams extends coreClient.ServiceClientOptions {
    /** Overrides client endpoint. */
    endpoint?: string;
}

/** Optional parameters. */
export declare interface UpdateDeploymentScopeOptionalParams extends coreClient.OperationOptions {
    body?: DeploymentScope;
}

/** Contains response data for the updateDeploymentScope operation. */
export declare type UpdateDeploymentScopeResponse = DeploymentScopeDataResult;

/** Optional parameters. */
export declare interface UpdateOrganizationUserMeOptionalParams extends coreClient.OperationOptions {
    body?: User;
}

/** Contains response data for the updateOrganizationUserMe operation. */
export declare type UpdateOrganizationUserMeResponse = UserDataResult;

/** Optional parameters. */
export declare interface UpdateOrganizationUserOptionalParams extends coreClient.OperationOptions {
    body?: User;
}

/** Contains response data for the updateOrganizationUser operation. */
export declare type UpdateOrganizationUserResponse = UserDataResult;

/** Optional parameters. */
export declare interface UpdateProjectIdentityOptionalParams extends coreClient.OperationOptions {
    body?: ProjectIdentity;
}

/** Contains response data for the updateProjectIdentity operation. */
export declare type UpdateProjectIdentityResponse = StatusResult;

/** Optional parameters. */
export declare interface UpdateProjectTagOptionalParams extends coreClient.OperationOptions {
    /** Dictionary of <string> */
    body?: {
        [propertyName: string]: string;
    };
}

/** Contains response data for the updateProjectTag operation. */
export declare type UpdateProjectTagResponse = StatusResult;

/** Optional parameters. */
export declare interface UpdateProjectTemplateOptionalParams extends coreClient.OperationOptions {
    body?: ProjectTemplate;
}

/** Contains response data for the updateProjectTemplate operation. */
export declare type UpdateProjectTemplateResponse = ProjectTemplateDataResult;

/** Optional parameters. */
export declare interface UpdateProjectUserMeOptionalParams extends coreClient.OperationOptions {
    body?: User;
}

/** Contains response data for the updateProjectUserMe operation. */
export declare type UpdateProjectUserMeResponse = UserDataResult;

/** Optional parameters. */
export declare interface UpdateProjectUserOptionalParams extends coreClient.OperationOptions {
    body?: User;
}

/** Contains response data for the updateProjectUser operation. */
export declare type UpdateProjectUserResponse = UserDataResult;

/** Optional parameters. */
export declare interface UpdateScheduleOptionalParams extends coreClient.OperationOptions {
    body?: Schedule;
}

/** Contains response data for the updateSchedule operation. */
export declare type UpdateScheduleResponse = ScheduleDataResult;

export declare interface User {
    organization: string;
    displayName?: string;
    loginName?: string;
    mailAddress?: string;
    userType: UserType;
    role: UserRole;
    projectMemberships?: ProjectMembership[];
    alternateIdentities?: UserAlternateIdentities;
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    };
    id: string;
}

export declare interface UserAlternateIdentities {
    azureResourceManager?: AlternateIdentity;
    azureDevOps?: AlternateIdentity;
    gitHub?: AlternateIdentity;
    kubernetes?: AlternateIdentity;
}

export declare interface UserDataResult {
    code?: number;
    status?: string;
    data?: User;
    location?: string;
}

export declare interface UserDefinition {
    identifier: string;
    role: string;
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    };
}

export declare interface UserListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: User[];
    location?: string;
}

/**
 * Defines values for UserRole. \
 * {@link KnownUserRole} can be used interchangeably with UserRole,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **None** \
 * **Member** \
 * **Admin** \
 * **Owner** \
 * **Adapter**
 */
export declare type UserRole = string;

/**
 * Defines values for UserType. \
 * {@link KnownUserType} can be used interchangeably with UserType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **User** \
 * **Group** \
 * **System** \
 * **Service**
 */
export declare type UserType = string;

export declare interface ValidationError {
    field?: string;
    message?: string;
}

export { }
