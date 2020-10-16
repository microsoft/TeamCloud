import * as coreHttp from "@azure/core-http";
export interface ProjectListDataResult {
    code?: number;
    status?: string;
    readonly data?: Project[];
    location?: string;
}
export interface Project {
    id?: string;
    name?: string;
    type?: ProjectType;
    resourceGroup?: AzureResourceGroup;
    users?: User[];
    /**
     * Dictionary of <string>
     */
    tags?: {
        [propertyName: string]: string;
    };
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
    links?: ProjectReferenceLinks;
}
export interface ProjectType {
    id?: string;
    isDefault?: boolean;
    region?: string;
    subscriptions?: string[];
    subscriptionCapacity?: number;
    resourceGroupNamePrefix?: string;
    providers?: ProviderReference[];
    /**
     * Dictionary of <string>
     */
    tags?: {
        [propertyName: string]: string;
    };
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
}
export interface ProviderReference {
    id?: string;
    dependsOn?: string[];
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
    /**
     * Dictionary of <any>
     */
    metadata?: {
        [propertyName: string]: any;
    };
}
export interface AzureResourceGroup {
    id?: string;
    name?: string;
    subscriptionId?: string;
    region?: string;
}
export interface User {
    id?: string;
    userType?: UserType;
    role?: UserRole;
    projectMemberships?: ProjectMembership[];
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
}
export interface ProjectMembership {
    projectId?: string;
    role?: ProjectMembershipRole;
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
}
export interface ProjectReferenceLinks {
    self?: ReferenceLink;
    identity?: ReferenceLink;
    users?: ReferenceLink;
    links?: ReferenceLink;
    offers?: ReferenceLink;
    components?: ReferenceLink;
}
export interface ReferenceLink {
    href?: string;
    readonly templated?: boolean;
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
export interface ProjectDefinition {
    name?: string;
    projectType?: string;
    users?: UserDefinition[];
    /**
     * Dictionary of <string>
     */
    tags?: {
        [propertyName: string]: string;
    };
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
}
export interface UserDefinition {
    identifier?: string;
    role?: string;
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
}
export interface StatusResult {
    code?: number;
    status?: string;
    readonly state?: string;
    stateMessage?: string;
    location?: string;
    errors?: ResultError[];
    trackingId?: string;
}
export interface ProjectDataResult {
    code?: number;
    status?: string;
    data?: Project;
    location?: string;
}
export interface ComponentListDataResult {
    code?: number;
    status?: string;
    readonly data?: Component[];
    location?: string;
}
export interface Component {
    id?: string;
    href?: string;
    offerId?: string;
    projectId?: string;
    providerId?: string;
    requestedBy?: string;
    displayName?: string;
    description?: string;
    inputJson?: string;
    valueJson?: string;
    scope?: ComponentScope;
    type?: ComponentType;
}
export interface ComponentRequest {
    offerId?: string;
    inputJson?: string;
}
export interface ComponentDataResult {
    code?: number;
    status?: string;
    data?: Component;
    location?: string;
}
export interface ProjectIdentityDataResult {
    code?: number;
    status?: string;
    data?: ProjectIdentity;
    location?: string;
}
export interface ProjectIdentity {
    id?: string;
    tenantId?: string;
    applicationId?: string;
    secret?: string;
}
export interface StringDictionaryDataResult {
    code?: number;
    status?: string;
    /**
     * Dictionary of <string>
     */
    readonly data?: {
        [propertyName: string]: string;
    };
    location?: string;
}
export interface ProjectLink {
    id?: string;
    href?: string;
    title?: string;
    type?: ProjectLinkType;
}
export interface ProjectLinkDataResult {
    code?: number;
    status?: string;
    data?: ProjectLink;
    location?: string;
}
export interface ComponentOfferListDataResult {
    code?: number;
    status?: string;
    readonly data?: ComponentOffer[];
    location?: string;
}
export interface ComponentOffer {
    id?: string;
    providerId?: string;
    displayName?: string;
    description?: string;
    inputJsonSchema?: string;
    scope?: ComponentOfferScope;
    type?: ComponentOfferType;
}
export interface ComponentOfferDataResult {
    code?: number;
    status?: string;
    data?: ComponentOffer;
    location?: string;
}
export interface ProviderDataReturnResult {
    code?: number;
    status?: string;
    data?: ProviderData;
    location?: string;
}
export interface ProviderData {
    id?: string;
    name?: string;
    location?: string;
    isSecret?: boolean;
    isShared?: boolean;
    scope?: ProviderDataScope;
    dataType?: ProviderDataType;
    readonly stringValue?: string;
}
export interface ProjectTypeListDataResult {
    code?: number;
    status?: string;
    readonly data?: ProjectType[];
    location?: string;
}
export interface ProjectTypeDataResult {
    code?: number;
    status?: string;
    data?: ProjectType;
    location?: string;
}
export interface UserListDataResult {
    code?: number;
    status?: string;
    readonly data?: User[];
    location?: string;
}
export interface UserDataResult {
    code?: number;
    status?: string;
    data?: User;
    location?: string;
}
export interface ProviderDataListDataResult {
    code?: number;
    status?: string;
    readonly data?: ProviderData[];
    location?: string;
}
export interface ProviderListDataResult {
    code?: number;
    status?: string;
    readonly data?: Provider[];
    location?: string;
}
export interface Provider {
    id?: string;
    url?: string;
    authCode?: string;
    principalId?: string;
    version?: string;
    resourceGroup?: AzureResourceGroup;
    events?: string[];
    eventSubscriptions?: ProviderEventSubscription[];
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
    registered?: Date;
    type?: ProviderType;
    commandMode?: ProviderCommandMode;
}
export interface ProviderEventSubscription {
    eventType?: string;
}
export interface ProviderDataResult {
    code?: number;
    status?: string;
    data?: Provider;
    location?: string;
}
export interface TeamCloudInstanceDataResult {
    code?: number;
    status?: string;
    data?: TeamCloudInstance;
    location?: string;
}
export interface TeamCloudInstance {
    version?: string;
    resourceGroup?: AzureResourceGroup;
    /**
     * Dictionary of <string>
     */
    tags?: {
        [propertyName: string]: string;
    };
    applications?: TeamCloudApplication[];
}
export interface TeamCloudApplication {
    url?: string;
    version?: string;
    type?: "Web";
    resourceGroup?: AzureResourceGroup;
}
/**
 * Defines values for UserType.
 */
export declare type UserType = "User" | "System" | "Provider" | "Application" | string;
/**
 * Defines values for UserRole.
 */
export declare type UserRole = "None" | "Provider" | "Creator" | "Admin" | string;
/**
 * Defines values for ProjectMembershipRole.
 */
export declare type ProjectMembershipRole = "None" | "Provider" | "Member" | "Owner" | string;
/**
 * Defines values for ResultErrorCode.
 */
export declare type ResultErrorCode = "Unknown" | "Failed" | "Conflict" | "NotFound" | "ServerError" | "ValidationError" | "Unauthorized" | "Forbidden" | string;
/**
 * Defines values for ComponentScope.
 */
export declare type ComponentScope = "System" | "Project" | "All" | string;
/**
 * Defines values for ComponentType.
 */
export declare type ComponentType = "Custom" | "GitRepository" | string;
/**
 * Defines values for ProjectLinkType.
 */
export declare type ProjectLinkType = "Link" | "Readme" | "Service" | "GitRepository" | "AzureResource" | string;
/**
 * Defines values for ComponentOfferScope.
 */
export declare type ComponentOfferScope = "System" | "Project" | "All" | string;
/**
 * Defines values for ComponentOfferType.
 */
export declare type ComponentOfferType = "Custom" | "GitRepository" | string;
/**
 * Defines values for ProviderDataScope.
 */
export declare type ProviderDataScope = "System" | "Project" | string;
/**
 * Defines values for ProviderDataType.
 */
export declare type ProviderDataType = "Property" | "Service" | string;
/**
 * Defines values for ProviderType.
 */
export declare type ProviderType = "Standard" | "Service" | "Virtual" | string;
/**
 * Defines values for ProviderCommandMode.
 */
export declare type ProviderCommandMode = "Simple" | "Extended" | string;
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
 * Optional parameters.
 */
export interface TeamCloudCreateProjectOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectDefinition;
}
/**
 * Contains response data for the createProject operation.
 */
export declare type TeamCloudCreateProjectResponse = StatusResult & {
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
 * Contains response data for the getProjectByNameOrId operation.
 */
export declare type TeamCloudGetProjectByNameOrIdResponse = ProjectDataResult & {
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
 * Optional parameters.
 */
export interface TeamCloudCreateProjectComponentOptionalParams extends coreHttp.OperationOptions {
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
 * Contains response data for the getProjectComponentById operation.
 */
export declare type TeamCloudGetProjectComponentByIdResponse = ComponentDataResult & {
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
 * Contains response data for the getProjectIdentity operation.
 */
export declare type TeamCloudGetProjectIdentityResponse = ProjectIdentityDataResult & {
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
        parsedBody: ProjectIdentityDataResult;
    };
};
/**
 * Contains response data for the getProjectLinks operation.
 */
export declare type TeamCloudGetProjectLinksResponse = StringDictionaryDataResult & {
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
 * Optional parameters.
 */
export interface TeamCloudCreateProjectLinkOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectLink;
}
/**
 * Contains response data for the createProjectLink operation.
 */
export declare type TeamCloudCreateProjectLinkResponse = ProjectLinkDataResult & {
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
        parsedBody: ProjectLinkDataResult;
    };
};
/**
 * Contains response data for the getProjectLinkByKey operation.
 */
export declare type TeamCloudGetProjectLinkByKeyResponse = StringDictionaryDataResult & {
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
 * Optional parameters.
 */
export interface TeamCloudUpdateProjectLinkOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectLink;
}
/**
 * Contains response data for the updateProjectLink operation.
 */
export declare type TeamCloudUpdateProjectLinkResponse = ProjectLinkDataResult & {
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
        parsedBody: ProjectLinkDataResult;
    };
};
/**
 * Contains response data for the deleteProjectLink operation.
 */
export declare type TeamCloudDeleteProjectLinkResponse = StatusResult & {
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
 * Contains response data for the getProjectOffers operation.
 */
export declare type TeamCloudGetProjectOffersResponse = ComponentOfferListDataResult & {
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
        parsedBody: ComponentOfferListDataResult;
    };
};
/**
 * Contains response data for the getProjectOfferById operation.
 */
export declare type TeamCloudGetProjectOfferByIdResponse = ComponentOfferDataResult & {
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
        parsedBody: ComponentOfferDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudGetProjectProviderDataOptionalParams extends coreHttp.OperationOptions {
    includeShared?: boolean;
}
/**
 * Contains response data for the getProjectProviderData operation.
 */
export declare type TeamCloudGetProjectProviderDataResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudCreateProjectProviderDataOptionalParams extends coreHttp.OperationOptions {
    body?: ProviderData;
}
/**
 * Contains response data for the createProjectProviderData operation.
 */
export declare type TeamCloudCreateProjectProviderDataResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
    };
};
/**
 * Contains response data for the getProjectProviderDataById operation.
 */
export declare type TeamCloudGetProjectProviderDataByIdResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudUpdateProjectProviderDataOptionalParams extends coreHttp.OperationOptions {
    body?: ProviderData;
}
/**
 * Contains response data for the updateProjectProviderData operation.
 */
export declare type TeamCloudUpdateProjectProviderDataResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
    };
};
/**
 * Contains response data for the deleteProjectProviderData operation.
 */
export declare type TeamCloudDeleteProjectProviderDataResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
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
 * Optional parameters.
 */
export interface TeamCloudCreateProjectTagOptionalParams extends coreHttp.OperationOptions {
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
export interface TeamCloudUpdateProjectTagOptionalParams extends coreHttp.OperationOptions {
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
 * Contains response data for the getProjectTypes operation.
 */
export declare type TeamCloudGetProjectTypesResponse = ProjectTypeListDataResult & {
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
        parsedBody: ProjectTypeListDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudCreateProjectTypeOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectType;
}
/**
 * Contains response data for the createProjectType operation.
 */
export declare type TeamCloudCreateProjectTypeResponse = ProjectTypeDataResult & {
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
        parsedBody: ProjectTypeDataResult;
    };
};
/**
 * Contains response data for the getProjectTypeById operation.
 */
export declare type TeamCloudGetProjectTypeByIdResponse = ProjectTypeDataResult & {
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
        parsedBody: ProjectTypeDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudUpdateProjectTypeOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectType;
}
/**
 * Contains response data for the updateProjectType operation.
 */
export declare type TeamCloudUpdateProjectTypeResponse = ProjectTypeDataResult & {
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
        parsedBody: ProjectTypeDataResult;
    };
};
/**
 * Contains response data for the deleteProjectType operation.
 */
export declare type TeamCloudDeleteProjectTypeResponse = ProjectTypeDataResult & {
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
        parsedBody: ProjectTypeDataResult;
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
 * Optional parameters.
 */
export interface TeamCloudCreateProjectUserOptionalParams extends coreHttp.OperationOptions {
    body?: UserDefinition;
}
/**
 * Contains response data for the createProjectUser operation.
 */
export declare type TeamCloudCreateProjectUserResponse = StatusResult & {
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
 * Contains response data for the getProjectUserByNameOrId operation.
 */
export declare type TeamCloudGetProjectUserByNameOrIdResponse = UserDataResult & {
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
export interface TeamCloudUpdateProjectUserOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}
/**
 * Contains response data for the updateProjectUser operation.
 */
export declare type TeamCloudUpdateProjectUserResponse = StatusResult & {
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
 * Optional parameters.
 */
export interface TeamCloudUpdateProjectUserMeOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}
/**
 * Contains response data for the updateProjectUserMe operation.
 */
export declare type TeamCloudUpdateProjectUserMeResponse = StatusResult & {
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
 * Contains response data for the getProviderData operation.
 */
export declare type TeamCloudGetProviderDataResponse = ProviderDataListDataResult & {
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
        parsedBody: ProviderDataListDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudCreateProviderDataOptionalParams extends coreHttp.OperationOptions {
    body?: ProviderData;
}
/**
 * Contains response data for the createProviderData operation.
 */
export declare type TeamCloudCreateProviderDataResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
    };
};
/**
 * Contains response data for the getProviderDataById operation.
 */
export declare type TeamCloudGetProviderDataByIdResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudUpdateProviderDataOptionalParams extends coreHttp.OperationOptions {
    body?: ProviderData;
}
/**
 * Contains response data for the updateProviderData operation.
 */
export declare type TeamCloudUpdateProviderDataResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
    };
};
/**
 * Contains response data for the deleteProviderData operation.
 */
export declare type TeamCloudDeleteProviderDataResponse = ProviderDataReturnResult & {
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
        parsedBody: ProviderDataReturnResult;
    };
};
/**
 * Contains response data for the getProviderOffers operation.
 */
export declare type TeamCloudGetProviderOffersResponse = ComponentOfferListDataResult & {
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
        parsedBody: ComponentOfferListDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudCreateProviderOfferOptionalParams extends coreHttp.OperationOptions {
    body?: ComponentOffer;
}
/**
 * Contains response data for the createProviderOffer operation.
 */
export declare type TeamCloudCreateProviderOfferResponse = ComponentOfferDataResult & {
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
        parsedBody: ComponentOfferDataResult;
    };
};
/**
 * Contains response data for the getProviderOfferById operation.
 */
export declare type TeamCloudGetProviderOfferByIdResponse = ComponentOfferDataResult & {
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
        parsedBody: ComponentOfferDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudUpdateProviderOfferOptionalParams extends coreHttp.OperationOptions {
    body?: ComponentOffer;
}
/**
 * Contains response data for the updateProviderOffer operation.
 */
export declare type TeamCloudUpdateProviderOfferResponse = ComponentOfferDataResult & {
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
        parsedBody: ComponentOfferDataResult;
    };
};
/**
 * Contains response data for the deleteProviderOffer operation.
 */
export declare type TeamCloudDeleteProviderOfferResponse = ComponentOfferDataResult & {
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
        parsedBody: ComponentOfferDataResult;
    };
};
/**
 * Contains response data for the getProviderProjectComponents operation.
 */
export declare type TeamCloudGetProviderProjectComponentsResponse = ComponentListDataResult & {
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
 * Optional parameters.
 */
export interface TeamCloudCreateProviderProjectComponentOptionalParams extends coreHttp.OperationOptions {
    body?: Component;
}
/**
 * Contains response data for the createProviderProjectComponent operation.
 */
export declare type TeamCloudCreateProviderProjectComponentResponse = ComponentDataResult & {
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
 * Contains response data for the getProviderProjectComponentById operation.
 */
export declare type TeamCloudGetProviderProjectComponentByIdResponse = ComponentDataResult & {
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
export interface TeamCloudUpdateProviderProjectComponentOptionalParams extends coreHttp.OperationOptions {
    body?: Component;
}
/**
 * Contains response data for the updateProviderProjectComponent operation.
 */
export declare type TeamCloudUpdateProviderProjectComponentResponse = ComponentDataResult & {
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
 * Contains response data for the deleteProviderProjectComponent operation.
 */
export declare type TeamCloudDeleteProviderProjectComponentResponse = StatusResult & {
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
 * Contains response data for the getProviders operation.
 */
export declare type TeamCloudGetProvidersResponse = ProviderListDataResult & {
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
        parsedBody: ProviderListDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudCreateProviderOptionalParams extends coreHttp.OperationOptions {
    body?: Provider;
}
/**
 * Contains response data for the createProvider operation.
 */
export declare type TeamCloudCreateProviderResponse = StatusResult & {
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
 * Contains response data for the getProviderById operation.
 */
export declare type TeamCloudGetProviderByIdResponse = ProviderDataResult & {
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
        parsedBody: ProviderDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudUpdateProviderOptionalParams extends coreHttp.OperationOptions {
    body?: Provider;
}
/**
 * Contains response data for the updateProvider operation.
 */
export declare type TeamCloudUpdateProviderResponse = StatusResult & {
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
 * Contains response data for the deleteProvider operation.
 */
export declare type TeamCloudDeleteProviderResponse = StatusResult & {
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
 * Optional parameters.
 */
export interface TeamCloudCreateTeamCloudAdminUserOptionalParams extends coreHttp.OperationOptions {
    body?: UserDefinition;
}
/**
 * Contains response data for the createTeamCloudAdminUser operation.
 */
export declare type TeamCloudCreateTeamCloudAdminUserResponse = StatusResult & {
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
 * Contains response data for the getTeamCloudInstance operation.
 */
export declare type TeamCloudGetTeamCloudInstanceResponse = TeamCloudInstanceDataResult & {
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
        parsedBody: TeamCloudInstanceDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudCreateTeamCloudInstanceOptionalParams extends coreHttp.OperationOptions {
    body?: TeamCloudInstance;
}
/**
 * Contains response data for the createTeamCloudInstance operation.
 */
export declare type TeamCloudCreateTeamCloudInstanceResponse = TeamCloudInstanceDataResult & {
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
        parsedBody: TeamCloudInstanceDataResult;
    };
};
/**
 * Optional parameters.
 */
export interface TeamCloudUpdateTeamCloudInstanceOptionalParams extends coreHttp.OperationOptions {
    body?: TeamCloudInstance;
}
/**
 * Contains response data for the updateTeamCloudInstance operation.
 */
export declare type TeamCloudUpdateTeamCloudInstanceResponse = TeamCloudInstanceDataResult & {
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
        parsedBody: TeamCloudInstanceDataResult;
    };
};
/**
 * Contains response data for the getTeamCloudTags operation.
 */
export declare type TeamCloudGetTeamCloudTagsResponse = StringDictionaryDataResult & {
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
 * Optional parameters.
 */
export interface TeamCloudCreateTeamCloudTagOptionalParams extends coreHttp.OperationOptions {
    /**
     * Dictionary of <string>
     */
    body?: {
        [propertyName: string]: string;
    };
}
/**
 * Contains response data for the createTeamCloudTag operation.
 */
export declare type TeamCloudCreateTeamCloudTagResponse = StatusResult & {
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
export interface TeamCloudUpdateTeamCloudTagOptionalParams extends coreHttp.OperationOptions {
    /**
     * Dictionary of <string>
     */
    body?: {
        [propertyName: string]: string;
    };
}
/**
 * Contains response data for the updateTeamCloudTag operation.
 */
export declare type TeamCloudUpdateTeamCloudTagResponse = StatusResult & {
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
 * Contains response data for the getTeamCloudTagByKey operation.
 */
export declare type TeamCloudGetTeamCloudTagByKeyResponse = StringDictionaryDataResult & {
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
 * Contains response data for the deleteTeamCloudTag operation.
 */
export declare type TeamCloudDeleteTeamCloudTagResponse = StatusResult & {
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
 * Contains response data for the getTeamCloudUsers operation.
 */
export declare type TeamCloudGetTeamCloudUsersResponse = UserListDataResult & {
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
 * Optional parameters.
 */
export interface TeamCloudCreateTeamCloudUserOptionalParams extends coreHttp.OperationOptions {
    body?: UserDefinition;
}
/**
 * Contains response data for the createTeamCloudUser operation.
 */
export declare type TeamCloudCreateTeamCloudUserResponse = StatusResult & {
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
 * Contains response data for the getTeamCloudUserByNameOrId operation.
 */
export declare type TeamCloudGetTeamCloudUserByNameOrIdResponse = UserDataResult & {
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
export interface TeamCloudUpdateTeamCloudUserOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}
/**
 * Contains response data for the updateTeamCloudUser operation.
 */
export declare type TeamCloudUpdateTeamCloudUserResponse = StatusResult & {
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
 * Contains response data for the deleteTeamCloudUser operation.
 */
export declare type TeamCloudDeleteTeamCloudUserResponse = StatusResult & {
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
 * Contains response data for the getTeamCloudUserMe operation.
 */
export declare type TeamCloudGetTeamCloudUserMeResponse = UserDataResult & {
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
export interface TeamCloudUpdateTeamCloudUserMeOptionalParams extends coreHttp.OperationOptions {
    body?: User;
}
/**
 * Contains response data for the updateTeamCloudUserMe operation.
 */
export declare type TeamCloudUpdateTeamCloudUserMeResponse = StatusResult & {
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
 * Optional parameters.
 */
export interface TeamCloudOptionalParams extends coreHttp.ServiceClientOptions {
    /**
     * Overrides client endpoint.
     */
    endpoint?: string;
}
//# sourceMappingURL=index.d.ts.map