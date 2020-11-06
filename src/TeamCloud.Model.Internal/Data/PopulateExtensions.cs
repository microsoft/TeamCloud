/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;

namespace TeamCloud.Model.Data
{
    public static class PopulateExtensions
    {
        public static TExternal PopulateExternalModel<TInternal, TExternal>(this TInternal source, TExternal target = null)
            where TInternal : IPopulate<TExternal>
            where TExternal : class, new()
            => source.PopulateExternalModel(target);


        public static Component PopulateExternalModel(this ComponentDocument source, Component target = null)
            => source.PopulateExternalModel<ComponentDocument, Component>(target);

        public static ComponentOffer PopulateExternalModel(this ComponentOfferDocument source, ComponentOffer target = null)
            => source.PopulateExternalModel<ComponentOfferDocument, ComponentOffer>(target);

        public static DeploymentScope PopulateExternalModel(this DeploymentScopeDocument source, DeploymentScope target = null)
            => source.PopulateExternalModel<DeploymentScopeDocument, DeploymentScope>(target);

        public static Organization PopulateExternalModel(this OrganizationDocument source, Organization target = null)
            => source.PopulateExternalModel<OrganizationDocument, Organization>(target);

        public static Project PopulateExternalModel(this ProjectDocument source, Project target = null)
        {
            var project = source.PopulateExternalModel<ProjectDocument, Project>(target);
            foreach (var user in project.Users)
                user.ProjectMemberships = user.ProjectMemberships.Where(m => m.ProjectId == project.Id).ToList();
            return project;
        }

        public static ProjectLink PopulateExternalModel(this ProjectLinkDocument source, ProjectLink target = null)
            => source.PopulateExternalModel<ProjectLinkDocument, ProjectLink>(target);

        public static ProjectTemplate PopulateExternalModel(this ProjectTemplateDocument source, ProjectTemplate target = null)
            => source.PopulateExternalModel<ProjectTemplateDocument, ProjectTemplate>(target);

        public static ProjectType PopulateExternalModel(this ProjectTypeDocument source, ProjectType target = null)
            => source.PopulateExternalModel<ProjectTypeDocument, ProjectType>(target);

        public static Provider PopulateExternalModel(this ProviderDocument source, Provider target = null)
            => source.PopulateExternalModel<ProviderDocument, Provider>(target);

        public static ProviderData PopulateExternalModel(this ProviderDataDocument source, ProviderData target = null)
            => source.PopulateExternalModel<ProviderDataDocument, ProviderData>(target);

        public static TeamCloudInstance PopulateExternalModel(this TeamCloudInstanceDocument source, TeamCloudInstance target = null)
            => source.PopulateExternalModel<TeamCloudInstanceDocument, TeamCloudInstance>(target);

        public static User PopulateExternalModel(this UserDocument source, User target = null)
            => source.PopulateExternalModel<UserDocument, User>(target);

        public static User PopulateExternalModel(this UserDocument source, string projectId, User target = null)
        {
            var user = source.PopulateExternalModel<UserDocument, User>(target);
            user.ProjectMemberships = user.ProjectMemberships.Where(m => m.ProjectId == projectId).ToList();
            return user;
        }


        public static TInternal PopulateFromExternalModel<TInternal, TExternal>(this TInternal target, TExternal source)
            where TInternal : IPopulate<TExternal>
            where TExternal : class, new()
        {
            if (source is null)
                throw new System.ArgumentNullException(nameof(source));

            target.PopulateFromExternalModel(source);

            return target;
        }
    }
}
