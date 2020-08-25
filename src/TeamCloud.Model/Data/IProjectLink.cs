/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Common;

namespace TeamCloud.Model.Data
{
    public interface IProjectLink : IIdentifiable
    {
        string HRef { get; set; }

        string Title { get; set; }

        ProjectLinkType Type { get; set; }
    }
}
