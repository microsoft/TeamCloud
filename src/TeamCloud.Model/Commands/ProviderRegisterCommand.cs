/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Commands
{
    public class ProviderRegisterCommand : ProviderCommand<ProviderConfiguration, ProviderRegisterCommandResult>
    {
        public ProviderRegisterCommand(Uri baseApi, User user, ProviderConfiguration payload) : base(baseApi, user, payload)
        { }
    }
}
