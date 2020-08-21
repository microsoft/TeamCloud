/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Common;

namespace TeamCloud.Model.Data
{
    public interface IProjectLink : IIdentifiable
    {
        string ProjectId { get; set; }

        string HRef { get; set; }

        string Title { get; set; }

        ProjectLinkType Type { get; set; }
    }
}
