import * as coreHttp from "@azure/core-http";
export interface ComponentListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Component[] | null;
    location?: string | null;
}
export interface Component {
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
export interface ErrorResult {
    code?: number;
    status?: string | null;
    errors?: ResultError[] | null;
}
export interface ResultError {
    code?: ResultErrorCode;
    message?: string | null;
    errors?: ValidationError[] | null;
}
export interface ValidationError {
    field?: string | null;
    message?: string | null;
}
export interface ComponentDefinition {
    templateId: string;
    displayName: string;
    inputJson?: string | null;
    deploymentScopeId?: string | null;
}
export interface ComponentDataResult {
    code?: number;
    status?: string | null;
    data?: Component;
    location?: string | null;
}
export interface StatusResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly state?: string | null;
    stateMessage?: string | null;
    location?: string | null;
    errors?: ResultError[] | null;
    trackingId?: string | null;
}
export interface ComponentTaskListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ComponentTask[] | null;
    location?: string | null;
}
export interface ComponentTask {
    organization: string;
    componentId: string;
    projectId: string;
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
export interface ComponentTaskDefinition {
    taskId: string;
    inputJson?: string | null;
}
export interface ComponentTaskDataResult {
    code?: number;
    status?: string | null;
    data?: ComponentTask;
    location?: string | null;
}
export interface ComponentTemplateListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ComponentTemplate[] | null;
    location?: string | null;
}
export interface ComponentTemplate {
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
    id: string;
}
export interface RepositoryReference {
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
export interface ComponentTemplatePermissions {
    none?: string[];
    member?: string[];
    admin?: string[];
    owner?: string[];
}
export interface ComponentTaskTemplate {
    id?: string | null;
    displayName?: string | null;
    description?: string | null;
    inputJsonSchema?: string | null;
    type: Enum3;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly typeName?: string | null;
}
export interface ComponentTaskRunner {
    id?: string | null;
    /** Dictionary of <string> */
    with?: {
        [propertyName: string]: string;
    } | null;
}
export interface ComponentTemplateDataResult {
    code?: number;
    status?: string | null;
    data?: ComponentTemplate;
    location?: string | null;
}
export interface DeploymentScopeListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: DeploymentScope[] | null;
    location?: string | null;
}
export interface DeploymentScope {
    organization: string;
    displayName: string;
    slug: string;
    isDefault: boolean;
    adapter?: DeploymentScopeAdapter;
    managementGroupId?: string | null;
    subscriptionIds?: string[] | null;
    authorizeUrl?: string | null;
    id: string;
}
export interface DeploymentScopeDefinition {
    displayName: string;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string | null;
    isDefault?: boolean;
    managementGroupId?: string | null;
    subscriptionIds?: string[] | null;
}
export interface DeploymentScopeDataResult {
    code?: number;
    status?: string | null;
    data?: DeploymentScope;
    location?: string | null;
}
export interface OrganizationListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Organization[] | null;
    location?: string | null;
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
    } | null;
    resourceId?: string | null;
    resourceState?: OrganizationResourceState;
    galleryId?: string | null;
    registryId?: string | null;
    storageId?: string | null;
    id: string;
}
export interface OrganizationDefinition {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string | null;
    displayName: string;
    subscriptionId: string;
    location: string;
}
export interface OrganizationDataResult {
    code?: number;
    status?: string | null;
    data?: Organization;
    location?: string | null;
}
export interface UserListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: User[] | null;
    location?: string | null;
}
export interface User {
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
export interface ProjectMembership {
    projectId: string;
    role: ProjectMembershipRole;
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    } | null;
}
export interface UserDefinition {
    identifier: string;
    role: string;
    /** Dictionary of <string> */
    properties?: {
        [propertyName: string]: string;
    } | null;
}
export interface UserDataResult {
    code?: number;
    status?: string | null;
    data?: User;
    location?: string | null;
}
export interface ProjectListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: Project[] | null;
    location?: string | null;
}
export interface Project {
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
export interface ProjectDefinition {
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly slug?: string | null;
    displayName: string;
    template: string;
    templateInput: string;
    users?: UserDefinition[] | null;
}
export interface ProjectDataResult {
    code?: number;
    status?: string | null;
    data?: Project;
    location?: string | null;
}
export interface ProjectIdentityListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ProjectIdentity[] | null;
    location?: string | null;
}
export interface ProjectIdentity {
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
export interface ProjectIdentityDefinition {
    displayName: string;
    deploymentScopeId: string;
}
export interface ProjectIdentityDataResult {
    code?: number;
    status?: string | null;
    data?: ProjectIdentity;
    location?: string | null;
}
export interface StringDictionaryDataResult {
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
export interface ProjectTemplateListDataResult {
    code?: number;
    status?: string | null;
    /** NOTE: This property will not be serialized. It can only be populated by the server. */
    readonly data?: ProjectTemplate[] | null;
    location?: string | null;
}
export interface ProjectTemplate {
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
export interface ProjectTemplateDefinition {
    displayName: string;
    repository: RepositoryDefinition;
}
export interface RepositoryDefinition {
    url: string;
    token?: string | null;
    version?: string | null;
}
export interface ProjectTemplateDataResult {
    code?: number;
    status?: string | null;
    data?: ProjectTemplate;
    location?: string | null;
}
/** Known values of {@link ComponentType} that the service accepts. */
export declare const enum KnownComponentType {
    Custom = "Custom",
    AzureResource = "AzureResource",
    Environment = "Environment",
    GitRepository = "GitRepository"
}
/**
 * Defines values for ComponentType. \
 * {@link KnownComponentType} can be used interchangeably with ComponentType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Custom** \
 * **AzureResource** \
 * **Environment** \
 * **GitRepository**
 */
export declare type ComponentType = string;
/** Known values of {@link ComponentResourceState} that the service accepts. */
export declare const enum KnownComponentResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Succeeded = "Succeeded",
    Failed = "Failed"
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
/** Known values of {@link Enum3} that the service accepts. */
export declare const enum KnownEnum3 {
    Zero = 0,
    One = 1,
    Two = 2
}
/**
 * Defines values for Enum3. \
 * {@link KnownEnum3} can be used interchangeably with Enum3,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **0** \
 * **1** \
 * **2**
 */
export declare type Enum3 = number;
/** Known values of {@link ComponentTaskResourceState} that the service accepts. */
export declare const enum KnownComponentTaskResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Succeeded = "Succeeded",
    Failed = "Failed"
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
/** Known values of {@link RepositoryReferenceProvider} that the service accepts. */
export declare const enum KnownRepositoryReferenceProvider {
    Unknown = "Unknown",
    GitHub = "GitHub",
    DevOps = "DevOps"
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
/** Known values of {@link RepositoryReferenceType} that the service accepts. */
export declare const enum KnownRepositoryReferenceType {
    Unknown = "Unknown",
    Tag = "Tag",
    Branch = "Branch",
    Hash = "Hash"
}
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
/** Known values of {@link ComponentTemplateType} that the service accepts. */
export declare const enum KnownComponentTemplateType {
    Custom = "Custom",
    AzureResource = "AzureResource",
    Environment = "Environment",
    GitRepository = "GitRepository"
}
/**
 * Defines values for ComponentTemplateType. \
 * {@link KnownComponentTemplateType} can be used interchangeably with ComponentTemplateType,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **Custom** \
 * **AzureResource** \
 * **Environment** \
 * **GitRepository**
 */
export declare type ComponentTemplateType = string;
/** Known values of {@link DeploymentScopeAdapter} that the service accepts. */
export declare const enum KnownDeploymentScopeAdapter {
    AzureResourceManager = "AzureResourceManager",
    AzureDevOps = "AzureDevOps",
    GitHub = "GitHub"
}
/**
 * Defines values for DeploymentScopeAdapter. \
 * {@link KnownDeploymentScopeAdapter} can be used interchangeably with DeploymentScopeAdapter,
 *  this enum contains the known values that the service supports.
 * ### Know values supported by the service
 * **AzureResourceManager** \
 * **AzureDevOps** \
 * **GitHub**
 */
export declare type DeploymentScopeAdapter = string;
/** Known values of {@link OrganizationResourceState} that the service accepts. */
export declare const enum KnownOrganizationResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Succeeded = "Succeeded",
    Failed = "Failed"
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
/** Known values of {@link UserType} that the service accepts. */
export declare const enum KnownUserType {
    User = "User",
    System = "System",
    Provider = "Provider",
    Application = "Application"
}
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
/** Known values of {@link UserRole} that the service accepts. */
export declare const enum KnownUserRole {
    None = "None",
    Member = "Member",
    Admin = "Admin",
    Owner = "Owner"
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
/** Known values of {@link ProjectMembershipRole} that the service accepts. */
export declare const enum KnownProjectMembershipRole {
    None = "None",
    Member = "Member",
    Admin = "Admin",
    Owner = "Owner"
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
/** Known values of {@link ProjectResourceState} that the service accepts. */
export declare const enum KnownProjectResourceState {
    Pending = "Pending",
    Initializing = "Initializing",
    Provisioning = "Provisioning",
    Succeeded = "Succeeded",
    Failed = "Failed"
}
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
/** Optional parameters. */
export interface TeamCloudGetComponentsOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateComponentOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateComponentTaskOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateDeploymentScopeOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudUpdateDeploymentScopeOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudAuthorizeDeploymentScopeOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateOrganizationOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateOrganizationUserOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudUpdateOrganizationUserOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudUpdateOrganizationUserMeOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateProjectOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateProjectIdentityOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudUpdateProjectIdentityOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateProjectTagOptionalParams extends coreHttp.OperationOptions {
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
export interface TeamCloudUpdateProjectTagOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateProjectTemplateOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudUpdateProjectTemplateOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudCreateProjectUserOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudUpdateProjectUserOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudUpdateProjectUserMeOptionalParams extends coreHttp.OperationOptions {
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
/** Optional parameters. */
export interface TeamCloudOptionalParams extends coreHttp.ServiceClientOptions {
    /** Overrides client endpoint. */
    endpoint?: string;
}
//# sourceMappingURL=index.d.ts.map