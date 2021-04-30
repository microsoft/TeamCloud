/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers.Core
{
    public class TeamCloudOrganizationContext
    {
        public User ContextUser { get; set; }

        public Organization Organization { get; set; }
    }

    public class TeamCloudOrganizationUserContext : TeamCloudOrganizationContext
    {
        public User User { get; set; }
    }

    public class TeamCloudProjectContext : TeamCloudOrganizationContext
    {
        public Project Project { get; set; }
    }

    public class TeamCloudDeploymentScopeContext : TeamCloudOrganizationContext
    {
        public DeploymentScope DeploymentScope { get; set; }
    }

    public class TeamCloudProjectTemplateContext : TeamCloudOrganizationContext
    {
        public ProjectTemplate ProjectTemplate { get; set; }
    }

    public class TeamCloudProjectUserContext : TeamCloudProjectContext
    {
        public User User { get; set; }
    }

    public class TeamCloudProjectIdentityContext : TeamCloudProjectContext
    {
        public ProjectIdentity ProjectIdentity { get; set; }
    }

    public class TeamCloudProjectComponentContext : TeamCloudProjectContext
    {
        public Component Component { get; set; }
    }
}
