/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderRegisterCommandResult : CommandResult<ProviderRegistration> { }

    public class ProviderProjectCreateCommandResult : CommandResult<ProviderOutput> { }

    public class ProviderProjectUpdateCommandResult : CommandResult<ProviderOutput> { }

    public class ProviderProjectDeleteCommandResult : CommandResult<ProviderOutput> { }

    public class ProviderProjectUserCreateCommandResult : CommandResult<ProviderOutput> { }

    public class ProviderProjectUserUpdateCommandResult : CommandResult<ProviderOutput> { }

    public class ProviderProjectUserDeleteCommandResult : CommandResult<ProviderOutput> { }

    public class ProviderTeamCloudUserCreateCommandResult : CommandResult<ProviderOutput> { }

    public class ProviderTeamCloudUserUpdateCommandResult : CommandResult<ProviderOutput> { }

    public class ProviderTeamCloudUserDeleteCommandResult : CommandResult<ProviderOutput> { }
}
