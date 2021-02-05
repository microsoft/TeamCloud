/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Diagnostics.CodeAnalysis;
using TeamCloud.Serialization.Resolver;

namespace TeamCloud.Model.Commands.Serialization
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid Uninstantiated Internal Classes", Justification = "Dynamically instatiated")]
    internal class CommandResultContractResolver : SuppressConverterContractResolver<CommandResultConverter>
    { }
}
