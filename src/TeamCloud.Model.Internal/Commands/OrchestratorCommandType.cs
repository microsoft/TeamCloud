/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public enum OrchestratorCommandType
    {
        ProjectCreate,
        ProjectDelete,
        ProjectUpdate,
        ProjectUserCreate,
        ProjectUserDelete,
        ProjectUserUpdate,
        ProviderCreate,
        ProviderDelete,
        ProviderUpdate,
        TeamCloudUserCreate,
        TeamCloudUserDelete,
        TeamCloudUserUpdate
    }
}
