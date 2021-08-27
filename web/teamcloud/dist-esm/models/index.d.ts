import * as coreClient from "@azure/core-client";
export interface AdapterInformationListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: AdapterInformation[];
    location?: string;
}
export interface AdapterInformation {
    type?: AdapterInformationType;
    displayName?: string;
    inputDataSchema?: string;
    inputDataForm?: string;
}
export interface ErrorResult {
    code?: number;
    status?: string;
    errors?: ResultError[];
}
export interface ResultError {
    code?: ResultErrorCode;
    message?: string;
    errors?: ValidationError[];
}
export interface ValidationError {
    field?: string;
    message?: string;
}
export interface ComponentListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Component[];
    location?: string;
}
export interface Component {
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
export interface ComponentDefinition {
    templateId: string;
    displayName: string;
    inputJson?: string;
    deploymentScopeId?: string;
}
export interface ComponentDataResult {
    code?: number;
    status?: string;
    data?: Component;
    location?: string;
}
export interface StatusResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly state?: string;
    stateMessage?: string;
    location?: string;
    errors?: ResultError[];
    trackingId?: string;
}
export interface ComponentTaskListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ComponentTask[];
    location?: string;
}
export interface ComponentTask {
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
export interface ComponentTaskDefinition {
    taskId: string;
    inputJson?: string;
}
export interface ComponentTaskDataResult {
    code?: number;
    status?: string;
    data?: ComponentTask;
    location?: string;
}
export interface ComponentTemplateListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ComponentTemplate[];
    location?: string;
}
export interface ComponentTemplate {
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
export interface RepositoryReference {
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
export interface ComponentTemplatePermissions {
    none?: string[];
    member?: string[];
    admin?: string[];
    owner?: string[];
    adapter?: string[];
}
export interface ComponentTaskTemplate {
    id?: string;
    displayName?: string;
    description?: string;
    inputJsonSchema?: string;
    type?: ComponentTaskTemplateType;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly typeName?: string;
}
export interface ComponentTaskRunner {
    id?: string;
    /** Dictionary of <string> */
    with?: {
        [propertyName: string]: string;
    };
}
export interface ComponentTemplateDataResult {
    code?: number;
    status?: string;
    data?: ComponentTemplate;
    location?: string;
}
export interface DeploymentScopeListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: DeploymentScope[];
    location?: string;
}
export interface DeploymentScope {
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
export interface DeploymentScopeDefinition {
    displayName: string;
    type: DeploymentScopeDefinitionType;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string;
    inputData?: string;
    isDefault?: boolean;
}
export interface DeploymentScopeDataResult {
    code?: number;
    status?: string;
    data?: DeploymentScope;
    location?: string;
}
export interface CommandAuditEntityListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: CommandAuditEntity[];
    location?: string;
}
export interface CommandAuditEntity {
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
export interface CommandAuditEntityDataResult {
    code?: number;
    status?: string;
    data?: CommandAuditEntity;
    location?: string;
}
export interface StringListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: string[];
    location?: string;
}
export interface OrganizationListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Organization[];
    location?: string;
}
export interface Organization {
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
    galleryId?: string;
    registryId?: string;
    storageId?: string;
    id: string;
}
export interface OrganizationDefinition {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string;
    displayName: string;
    subscriptionId: string;
    location: string;
}
export interface OrganizationDataResult {
    code?: number;
    status?: string;
    data?: Organization;
    location?: string;
}
export interface UserListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: User[];
    location?: string;
}
export interface User {
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
export interface ProjectMembership {
    projectId: string;
    role: ProjectMembershipRole;
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    };
}
export interface UserAlternateIdentities {
    azureResourceManager?: AlternateIdentity;
    azureDevOps?: AlternateIdentity;
    gitHub?: AlternateIdentity;
}
export interface AlternateIdentity {
    login?: string;
}
export interface UserDefinition {
    identifier: string;
    role: string;
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    };
}
export interface UserDataResult {
    code?: number;
    status?: string;
    data?: User;
    location?: string;
}
export interface ProjectListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Project[];
    location?: string;
}
export interface Project {
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
    id: string;
}
export interface ProjectDefinition {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string;
    displayName: string;
    template: string;
    templateInput: string;
    users?: UserDefinition[];
}
export interface ProjectDataResult {
    code?: number;
    status?: string;
    data?: Project;
    location?: string;
}
export interface ProjectIdentityListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ProjectIdentity[];
    location?: string;
}
export interface ProjectIdentity {
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
export interface ProjectIdentityDefinition {
    displayName: string;
    deploymentScopeId: string;
}
export interface ProjectIdentityDataResult {
    code?: number;
    status?: string;
    data?: ProjectIdentity;
    location?: string;
}
export interface StringDictionaryDataResult {
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
export interface ProjectTemplateListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ProjectTemplate[];
    location?: string;
}
export interface ProjectTemplate {
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
export interface ProjectTemplateDefinition {
    displayName: string;
    repository: RepositoryDefinition;
}
export interface RepositoryDefinition {
    url: string;
    token?: string;
    version?: string;
}
export interface ProjectTemplateDataResult {
    code?: number;
    status?: string;
    data?: ProjectTemplate;
    location?: string;
}
export interface ScheduleListDataResult {
    code?: number;
    status?: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Schedule[];
    location?: string;
}
export interface Schedule {
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
export interface ComponentTaskReference {
    componentId?: string;
    componentTaskTemplateId?: string;
    inputJson?: string;
}
export interface ScheduleDefinition {
    enabled?: boolean;
    recurring?: boolean;
    daysOfWeek?: ScheduleDefinitionDaysOfWeekItem[];
    utcHour?: number;
    utcMinute?: number;
    componentTasks?: ComponentTaskReference[];
}
export interface ScheduleDataResult {
    code?: number;
    status?: string;
    data?: Schedule;
    location?: string;
}
/** Known values of {@link AdapterInformationType} that the service accepts. */
export declare enum KnownAdapterInformationType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub"
}
/**
 * Defines values for AdapterInformationType. \
 * {@link KnownAdapterInformationType} can be used interchangeably with AdapterInformationType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub**
 */
export declare type AdapterInformationType = string;
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
/** Known values of {@link ComponentType} that the service accepts. */
export declare enum KnownComponentType {
    Environment = "Environment",
    Repository = "Repository"
}
/**
 * Defines values for ComponentType. \
 * {@link KnownComponentType} can be used interchangeably with ComponentType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Environment** \
 * **Repository**
 */
export declare type ComponentType = string;
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
/** Known values of {@link ComponentTaskType} that the service accepts. */
export declare enum KnownComponentTaskType {
    Custom = "Custom",
    Create = "Create",
    Delete = "Delete"
}
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
/** Known values of {@link ComponentTaskState} that the service accepts. */
export declare enum KnownComponentTaskState {
    Pending = "Pending",
    Initializing = "Initializing",
    Processing = "Processing",
    Succeeded = "Succeeded",
    Failed = "Failed"
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
 * **Failed**
 */
export declare type ComponentTaskState = string;
/** Known values of {@link RepositoryReferenceProvider} that the service accepts. */
export declare enum KnownRepositoryReferenceProvider {
    Unknown = "Unknown",
    GitHub = "GitHub",
    DevOps = "DevOps"
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
/** Known values of {@link RepositoryReferenceType} that the service accepts. */
export declare enum KnownRepositoryReferenceType {
    Unknown = "Unknown",
    Tag = "Tag",
    Branch = "Branch",
    Hash = "Hash"
}
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
/** Known values of {@link ComponentTaskTemplateType} that the service accepts. */
export declare enum KnownComponentTaskTemplateType {
    Custom = "Custom",
    Create = "Create",
    Delete = "Delete"
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
/** Known values of {@link ComponentTemplateType} that the service accepts. */
export declare enum KnownComponentTemplateType {
    Environment = "Environment",
    Repository = "Repository"
}
/**
 * Defines values for ComponentTemplateType. \
 * {@link KnownComponentTemplateType} can be used interchangeably with ComponentTemplateType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Environment** \
 * **Repository**
 */
export declare type ComponentTemplateType = string;
/** Known values of {@link DeploymentScopeType} that the service accepts. */
export declare enum KnownDeploymentScopeType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub"
}
/**
 * Defines values for DeploymentScopeType. \
 * {@link KnownDeploymentScopeType} can be used interchangeably with DeploymentScopeType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub**
 */
export declare type DeploymentScopeType = string;
/** Known values of {@link DeploymentScopeComponentTypesItem} that the service accepts. */
export declare enum KnownDeploymentScopeComponentTypesItem {
    Environment = "Environment",
    Repository = "Repository"
}
/**
 * Defines values for DeploymentScopeComponentTypesItem. \
 * {@link KnownDeploymentScopeComponentTypesItem} can be used interchangeably with DeploymentScopeComponentTypesItem,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Environment** \
 * **Repository**
 */
export declare type DeploymentScopeComponentTypesItem = string;
/** Known values of {@link DeploymentScopeDefinitionType} that the service accepts. */
export declare enum KnownDeploymentScopeDefinitionType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub"
}
/**
 * Defines values for DeploymentScopeDefinitionType. \
 * {@link KnownDeploymentScopeDefinitionType} can be used interchangeably with DeploymentScopeDefinitionType,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub**
 */
export declare type DeploymentScopeDefinitionType = string;
/** Known values of {@link CommandAuditEntityRuntimeStatus} that the service accepts. */
export declare enum KnownCommandAuditEntityRuntimeStatus {
    Unknown = "Unknown",
    Running = "Running",
    Completed = "Completed",
    ContinuedAsNew = "ContinuedAsNew",
    Failed = "Failed",
    Canceled = "Canceled",
    Terminated = "Terminated",
    Pending = "Pending"
}
/**
 * Defines values for CommandAuditEntityRuntimeStatus. \
 * {@link KnownCommandAuditEntityRuntimeStatus} can be used interchangeably with CommandAuditEntityRuntimeStatus,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **Unknown** \
 * **Running** \
 * **Completed** \
 * **ContinuedAsNew** \
 * **Failed** \
 * **Canceled** \
 * **Terminated** \
 * **Pending**
 */
export declare type CommandAuditEntityRuntimeStatus = string;
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
/** Known values of {@link UserType} that the service accepts. */
export declare enum KnownUserType {
    User = "User",
    Group = "Group",
    System = "System",
    Service = "Service"
}
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
/** Known values of {@link UserRole} that the service accepts. */
export declare enum KnownUserRole {
    None = "None",
    Member = "Member",
    Admin = "Admin",
    Owner = "Owner"
}
/**
 * Defines values for UserRole. \
 * {@link KnownUserRole} can be used interchangeably with UserRole,
 *  this enum contains the known values that the service supports.
 * ### Known values supported by the service
 * **None** \
 * **Member** \
 * **Admin** \
 * **Owner**
 */
export declare type UserRole = string;
/** Known values of {@link ProjectMembershipRole} that the service accepts. */
export declare enum KnownProjectMembershipRole {
    None = "None",
    Member = "Member",
    Admin = "Admin",
    Owner = "Owner",
    Adapter = "Adapter"
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
/** Optional parameters. */
export interface TeamCloudGetAdaptersOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getAdapters operation. */
export declare type TeamCloudGetAdaptersResponse = AdapterInformationListDataResult;
/** Optional parameters. */
export interface TeamCloudGetComponentsOptionalParams extends coreClient.OperationOptions {
    deleted?: boolean;
}
/** Contains response data for the getComponents operation. */
export declare type TeamCloudGetComponentsResponse = ComponentListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateComponentOptionalParams extends coreClient.OperationOptions {
    body?: ComponentDefinition;
}
/** Contains response data for the createComponent operation. */
export declare type TeamCloudCreateComponentResponse = ComponentDataResult;
/** Optional parameters. */
export interface TeamCloudGetComponentOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponent operation. */
export declare type TeamCloudGetComponentResponse = ComponentDataResult;
/** Optional parameters. */
export interface TeamCloudDeleteComponentOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteComponent operation. */
export declare type TeamCloudDeleteComponentResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetComponentTasksOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponentTasks operation. */
export declare type TeamCloudGetComponentTasksResponse = ComponentTaskListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateComponentTaskOptionalParams extends coreClient.OperationOptions {
    body?: ComponentTaskDefinition;
}
/** Contains response data for the createComponentTask operation. */
export declare type TeamCloudCreateComponentTaskResponse = ComponentTaskDataResult;
/** Optional parameters. */
export interface TeamCloudGetComponentTaskOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponentTask operation. */
export declare type TeamCloudGetComponentTaskResponse = ComponentTaskDataResult;
/** Optional parameters. */
export interface TeamCloudGetComponentTemplatesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponentTemplates operation. */
export declare type TeamCloudGetComponentTemplatesResponse = ComponentTemplateListDataResult;
/** Optional parameters. */
export interface TeamCloudGetComponentTemplateOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponentTemplate operation. */
export declare type TeamCloudGetComponentTemplateResponse = ComponentTemplateDataResult;
/** Optional parameters. */
export interface TeamCloudGetDeploymentScopesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getDeploymentScopes operation. */
export declare type TeamCloudGetDeploymentScopesResponse = DeploymentScopeListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateDeploymentScopeOptionalParams extends coreClient.OperationOptions {
    body?: DeploymentScopeDefinition;
}
/** Contains response data for the createDeploymentScope operation. */
export declare type TeamCloudCreateDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface TeamCloudGetDeploymentScopeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getDeploymentScope operation. */
export declare type TeamCloudGetDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface TeamCloudUpdateDeploymentScopeOptionalParams extends coreClient.OperationOptions {
    body?: DeploymentScope;
}
/** Contains response data for the updateDeploymentScope operation. */
export declare type TeamCloudUpdateDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface TeamCloudDeleteDeploymentScopeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteDeploymentScope operation. */
export declare type TeamCloudDeleteDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface TeamCloudAuthorizeDeploymentScopeOptionalParams extends coreClient.OperationOptions {
    body?: DeploymentScope;
}
/** Contains response data for the authorizeDeploymentScope operation. */
export declare type TeamCloudAuthorizeDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface TeamCloudNegotiateSignalROptionalParams extends coreClient.OperationOptions {
}
/** Optional parameters. */
export interface TeamCloudGetAuditEntriesOptionalParams extends coreClient.OperationOptions {
    timeRange?: string;
    /** Array of Get1ItemsItem */
    commands?: string[];
}
/** Contains response data for the getAuditEntries operation. */
export declare type TeamCloudGetAuditEntriesResponse = CommandAuditEntityListDataResult;
/** Optional parameters. */
export interface TeamCloudGetAuditEntryOptionalParams extends coreClient.OperationOptions {
    expand?: boolean;
}
/** Contains response data for the getAuditEntry operation. */
export declare type TeamCloudGetAuditEntryResponse = CommandAuditEntityDataResult;
/** Optional parameters. */
export interface TeamCloudGetAuditCommandsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getAuditCommands operation. */
export declare type TeamCloudGetAuditCommandsResponse = StringListDataResult;
/** Optional parameters. */
export interface TeamCloudGetOrganizationsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganizations operation. */
export declare type TeamCloudGetOrganizationsResponse = OrganizationListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateOrganizationOptionalParams extends coreClient.OperationOptions {
    body?: OrganizationDefinition;
}
/** Contains response data for the createOrganization operation. */
export declare type TeamCloudCreateOrganizationResponse = OrganizationDataResult;
/** Optional parameters. */
export interface TeamCloudGetOrganizationOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganization operation. */
export declare type TeamCloudGetOrganizationResponse = OrganizationDataResult;
/** Optional parameters. */
export interface TeamCloudDeleteOrganizationOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteOrganization operation. */
export declare type TeamCloudDeleteOrganizationResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetOrganizationUsersOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganizationUsers operation. */
export declare type TeamCloudGetOrganizationUsersResponse = UserListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateOrganizationUserOptionalParams extends coreClient.OperationOptions {
    body?: UserDefinition;
}
/** Contains response data for the createOrganizationUser operation. */
export declare type TeamCloudCreateOrganizationUserResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudGetOrganizationUserOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganizationUser operation. */
export declare type TeamCloudGetOrganizationUserResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudUpdateOrganizationUserOptionalParams extends coreClient.OperationOptions {
    body?: User;
}
/** Contains response data for the updateOrganizationUser operation. */
export declare type TeamCloudUpdateOrganizationUserResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudDeleteOrganizationUserOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteOrganizationUser operation. */
export declare type TeamCloudDeleteOrganizationUserResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetOrganizationUserMeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganizationUserMe operation. */
export declare type TeamCloudGetOrganizationUserMeResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudUpdateOrganizationUserMeOptionalParams extends coreClient.OperationOptions {
    body?: User;
}
/** Contains response data for the updateOrganizationUserMe operation. */
export declare type TeamCloudUpdateOrganizationUserMeResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudGetProjectsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjects operation. */
export declare type TeamCloudGetProjectsResponse = ProjectListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateProjectOptionalParams extends coreClient.OperationOptions {
    body?: ProjectDefinition;
}
/** Contains response data for the createProject operation. */
export declare type TeamCloudCreateProjectResponse = ProjectDataResult;
/** Optional parameters. */
export interface TeamCloudGetProjectOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProject operation. */
export declare type TeamCloudGetProjectResponse = ProjectDataResult;
/** Optional parameters. */
export interface TeamCloudDeleteProjectOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProject operation. */
export declare type TeamCloudDeleteProjectResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetProjectIdentitiesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectIdentities operation. */
export declare type TeamCloudGetProjectIdentitiesResponse = ProjectIdentityListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateProjectIdentityOptionalParams extends coreClient.OperationOptions {
    body?: ProjectIdentityDefinition;
}
/** Contains response data for the createProjectIdentity operation. */
export declare type TeamCloudCreateProjectIdentityResponse = ProjectIdentityDataResult;
/** Optional parameters. */
export interface TeamCloudGetProjectIdentityOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectIdentity operation. */
export declare type TeamCloudGetProjectIdentityResponse = ProjectIdentityDataResult;
/** Optional parameters. */
export interface TeamCloudUpdateProjectIdentityOptionalParams extends coreClient.OperationOptions {
    body?: ProjectIdentity;
}
/** Contains response data for the updateProjectIdentity operation. */
export declare type TeamCloudUpdateProjectIdentityResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudDeleteProjectIdentityOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProjectIdentity operation. */
export declare type TeamCloudDeleteProjectIdentityResponse = ProjectIdentityDataResult;
/** Optional parameters. */
export interface TeamCloudGetProjectTagsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectTags operation. */
export declare type TeamCloudGetProjectTagsResponse = StringDictionaryDataResult;
/** Optional parameters. */
export interface TeamCloudCreateProjectTagOptionalParams extends coreClient.OperationOptions {
    /** Dictionary of <string> */
    body?: {
        [propertyName: string]: string;
    };
}
/** Contains response data for the createProjectTag operation. */
export declare type TeamCloudCreateProjectTagResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudUpdateProjectTagOptionalParams extends coreClient.OperationOptions {
    /** Dictionary of <string> */
    body?: {
        [propertyName: string]: string;
    };
}
/** Contains response data for the updateProjectTag operation. */
export declare type TeamCloudUpdateProjectTagResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetProjectTagByKeyOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectTagByKey operation. */
export declare type TeamCloudGetProjectTagByKeyResponse = StringDictionaryDataResult;
/** Optional parameters. */
export interface TeamCloudDeleteProjectTagOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProjectTag operation. */
export declare type TeamCloudDeleteProjectTagResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetProjectTemplatesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectTemplates operation. */
export declare type TeamCloudGetProjectTemplatesResponse = ProjectTemplateListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateProjectTemplateOptionalParams extends coreClient.OperationOptions {
    body?: ProjectTemplateDefinition;
}
/** Contains response data for the createProjectTemplate operation. */
export declare type TeamCloudCreateProjectTemplateResponse = ProjectTemplateDataResult;
/** Optional parameters. */
export interface TeamCloudGetProjectTemplateOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectTemplate operation. */
export declare type TeamCloudGetProjectTemplateResponse = ProjectTemplateDataResult;
/** Optional parameters. */
export interface TeamCloudUpdateProjectTemplateOptionalParams extends coreClient.OperationOptions {
    body?: ProjectTemplate;
}
/** Contains response data for the updateProjectTemplate operation. */
export declare type TeamCloudUpdateProjectTemplateResponse = ProjectTemplateDataResult;
/** Optional parameters. */
export interface TeamCloudDeleteProjectTemplateOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProjectTemplate operation. */
export declare type TeamCloudDeleteProjectTemplateResponse = ProjectTemplateDataResult;
/** Optional parameters. */
export interface TeamCloudGetProjectUsersOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectUsers operation. */
export declare type TeamCloudGetProjectUsersResponse = UserListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateProjectUserOptionalParams extends coreClient.OperationOptions {
    body?: UserDefinition;
}
/** Contains response data for the createProjectUser operation. */
export declare type TeamCloudCreateProjectUserResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudGetProjectUserOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectUser operation. */
export declare type TeamCloudGetProjectUserResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudUpdateProjectUserOptionalParams extends coreClient.OperationOptions {
    body?: User;
}
/** Contains response data for the updateProjectUser operation. */
export declare type TeamCloudUpdateProjectUserResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudDeleteProjectUserOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProjectUser operation. */
export declare type TeamCloudDeleteProjectUserResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetProjectUserMeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectUserMe operation. */
export declare type TeamCloudGetProjectUserMeResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudUpdateProjectUserMeOptionalParams extends coreClient.OperationOptions {
    body?: User;
}
/** Contains response data for the updateProjectUserMe operation. */
export declare type TeamCloudUpdateProjectUserMeResponse = UserDataResult;
/** Optional parameters. */
export interface TeamCloudGetSchedulesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getSchedules operation. */
export declare type TeamCloudGetSchedulesResponse = ScheduleListDataResult;
/** Optional parameters. */
export interface TeamCloudCreateScheduleOptionalParams extends coreClient.OperationOptions {
    body?: ScheduleDefinition;
}
/** Contains response data for the createSchedule operation. */
export declare type TeamCloudCreateScheduleResponse = ScheduleDataResult;
/** Optional parameters. */
export interface TeamCloudGetScheduleOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getSchedule operation. */
export declare type TeamCloudGetScheduleResponse = ScheduleDataResult;
/** Optional parameters. */
export interface TeamCloudUpdateScheduleOptionalParams extends coreClient.OperationOptions {
    body?: Schedule;
}
/** Contains response data for the updateSchedule operation. */
export declare type TeamCloudUpdateScheduleResponse = ScheduleDataResult;
/** Optional parameters. */
export interface TeamCloudRunScheduleOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the runSchedule operation. */
export declare type TeamCloudRunScheduleResponse = ScheduleDataResult;
/** Optional parameters. */
export interface TeamCloudGetStatusOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getStatus operation. */
export declare type TeamCloudGetStatusResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetProjectStatusOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectStatus operation. */
export declare type TeamCloudGetProjectStatusResponse = StatusResult;
/** Optional parameters. */
export interface TeamCloudGetUserProjectsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getUserProjects operation. */
export declare type TeamCloudGetUserProjectsResponse = ProjectListDataResult;
/** Optional parameters. */
export interface TeamCloudGetUserProjectsMeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getUserProjectsMe operation. */
export declare type TeamCloudGetUserProjectsMeResponse = ProjectListDataResult;
/** Optional parameters. */
export interface TeamCloudOptionalParams extends coreClient.ServiceClientOptions {
    /** Overrides client endpoint. */
    endpoint?: string;
}
//# sourceMappingURL=index.d.ts.map