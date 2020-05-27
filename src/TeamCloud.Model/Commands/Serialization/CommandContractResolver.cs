/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Serialization.Resolver;

namespace TeamCloud.Model.Commands.Serialization
{
    internal class CommandContractResolver : SuppressContractResolver<CommandConverter>
    { }
}
