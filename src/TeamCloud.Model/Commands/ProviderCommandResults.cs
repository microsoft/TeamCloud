/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderRegisterCommandResult : CommandResult<ProviderRegistration> { }

    public class ProviderProjectCreateCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderProjectUpdateCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderProjectDeleteCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderProjectUserCreateCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderProjectUserUpdateCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderProjectUserDeleteCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderTeamCloudCreateCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderTeamCloudUserCreateCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderTeamCloudUserUpdateCommandResult : CommandResult<ProviderProperties> { }

    public class ProviderTeamCloudUserDeleteCommandResult : CommandResult<ProviderProperties> { }
}
