import * as coreHttp from '@azure/core-http';

export declare interface AzureResourceGroup {
    id?: string;
    name?: string;
    subscriptionId?: string;
    region?: string;
}

export declare interface Component {
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

export declare interface ComponentDataResult {
    code?: number;
    status?: string;
    data?: Component;
    location?: string;
}

export declare interface ComponentListDataResult {
    code?: number;
    status?: string;
    readonly data?: Component[];
    location?: string;
}

export declare interface ComponentOffer {
    id?: string;
    providerId?: string;
    displayName?: string;
    description?: string;
    inputJsonSchema?: string;
    scope?: ComponentOfferScope;
    type?: ComponentOfferType;
}

export declare interface ComponentOfferDataResult {
    code?: number;
    status?: string;
    data?: ComponentOffer;
    location?: string;
}

export declare interface ComponentOfferListDataResult {
    code?: number;
    status?: string;
    readonly data?: ComponentOffer[];
    location?: string;
}

/**
 * Defines values for ComponentOfferScope.
 */
export declare type ComponentOfferScope = "System" | "Project" | "All" | string;

/**
 * Defines values for ComponentOfferType.
 */
export declare type ComponentOfferType = "Custom" | "GitRepository" | string;

export declare interface ComponentRequest {
    offerId?: string;
    inputJson?: string;
}

/**
 * Defines values for ComponentScope.
 */
export declare type ComponentScope = "System" | "Project" | "All" | string;

/**
 * Defines values for ComponentType.
 */
export declare type ComponentType = "Custom" | "GitRepository" | string;

export declare interface ErrorResult {
    code?: number;
    status?: string;
    errors?: ResultError[];
}

export declare interface Project {
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

export declare interface ProjectDataResult {
    code?: number;
    status?: string;
    data?: Project;
    location?: string;
}

export declare interface ProjectDefinition {
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

export declare interface ProjectIdentity {
    id?: string;
    tenantId?: string;
    applicationId?: string;
    secret?: string;
}

export declare interface ProjectIdentityDataResult {
    code?: number;
    status?: string;
    data?: ProjectIdentity;
    location?: string;
}

export declare interface ProjectLink {
    id?: string;
    href?: string;
    title?: string;
    type?: ProjectLinkType;
}

export declare interface ProjectLinkDataResult {
    code?: number;
    status?: string;
    data?: ProjectLink;
    location?: string;
}

/**
 * Defines values for ProjectLinkType.
 */
export declare type ProjectLinkType = "Link" | "Readme" | "Service" | "GitRepository" | "AzureResource" | string;

export declare interface ProjectListDataResult {
    code?: number;
    status?: string;
    readonly data?: Project[];
    location?: string;
}

export declare interface ProjectMembership {
    projectId?: string;
    role?: ProjectMembershipRole;
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
}

/**
 * Defines values for ProjectMembershipRole.
 */
export declare type ProjectMembershipRole = "None" | "Provider" | "Member" | "Owner" | string;

export declare interface ProjectReferenceLinks {
    self?: ReferenceLink;
    identity?: ReferenceLink;
    users?: ReferenceLink;
    links?: ReferenceLink;
    offers?: ReferenceLink;
    components?: ReferenceLink;
}

export declare interface ProjectType {
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

export declare interface ProjectTypeDataResult {
    code?: number;
    status?: string;
    data?: ProjectType;
    location?: string;
}

export declare interface ProjectTypeListDataResult {
    code?: number;
    status?: string;
    readonly data?: ProjectType[];
    location?: string;
}

export declare interface Provider {
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

/**
 * Defines values for ProviderCommandMode.
 */
export declare type ProviderCommandMode = "Simple" | "Extended" | string;

export declare interface ProviderData {
    id?: string;
    name?: string;
    location?: string;
    isSecret?: boolean;
    isShared?: boolean;
    scope?: ProviderDataScope;
    dataType?: ProviderDataType;
    readonly stringValue?: string;
}

export declare interface ProviderDataListDataResult {
    code?: number;
    status?: string;
    readonly data?: ProviderData[];
    location?: string;
}

export declare interface ProviderDataResult {
    code?: number;
    status?: string;
    data?: Provider;
    location?: string;
}

export declare interface ProviderDataReturnResult {
    code?: number;
    status?: string;
    data?: ProviderData;
    location?: string;
}

/**
 * Defines values for ProviderDataScope.
 */
export declare type ProviderDataScope = "System" | "Project" | string;

/**
 * Defines values for ProviderDataType.
 */
export declare type ProviderDataType = "Property" | "Service" | string;

export declare interface ProviderEventSubscription {
    eventType?: string;
}

export declare interface ProviderListDataResult {
    code?: number;
    status?: string;
    readonly data?: Provider[];
    location?: string;
}

export declare interface ProviderReference {
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

/**
 * Defines values for ProviderType.
 */
export declare type ProviderType = "Standard" | "Service" | "Virtual" | string;

export declare interface ReferenceLink {
    href?: string;
    readonly templated?: boolean;
}

export declare interface ResultError {
    code?: ResultErrorCode;
    message?: string;
    errors?: ValidationError[];
}

/**
 * Defines values for ResultErrorCode.
 */
export declare type ResultErrorCode = "Unknown" | "Failed" | "Conflict" | "NotFound" | "ServerError" | "ValidationError" | "Unauthorized" | "Forbidden" | string;

export declare interface StatusResult {
    code?: number;
    status?: string;
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
     */
    readonly data?: {
        [propertyName: string]: string;
    };
    location?: string;
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
     * Gets all Projects.
     * @param options The options parameters.
     */
    getProjects(options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectsResponse>;
    /**
     * Creates a new Project.
     * @param options The options parameters.
     */
    createProject(options?: TeamCloudCreateProjectOptionalParams): Promise<TeamCloudCreateProjectResponse>;
    /**
     * Gets a Project by Name or ID.
     * @param projectNameOrId
     * @param options The options parameters.
     */
    getProjectByNameOrId(projectNameOrId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectByNameOrIdResponse>;
    /**
     * Deletes a Project.
     * @param projectNameOrId
     * @param options The options parameters.
     */
    deleteProject(projectNameOrId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectResponse>;
    /**
     * Gets all Components for a Project.
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponents(projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentsResponse>;
    /**
     * Creates a new Project Component.
     * @param projectId
     * @param options The options parameters.
     */
    createProjectComponent(projectId: string, options?: TeamCloudCreateProjectComponentOptionalParams): Promise<TeamCloudCreateProjectComponentResponse>;
    /**
     * Gets a Project Component by id.
     * @param componentId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectComponentById(componentId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectComponentByIdResponse>;
    /**
     * Deletes an existing Project Component.
     * @param componentId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectComponent(componentId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectComponentResponse>;
    /**
     * Gets the ProjectIdentity for a Project.
     * @param projectId
     * @param options The options parameters.
     */
    getProjectIdentity(projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectIdentityResponse>;
    /**
     * Gets all Links for a Project.
     * @param projectId
     * @param options The options parameters.
     */
    getProjectLinks(projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectLinksResponse>;
    /**
     * Creates a new Project Link.
     * @param projectId
     * @param options The options parameters.
     */
    createProjectLink(projectId: string, options?: TeamCloudCreateProjectLinkOptionalParams): Promise<TeamCloudCreateProjectLinkResponse>;
    /**
     * Gets a Project Link by Key.
     * @param linkId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectLinkByKey(linkId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectLinkByKeyResponse>;
    /**
     * Updates an existing Project Link.
     * @param linkId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectLink(linkId: string, projectId: string, options?: TeamCloudUpdateProjectLinkOptionalParams): Promise<TeamCloudUpdateProjectLinkResponse>;
    /**
     * Deletes an existing Project Link.
     * @param linkId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectLink(linkId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectLinkResponse>;
    /**
     * Gets all Project Offers.
     * @param projectId
     * @param options The options parameters.
     */
    getProjectOffers(projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectOffersResponse>;
    /**
     * Gets the Offer by id.
     * @param offerId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectOfferById(offerId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectOfferByIdResponse>;
    /**
     * Gets the ProviderData items for a Project.
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    getProjectProviderData(projectId: string, providerId: string, options?: TeamCloudGetProjectProviderDataOptionalParams): Promise<TeamCloudGetProjectProviderDataResponse>;
    /**
     * Creates a new ProviderData
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    createProjectProviderData(projectId: string, providerId: string, options?: TeamCloudCreateProjectProviderDataOptionalParams): Promise<TeamCloudCreateProjectProviderDataResponse>;
    /**
     * Gets a ProviderData for a Project by ID.
     * @param providerDataId
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    getProjectProviderDataById(providerDataId: string, projectId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectProviderDataByIdResponse>;
    /**
     * Updates an existing ProviderData.
     * @param providerDataId
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    updateProjectProviderData(providerDataId: string, projectId: string, providerId: string, options?: TeamCloudUpdateProjectProviderDataOptionalParams): Promise<TeamCloudUpdateProjectProviderDataResponse>;
    /**
     * Deletes a ProviderData.
     * @param providerDataId
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    deleteProjectProviderData(providerDataId: string, projectId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectProviderDataResponse>;
    /**
     * Gets all Tags for a Project.
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTags(projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTagsResponse>;
    /**
     * Creates a new Project Tag.
     * @param projectId
     * @param options The options parameters.
     */
    createProjectTag(projectId: string, options?: TeamCloudCreateProjectTagOptionalParams): Promise<TeamCloudCreateProjectTagResponse>;
    /**
     * Updates an existing Project Tag.
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectTag(projectId: string, options?: TeamCloudUpdateProjectTagOptionalParams): Promise<TeamCloudUpdateProjectTagResponse>;
    /**
     * Gets a Project Tag by Key.
     * @param tagKey
     * @param projectId
     * @param options The options parameters.
     */
    getProjectTagByKey(tagKey: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTagByKeyResponse>;
    /**
     * Deletes an existing Project Tag.
     * @param tagKey
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectTag(tagKey: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectTagResponse>;
    /**
     * Gets all Project Types.
     * @param options The options parameters.
     */
    getProjectTypes(options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTypesResponse>;
    /**
     * Creates a new Project Type.
     * @param options The options parameters.
     */
    createProjectType(options?: TeamCloudCreateProjectTypeOptionalParams): Promise<TeamCloudCreateProjectTypeResponse>;
    /**
     * Gets a Project Type by ID.
     * @param projectTypeId
     * @param options The options parameters.
     */
    getProjectTypeById(projectTypeId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectTypeByIdResponse>;
    /**
     * Updates an existing Project Type.
     * @param projectTypeId
     * @param options The options parameters.
     */
    updateProjectType(projectTypeId: string, options?: TeamCloudUpdateProjectTypeOptionalParams): Promise<TeamCloudUpdateProjectTypeResponse>;
    /**
     * Deletes a Project Type.
     * @param projectTypeId
     * @param options The options parameters.
     */
    deleteProjectType(projectTypeId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectTypeResponse>;
    /**
     * Gets all Users for a Project.
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUsers(projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUsersResponse>;
    /**
     * Creates a new Project User
     * @param projectId
     * @param options The options parameters.
     */
    createProjectUser(projectId: string, options?: TeamCloudCreateProjectUserOptionalParams): Promise<TeamCloudCreateProjectUserResponse>;
    /**
     * Gets a Project User by ID or email address.
     * @param userNameOrId
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUserByNameOrId(userNameOrId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUserByNameOrIdResponse>;
    /**
     * Updates an existing Project User.
     * @param userNameOrId
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUser(userNameOrId: string, projectId: string, options?: TeamCloudUpdateProjectUserOptionalParams): Promise<TeamCloudUpdateProjectUserResponse>;
    /**
     * Deletes an existing Project User.
     * @param userNameOrId
     * @param projectId
     * @param options The options parameters.
     */
    deleteProjectUser(userNameOrId: string, projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProjectUserResponse>;
    /**
     * Gets a Project User for the calling user.
     * @param projectId
     * @param options The options parameters.
     */
    getProjectUserMe(projectId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectUserMeResponse>;
    /**
     * Updates an existing Project User.
     * @param projectId
     * @param options The options parameters.
     */
    updateProjectUserMe(projectId: string, options?: TeamCloudUpdateProjectUserMeOptionalParams): Promise<TeamCloudUpdateProjectUserMeResponse>;
    /**
     * Gets all ProviderData for a Provider.
     * @param providerId
     * @param options The options parameters.
     */
    getProviderData(providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProviderDataResponse>;
    /**
     * Creates a new ProviderData item
     * @param providerId
     * @param options The options parameters.
     */
    createProviderData(providerId: string, options?: TeamCloudCreateProviderDataOptionalParams): Promise<TeamCloudCreateProviderDataResponse>;
    /**
     * Gets the ProviderData by ID.
     * @param providerDataId
     * @param providerId
     * @param options The options parameters.
     */
    getProviderDataById(providerDataId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProviderDataByIdResponse>;
    /**
     * Updates an existing ProviderData.
     * @param providerDataId
     * @param providerId
     * @param options The options parameters.
     */
    updateProviderData(providerDataId: string, providerId: string, options?: TeamCloudUpdateProviderDataOptionalParams): Promise<TeamCloudUpdateProviderDataResponse>;
    /**
     * Deletes a ProviderData.
     * @param providerDataId
     * @param providerId
     * @param options The options parameters.
     */
    deleteProviderData(providerDataId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProviderDataResponse>;
    /**
     * Gets all Provider Offers.
     * @param providerId
     * @param options The options parameters.
     */
    getProviderOffers(providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProviderOffersResponse>;
    /**
     * Creates a new ComponentOffer item
     * @param providerId
     * @param options The options parameters.
     */
    createProviderOffer(providerId: string, options?: TeamCloudCreateProviderOfferOptionalParams): Promise<TeamCloudCreateProviderOfferResponse>;
    /**
     * Gets the Offer by id.
     * @param offerId
     * @param providerId
     * @param options The options parameters.
     */
    getProviderOfferById(offerId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProviderOfferByIdResponse>;
    /**
     * Updates an existing ComponentOffer.
     * @param offerId
     * @param providerId
     * @param options The options parameters.
     */
    updateProviderOffer(offerId: string, providerId: string, options?: TeamCloudUpdateProviderOfferOptionalParams): Promise<TeamCloudUpdateProviderOfferResponse>;
    /**
     * Deletes a ComponentOffer.
     * @param offerId
     * @param providerId
     * @param options The options parameters.
     */
    deleteProviderOffer(offerId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProviderOfferResponse>;
    /**
     * Gets all Components for a Project.
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    getProviderProjectComponents(projectId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProviderProjectComponentsResponse>;
    /**
     * Creates a new Project Component.
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    createProviderProjectComponent(projectId: string, providerId: string, options?: TeamCloudCreateProviderProjectComponentOptionalParams): Promise<TeamCloudCreateProviderProjectComponentResponse>;
    /**
     * Gets a Project Component by id.
     * @param componentId
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    getProviderProjectComponentById(componentId: string, projectId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProviderProjectComponentByIdResponse>;
    /**
     * Updates an existing Project Component.
     * @param componentId
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    updateProviderProjectComponent(componentId: string, projectId: string, providerId: string, options?: TeamCloudUpdateProviderProjectComponentOptionalParams): Promise<TeamCloudUpdateProviderProjectComponentResponse>;
    /**
     * Deletes an existing Project Component.
     * @param componentId
     * @param projectId
     * @param providerId
     * @param options The options parameters.
     */
    deleteProviderProjectComponent(componentId: string, projectId: string, providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProviderProjectComponentResponse>;
    /**
     * Gets all Providers.
     * @param options The options parameters.
     */
    getProviders(options?: coreHttp.OperationOptions): Promise<TeamCloudGetProvidersResponse>;
    /**
     * Creates a new Provider.
     * @param options The options parameters.
     */
    createProvider(options?: TeamCloudCreateProviderOptionalParams): Promise<TeamCloudCreateProviderResponse>;
    /**
     * Gets a Provider by ID.
     * @param providerId
     * @param options The options parameters.
     */
    getProviderById(providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProviderByIdResponse>;
    /**
     * Updates an existing Provider.
     * @param providerId
     * @param options The options parameters.
     */
    updateProvider(providerId: string, options?: TeamCloudUpdateProviderOptionalParams): Promise<TeamCloudUpdateProviderResponse>;
    /**
     * Deletes an existing Provider.
     * @param providerId
     * @param options The options parameters.
     */
    deleteProvider(providerId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteProviderResponse>;
    /**
     * Gets the status of a long-running operation.
     * @param trackingId
     * @param options The options parameters.
     */
    getStatus(trackingId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetStatusResponse>;
    /**
     * Gets the status of a long-running operation.
     * @param projectId
     * @param trackingId
     * @param options The options parameters.
     */
    getProjectStatus(projectId: string, trackingId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetProjectStatusResponse>;
    /**
     * Creates a new TeamCloud User as an Admin.
     * @param options The options parameters.
     */
    createTeamCloudAdminUser(options?: TeamCloudCreateTeamCloudAdminUserOptionalParams): Promise<TeamCloudCreateTeamCloudAdminUserResponse>;
    /**
     * Gets the TeamCloud instance.
     * @param options The options parameters.
     */
    getTeamCloudInstance(options?: coreHttp.OperationOptions): Promise<TeamCloudGetTeamCloudInstanceResponse>;
    /**
     * Updates the TeamCloud instance.
     * @param options The options parameters.
     */
    createTeamCloudInstance(options?: TeamCloudCreateTeamCloudInstanceOptionalParams): Promise<TeamCloudCreateTeamCloudInstanceResponse>;
    /**
     * Updates the TeamCloud instance.
     * @param options The options parameters.
     */
    updateTeamCloudInstance(options?: TeamCloudUpdateTeamCloudInstanceOptionalParams): Promise<TeamCloudUpdateTeamCloudInstanceResponse>;
    /**
     * Gets all Tags for a TeamCloud Instance.
     * @param options The options parameters.
     */
    getTeamCloudTags(options?: coreHttp.OperationOptions): Promise<TeamCloudGetTeamCloudTagsResponse>;
    /**
     * Creates a new TeamCloud Tag.
     * @param options The options parameters.
     */
    createTeamCloudTag(options?: TeamCloudCreateTeamCloudTagOptionalParams): Promise<TeamCloudCreateTeamCloudTagResponse>;
    /**
     * Updates an existing TeamCloud Tag.
     * @param options The options parameters.
     */
    updateTeamCloudTag(options?: TeamCloudUpdateTeamCloudTagOptionalParams): Promise<TeamCloudUpdateTeamCloudTagResponse>;
    /**
     * Gets a TeamCloud Tag by Key.
     * @param tagKey
     * @param options The options parameters.
     */
    getTeamCloudTagByKey(tagKey: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetTeamCloudTagByKeyResponse>;
    /**
     * Deletes an existing TeamCloud Tag.
     * @param tagKey
     * @param options The options parameters.
     */
    deleteTeamCloudTag(tagKey: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteTeamCloudTagResponse>;
    /**
     * Gets all TeamCloud Users.
     * @param options The options parameters.
     */
    getTeamCloudUsers(options?: coreHttp.OperationOptions): Promise<TeamCloudGetTeamCloudUsersResponse>;
    /**
     * Creates a new TeamCloud User.
     * @param options The options parameters.
     */
    createTeamCloudUser(options?: TeamCloudCreateTeamCloudUserOptionalParams): Promise<TeamCloudCreateTeamCloudUserResponse>;
    /**
     * Gets a TeamCloud User by ID or email address.
     * @param userNameOrId
     * @param options The options parameters.
     */
    getTeamCloudUserByNameOrId(userNameOrId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetTeamCloudUserByNameOrIdResponse>;
    /**
     * Updates an existing TeamCloud User.
     * @param userNameOrId
     * @param options The options parameters.
     */
    updateTeamCloudUser(userNameOrId: string, options?: TeamCloudUpdateTeamCloudUserOptionalParams): Promise<TeamCloudUpdateTeamCloudUserResponse>;
    /**
     * Deletes an existing TeamCloud User.
     * @param userNameOrId
     * @param options The options parameters.
     */
    deleteTeamCloudUser(userNameOrId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudDeleteTeamCloudUserResponse>;
    /**
     * Gets a TeamCloud User A User matching the current authenticated user.
     * @param options The options parameters.
     */
    getTeamCloudUserMe(options?: coreHttp.OperationOptions): Promise<TeamCloudGetTeamCloudUserMeResponse>;
    /**
     * Updates an existing TeamCloud User.
     * @param options The options parameters.
     */
    updateTeamCloudUserMe(options?: TeamCloudUpdateTeamCloudUserMeOptionalParams): Promise<TeamCloudUpdateTeamCloudUserMeResponse>;
    /**
     * Gets all Projects for a User.
     * @param userId
     * @param options The options parameters.
     */
    getUserProjects(userId: string, options?: coreHttp.OperationOptions): Promise<TeamCloudGetUserProjectsResponse>;
    /**
     * Gets all Projects for a User.
     * @param options The options parameters.
     */
    getUserProjectsMe(options?: coreHttp.OperationOptions): Promise<TeamCloudGetUserProjectsMeResponse>;
}

export declare interface TeamCloudApplication {
    url?: string;
    version?: string;
    type?: "Web";
    resourceGroup?: AzureResourceGroup;
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
export declare interface TeamCloudCreateProjectLinkOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudCreateProjectOptionalParams extends coreHttp.OperationOptions {
    body?: ProjectDefinition;
}

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateProjectProviderDataOptionalParams extends coreHttp.OperationOptions {
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
export declare interface TeamCloudCreateProjectTypeOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudCreateProjectUserOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudCreateProviderDataOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudCreateProviderOfferOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudCreateProviderOptionalParams extends coreHttp.OperationOptions {
    body?: Provider;
}

/**
 * Optional parameters.
 */
export declare interface TeamCloudCreateProviderProjectComponentOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudCreateTeamCloudAdminUserOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudCreateTeamCloudInstanceOptionalParams extends coreHttp.OperationOptions {
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
export declare interface TeamCloudCreateTeamCloudTagOptionalParams extends coreHttp.OperationOptions {
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
export declare interface TeamCloudCreateTeamCloudUserOptionalParams extends coreHttp.OperationOptions {
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
export declare interface TeamCloudGetProjectProviderDataOptionalParams extends coreHttp.OperationOptions {
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

export declare interface TeamCloudInstance {
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

export declare interface TeamCloudInstanceDataResult {
    code?: number;
    status?: string;
    data?: TeamCloudInstance;
    location?: string;
}

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
export declare interface TeamCloudUpdateProjectLinkOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProjectProviderDataOptionalParams extends coreHttp.OperationOptions {
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
export declare interface TeamCloudUpdateProjectTypeOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProjectUserMeOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProjectUserOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProviderDataOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProviderOfferOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProviderOptionalParams extends coreHttp.OperationOptions {
    body?: Provider;
}

/**
 * Optional parameters.
 */
export declare interface TeamCloudUpdateProviderProjectComponentOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateTeamCloudInstanceOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateTeamCloudTagOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateTeamCloudUserMeOptionalParams extends coreHttp.OperationOptions {
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
 * Optional parameters.
 */
export declare interface TeamCloudUpdateTeamCloudUserOptionalParams extends coreHttp.OperationOptions {
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

export declare interface User {
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

export declare interface UserDataResult {
    code?: number;
    status?: string;
    data?: User;
    location?: string;
}

export declare interface UserDefinition {
    identifier?: string;
    role?: string;
    /**
     * Dictionary of <string>
     */
    properties?: {
        [propertyName: string]: string;
    };
}

export declare interface UserListDataResult {
    code?: number;
    status?: string;
    readonly data?: User[];
    location?: string;
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
    field?: string;
    message?: string;
}

export { }
