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

        public static Model.Data.User PopulateExternalModel(this UserDocument source, Model.Data.User target = null)
            => source.PopulateExternalModel<UserDocument, Model.Data.User>(target);

        public static Model.Data.User PopulateExternalModel(this UserDocument source, string projectId, Model.Data.User target = null)
        {
            var user = source.PopulateExternalModel<UserDocument, Model.Data.User>(target);
            user.ProjectMemberships = user.ProjectMemberships.Where(m => m.ProjectId == projectId).ToList();
            return user;
        }

        public static Model.Data.Project PopulateExternalModel(this ProjectDocument source, Model.Data.Project target = null)
        {
            var project = source.PopulateExternalModel<ProjectDocument, Model.Data.Project>(target);
            foreach (var user in project.Users)
                user.ProjectMemberships = user.ProjectMemberships.Where(m => m.ProjectId == project.Id).ToList();
            return project;
        }

        public static Model.Data.Provider PopulateExternalModel(this ProviderDocument source, Model.Data.Provider target = null)
            => source.PopulateExternalModel<ProviderDocument, Model.Data.Provider>(target);

        public static Model.Data.ProviderData PopulateExternalModel(this ProviderDataDocument source, Model.Data.ProviderData target = null)
            => source.PopulateExternalModel<ProviderDataDocument, Model.Data.ProviderData>(target);

        public static Model.Data.ProjectType PopulateExternalModel(this ProjectTypeDocument source, Model.Data.ProjectType target = null)
            => source.PopulateExternalModel<ProjectTypeDocument, Model.Data.ProjectType>(target);

        public static Model.Data.TeamCloudInstance PopulateExternalModel(this TeamCloudInstanceDocument source, Model.Data.TeamCloudInstance target = null)
            => source.PopulateExternalModel<TeamCloudInstanceDocument, Model.Data.TeamCloudInstance>(target);

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
