import * as coreHttp from "@azure/core-http";
import { TeamCloudContext } from "./teamCloudContext";
import { TeamCloudOptionalParams, TeamCloudGetProjectsResponse, TeamCloudCreateProjectOptionalParams, TeamCloudCreateProjectResponse, TeamCloudGetProjectByNameOrIdResponse, TeamCloudDeleteProjectResponse, TeamCloudGetProjectComponentsResponse, TeamCloudCreateProjectComponentOptionalParams, TeamCloudCreateProjectComponentResponse, TeamCloudGetProjectComponentByIdResponse, TeamCloudDeleteProjectComponentResponse, TeamCloudGetProjectIdentityResponse, TeamCloudGetProjectLinksResponse, TeamCloudCreateProjectLinkOptionalParams, TeamCloudCreateProjectLinkResponse, TeamCloudGetProjectLinkByKeyResponse, TeamCloudUpdateProjectLinkOptionalParams, TeamCloudUpdateProjectLinkResponse, TeamCloudDeleteProjectLinkResponse, TeamCloudGetProjectOffersResponse, TeamCloudGetProjectOfferByIdResponse, TeamCloudGetProjectProviderDataOptionalParams, TeamCloudGetProjectProviderDataResponse, TeamCloudCreateProjectProviderDataOptionalParams, TeamCloudCreateProjectProviderDataResponse, TeamCloudGetProjectProviderDataByIdResponse, TeamCloudUpdateProjectProviderDataOptionalParams, TeamCloudUpdateProjectProviderDataResponse, TeamCloudDeleteProjectProviderDataResponse, TeamCloudGetProjectTagsResponse, TeamCloudCreateProjectTagOptionalParams, TeamCloudCreateProjectTagResponse, TeamCloudUpdateProjectTagOptionalParams, TeamCloudUpdateProjectTagResponse, TeamCloudGetProjectTagByKeyResponse, TeamCloudDeleteProjectTagResponse, TeamCloudGetProjectTypesResponse, TeamCloudCreateProjectTypeOptionalParams, TeamCloudCreateProjectTypeResponse, TeamCloudGetProjectTypeByIdResponse, TeamCloudUpdateProjectTypeOptionalParams, TeamCloudUpdateProjectTypeResponse, TeamCloudDeleteProjectTypeResponse, TeamCloudGetProjectUsersResponse, TeamCloudCreateProjectUserOptionalParams, TeamCloudCreateProjectUserResponse, TeamCloudGetProjectUserByNameOrIdResponse, TeamCloudUpdateProjectUserOptionalParams, TeamCloudUpdateProjectUserResponse, TeamCloudDeleteProjectUserResponse, TeamCloudGetProjectUserMeResponse, TeamCloudUpdateProjectUserMeOptionalParams, TeamCloudUpdateProjectUserMeResponse, TeamCloudGetProviderDataResponse, TeamCloudCreateProviderDataOptionalParams, TeamCloudCreateProviderDataResponse, TeamCloudGetProviderDataByIdResponse, TeamCloudUpdateProviderDataOptionalParams, TeamCloudUpdateProviderDataResponse, TeamCloudDeleteProviderDataResponse, TeamCloudGetProviderOffersResponse, TeamCloudCreateProviderOfferOptionalParams, TeamCloudCreateProviderOfferResponse, TeamCloudGetProviderOfferByIdResponse, TeamCloudUpdateProviderOfferOptionalParams, TeamCloudUpdateProviderOfferResponse, TeamCloudDeleteProviderOfferResponse, TeamCloudGetProviderProjectComponentsResponse, TeamCloudCreateProviderProjectComponentOptionalParams, TeamCloudCreateProviderProjectComponentResponse, TeamCloudGetProviderProjectComponentByIdResponse, TeamCloudUpdateProviderProjectComponentOptionalParams, TeamCloudUpdateProviderProjectComponentResponse, TeamCloudDeleteProviderProjectComponentResponse, TeamCloudGetProvidersResponse, TeamCloudCreateProviderOptionalParams, TeamCloudCreateProviderResponse, TeamCloudGetProviderByIdResponse, TeamCloudUpdateProviderOptionalParams, TeamCloudUpdateProviderResponse, TeamCloudDeleteProviderResponse, TeamCloudGetStatusResponse, TeamCloudGetProjectStatusResponse, TeamCloudCreateTeamCloudAdminUserOptionalParams, TeamCloudCreateTeamCloudAdminUserResponse, TeamCloudGetTeamCloudInstanceResponse, TeamCloudCreateTeamCloudInstanceOptionalParams, TeamCloudCreateTeamCloudInstanceResponse, TeamCloudUpdateTeamCloudInstanceOptionalParams, TeamCloudUpdateTeamCloudInstanceResponse, TeamCloudGetTeamCloudTagsResponse, TeamCloudCreateTeamCloudTagOptionalParams, TeamCloudCreateTeamCloudTagResponse, TeamCloudUpdateTeamCloudTagOptionalParams, TeamCloudUpdateTeamCloudTagResponse, TeamCloudGetTeamCloudTagByKeyResponse, TeamCloudDeleteTeamCloudTagResponse, TeamCloudGetTeamCloudUsersResponse, TeamCloudCreateTeamCloudUserOptionalParams, TeamCloudCreateTeamCloudUserResponse, TeamCloudGetTeamCloudUserByNameOrIdResponse, TeamCloudUpdateTeamCloudUserOptionalParams, TeamCloudUpdateTeamCloudUserResponse, TeamCloudDeleteTeamCloudUserResponse, TeamCloudGetTeamCloudUserMeResponse, TeamCloudUpdateTeamCloudUserMeOptionalParams, TeamCloudUpdateTeamCloudUserMeResponse, TeamCloudGetUserProjectsResponse, TeamCloudGetUserProjectsMeResponse } from "./models";
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
//# sourceMappingURL=teamCloud.d.ts.map