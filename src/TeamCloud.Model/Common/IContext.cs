/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Model.Common
{
    public interface IOrganizationContext
    {
        string Organization { get; set; }
    }

    public interface IProjectContext : IOrganizationContext
    {
        string ProjectId { get; set; }
    }
}
