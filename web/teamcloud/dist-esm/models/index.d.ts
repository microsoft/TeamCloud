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
    organizationName: string;
    templateId: string;
    projectId: string;
    projectName: string;
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
    deploymentScopeName?: string;
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
    organizationName: string;
    componentId: string;
    componentName: string;
    projectId: string;
    projectName: string;
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
    organizationName: string;
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
    organizationName: string;
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
    secretsVaultId?: string;
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
    /** Dictionary of <string> */
    tags?: {
        [propertyName: string]: string;
    };
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
    organizationName: string;
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
    kubernetes?: AlternateIdentity;
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
    organizationName: string;
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
    projectName: string;
    organization: string;
    organizationName: string;
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
    organizationName: string;
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
export interface TeamCloudInformationDataResult {
    code?: number;
    status?: string;
    data?: TeamCloudInformation;
    location?: string;
}
export interface TeamCloudInformation {
    imageVersion?: string;
    templateVersion?: string;
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
    organizationName: string;
    projectId: string;
    projectName: string;
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
    componentName?: string;
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
    GitHub = "GitHub",
    Kubernetes = "Kubernetes"
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
    Repository = "Repository",
    Namespace = "Namespace"
}
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
    Canceled = "Canceled",
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
 * **Canceled** \
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
    Repository = "Repository",
    Namespace = "Namespace"
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
/** Known values of {@link DeploymentScopeType} that the service accepts. */
export declare enum KnownDeploymentScopeType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub",
    Kubernetes = "Kubernetes"
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
/** Known values of {@link DeploymentScopeComponentTypesItem} that the service accepts. */
export declare enum KnownDeploymentScopeComponentTypesItem {
    Environment = "Environment",
    Repository = "Repository",
    Namespace = "Namespace"
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
/** Known values of {@link DeploymentScopeDefinitionType} that the service accepts. */
export declare enum KnownDeploymentScopeDefinitionType {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub",
    Kubernetes = "Kubernetes"
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
    Owner = "Owner",
    Adapter = "Adapter"
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
export interface GetAdaptersOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getAdapters operation. */
export declare type GetAdaptersResponse = AdapterInformationListDataResult;
/** Optional parameters. */
export interface GetComponentsOptionalParams extends coreClient.OperationOptions {
    deleted?: boolean;
}
/** Contains response data for the getComponents operation. */
export declare type GetComponentsResponse = ComponentListDataResult;
/** Optional parameters. */
export interface CreateComponentOptionalParams extends coreClient.OperationOptions {
    body?: ComponentDefinition;
}
/** Contains response data for the createComponent operation. */
export declare type CreateComponentResponse = ComponentDataResult;
/** Optional parameters. */
export interface GetComponentOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponent operation. */
export declare type GetComponentResponse = ComponentDataResult;
/** Optional parameters. */
export interface DeleteComponentOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteComponent operation. */
export declare type DeleteComponentResponse = StatusResult;
/** Optional parameters. */
export interface GetComponentTasksOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponentTasks operation. */
export declare type GetComponentTasksResponse = ComponentTaskListDataResult;
/** Optional parameters. */
export interface CreateComponentTaskOptionalParams extends coreClient.OperationOptions {
    body?: ComponentTaskDefinition;
}
/** Contains response data for the createComponentTask operation. */
export declare type CreateComponentTaskResponse = ComponentTaskDataResult;
/** Optional parameters. */
export interface GetComponentTaskOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponentTask operation. */
export declare type GetComponentTaskResponse = ComponentTaskDataResult;
/** Optional parameters. */
export interface CancelComponentTaskOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the cancelComponentTask operation. */
export declare type CancelComponentTaskResponse = ComponentTaskDataResult;
/** Optional parameters. */
export interface ReRunComponentTaskOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the reRunComponentTask operation. */
export declare type ReRunComponentTaskResponse = ComponentTaskDataResult;
/** Optional parameters. */
export interface GetComponentTemplatesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponentTemplates operation. */
export declare type GetComponentTemplatesResponse = ComponentTemplateListDataResult;
/** Optional parameters. */
export interface GetComponentTemplateOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getComponentTemplate operation. */
export declare type GetComponentTemplateResponse = ComponentTemplateDataResult;
/** Optional parameters. */
export interface GetDeploymentScopesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getDeploymentScopes operation. */
export declare type GetDeploymentScopesResponse = DeploymentScopeListDataResult;
/** Optional parameters. */
export interface CreateDeploymentScopeOptionalParams extends coreClient.OperationOptions {
    body?: DeploymentScopeDefinition;
}
/** Contains response data for the createDeploymentScope operation. */
export declare type CreateDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface GetDeploymentScopeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getDeploymentScope operation. */
export declare type GetDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface UpdateDeploymentScopeOptionalParams extends coreClient.OperationOptions {
    body?: DeploymentScope;
}
/** Contains response data for the updateDeploymentScope operation. */
export declare type UpdateDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface DeleteDeploymentScopeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteDeploymentScope operation. */
export declare type DeleteDeploymentScopeResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface InitializeAuthorizationOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the initializeAuthorization operation. */
export declare type InitializeAuthorizationResponse = DeploymentScopeDataResult;
/** Optional parameters. */
export interface NegotiateSignalROptionalParams extends coreClient.OperationOptions {
}
/** Optional parameters. */
export interface GetAuditEntriesOptionalParams extends coreClient.OperationOptions {
    timeRange?: string;
    /** Array of Get1ItemsItem */
    commands?: string[];
}
/** Contains response data for the getAuditEntries operation. */
export declare type GetAuditEntriesResponse = CommandAuditEntityListDataResult;
/** Optional parameters. */
export interface GetAuditEntryOptionalParams extends coreClient.OperationOptions {
    expand?: boolean;
}
/** Contains response data for the getAuditEntry operation. */
export declare type GetAuditEntryResponse = CommandAuditEntityDataResult;
/** Optional parameters. */
export interface GetAuditCommandsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getAuditCommands operation. */
export declare type GetAuditCommandsResponse = StringListDataResult;
/** Optional parameters. */
export interface GetOrganizationsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganizations operation. */
export declare type GetOrganizationsResponse = OrganizationListDataResult;
/** Optional parameters. */
export interface CreateOrganizationOptionalParams extends coreClient.OperationOptions {
    body?: OrganizationDefinition;
}
/** Contains response data for the createOrganization operation. */
export declare type CreateOrganizationResponse = OrganizationDataResult;
/** Optional parameters. */
export interface GetOrganizationOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganization operation. */
export declare type GetOrganizationResponse = OrganizationDataResult;
/** Optional parameters. */
export interface DeleteOrganizationOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteOrganization operation. */
export declare type DeleteOrganizationResponse = StatusResult;
/** Optional parameters. */
export interface GetOrganizationUsersOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganizationUsers operation. */
export declare type GetOrganizationUsersResponse = UserListDataResult;
/** Optional parameters. */
export interface CreateOrganizationUserOptionalParams extends coreClient.OperationOptions {
    body?: UserDefinition;
}
/** Contains response data for the createOrganizationUser operation. */
export declare type CreateOrganizationUserResponse = UserDataResult;
/** Optional parameters. */
export interface GetOrganizationUserOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganizationUser operation. */
export declare type GetOrganizationUserResponse = UserDataResult;
/** Optional parameters. */
export interface UpdateOrganizationUserOptionalParams extends coreClient.OperationOptions {
    body?: User;
}
/** Contains response data for the updateOrganizationUser operation. */
export declare type UpdateOrganizationUserResponse = UserDataResult;
/** Optional parameters. */
export interface DeleteOrganizationUserOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteOrganizationUser operation. */
export declare type DeleteOrganizationUserResponse = StatusResult;
/** Optional parameters. */
export interface GetOrganizationUserMeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getOrganizationUserMe operation. */
export declare type GetOrganizationUserMeResponse = UserDataResult;
/** Optional parameters. */
export interface UpdateOrganizationUserMeOptionalParams extends coreClient.OperationOptions {
    body?: User;
}
/** Contains response data for the updateOrganizationUserMe operation. */
export declare type UpdateOrganizationUserMeResponse = UserDataResult;
/** Optional parameters. */
export interface GetProjectsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjects operation. */
export declare type GetProjectsResponse = ProjectListDataResult;
/** Optional parameters. */
export interface CreateProjectOptionalParams extends coreClient.OperationOptions {
    body?: ProjectDefinition;
}
/** Contains response data for the createProject operation. */
export declare type CreateProjectResponse = ProjectDataResult;
/** Optional parameters. */
export interface GetProjectOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProject operation. */
export declare type GetProjectResponse = ProjectDataResult;
/** Optional parameters. */
export interface DeleteProjectOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProject operation. */
export declare type DeleteProjectResponse = StatusResult;
/** Optional parameters. */
export interface GetProjectIdentitiesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectIdentities operation. */
export declare type GetProjectIdentitiesResponse = ProjectIdentityListDataResult;
/** Optional parameters. */
export interface CreateProjectIdentityOptionalParams extends coreClient.OperationOptions {
    body?: ProjectIdentityDefinition;
}
/** Contains response data for the createProjectIdentity operation. */
export declare type CreateProjectIdentityResponse = ProjectIdentityDataResult;
/** Optional parameters. */
export interface GetProjectIdentityOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectIdentity operation. */
export declare type GetProjectIdentityResponse = ProjectIdentityDataResult;
/** Optional parameters. */
export interface UpdateProjectIdentityOptionalParams extends coreClient.OperationOptions {
    body?: ProjectIdentity;
}
/** Contains response data for the updateProjectIdentity operation. */
export declare type UpdateProjectIdentityResponse = StatusResult;
/** Optional parameters. */
export interface DeleteProjectIdentityOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProjectIdentity operation. */
export declare type DeleteProjectIdentityResponse = ProjectIdentityDataResult;
/** Optional parameters. */
export interface GetProjectTagsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectTags operation. */
export declare type GetProjectTagsResponse = StringDictionaryDataResult;
/** Optional parameters. */
export interface CreateProjectTagOptionalParams extends coreClient.OperationOptions {
    /** Dictionary of <string> */
    body?: {
        [propertyName: string]: string;
    };
}
/** Contains response data for the createProjectTag operation. */
export declare type CreateProjectTagResponse = StatusResult;
/** Optional parameters. */
export interface UpdateProjectTagOptionalParams extends coreClient.OperationOptions {
    /** Dictionary of <string> */
    body?: {
        [propertyName: string]: string;
    };
}
/** Contains response data for the updateProjectTag operation. */
export declare type UpdateProjectTagResponse = StatusResult;
/** Optional parameters. */
export interface GetProjectTagByKeyOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectTagByKey operation. */
export declare type GetProjectTagByKeyResponse = StringDictionaryDataResult;
/** Optional parameters. */
export interface DeleteProjectTagOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProjectTag operation. */
export declare type DeleteProjectTagResponse = StatusResult;
/** Optional parameters. */
export interface GetProjectTemplatesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectTemplates operation. */
export declare type GetProjectTemplatesResponse = ProjectTemplateListDataResult;
/** Optional parameters. */
export interface CreateProjectTemplateOptionalParams extends coreClient.OperationOptions {
    body?: ProjectTemplateDefinition;
}
/** Contains response data for the createProjectTemplate operation. */
export declare type CreateProjectTemplateResponse = ProjectTemplateDataResult;
/** Optional parameters. */
export interface GetProjectTemplateOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectTemplate operation. */
export declare type GetProjectTemplateResponse = ProjectTemplateDataResult;
/** Optional parameters. */
export interface UpdateProjectTemplateOptionalParams extends coreClient.OperationOptions {
    body?: ProjectTemplate;
}
/** Contains response data for the updateProjectTemplate operation. */
export declare type UpdateProjectTemplateResponse = ProjectTemplateDataResult;
/** Optional parameters. */
export interface DeleteProjectTemplateOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProjectTemplate operation. */
export declare type DeleteProjectTemplateResponse = ProjectTemplateDataResult;
/** Optional parameters. */
export interface GetProjectUsersOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectUsers operation. */
export declare type GetProjectUsersResponse = UserListDataResult;
/** Optional parameters. */
export interface CreateProjectUserOptionalParams extends coreClient.OperationOptions {
    body?: UserDefinition;
}
/** Contains response data for the createProjectUser operation. */
export declare type CreateProjectUserResponse = UserDataResult;
/** Optional parameters. */
export interface GetProjectUserOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectUser operation. */
export declare type GetProjectUserResponse = UserDataResult;
/** Optional parameters. */
export interface UpdateProjectUserOptionalParams extends coreClient.OperationOptions {
    body?: User;
}
/** Contains response data for the updateProjectUser operation. */
export declare type UpdateProjectUserResponse = UserDataResult;
/** Optional parameters. */
export interface DeleteProjectUserOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the deleteProjectUser operation. */
export declare type DeleteProjectUserResponse = StatusResult;
/** Optional parameters. */
export interface GetProjectUserMeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectUserMe operation. */
export declare type GetProjectUserMeResponse = UserDataResult;
/** Optional parameters. */
export interface UpdateProjectUserMeOptionalParams extends coreClient.OperationOptions {
    body?: User;
}
/** Contains response data for the updateProjectUserMe operation. */
export declare type UpdateProjectUserMeResponse = UserDataResult;
/** Optional parameters. */
export interface GetInfoOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getInfo operation. */
export declare type GetInfoResponse = TeamCloudInformationDataResult;
/** Optional parameters. */
export interface GetSchedulesOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getSchedules operation. */
export declare type GetSchedulesResponse = ScheduleListDataResult;
/** Optional parameters. */
export interface CreateScheduleOptionalParams extends coreClient.OperationOptions {
    body?: ScheduleDefinition;
}
/** Contains response data for the createSchedule operation. */
export declare type CreateScheduleResponse = ScheduleDataResult;
/** Optional parameters. */
export interface GetScheduleOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getSchedule operation. */
export declare type GetScheduleResponse = ScheduleDataResult;
/** Optional parameters. */
export interface UpdateScheduleOptionalParams extends coreClient.OperationOptions {
    body?: Schedule;
}
/** Contains response data for the updateSchedule operation. */
export declare type UpdateScheduleResponse = ScheduleDataResult;
/** Optional parameters. */
export interface RunScheduleOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the runSchedule operation. */
export declare type RunScheduleResponse = ScheduleDataResult;
/** Optional parameters. */
export interface GetStatusOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getStatus operation. */
export declare type GetStatusResponse = StatusResult;
/** Optional parameters. */
export interface GetProjectStatusOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getProjectStatus operation. */
export declare type GetProjectStatusResponse = StatusResult;
/** Optional parameters. */
export interface GetUserProjectsOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getUserProjects operation. */
export declare type GetUserProjectsResponse = ProjectListDataResult;
/** Optional parameters. */
export interface GetUserProjectsMeOptionalParams extends coreClient.OperationOptions {
}
/** Contains response data for the getUserProjectsMe operation. */
export declare type GetUserProjectsMeResponse = ProjectListDataResult;
/** Optional parameters. */
export interface TeamCloudOptionalParams extends coreClient.ServiceClientOptions {
    /** Overrides client endpoint. */
    endpoint?: string;
}
//# sourceMappingURL=index.d.ts.map