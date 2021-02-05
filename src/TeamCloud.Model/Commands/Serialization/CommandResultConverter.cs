/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Serialization.Converter;

namespace TeamCloud.Model.Commands.Serialization
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid Uninstantiated Internal Classes", Justification = "Dynamically instatiated")]
    internal class CommandResultConverter : TypedConverter<ICommandResult>
    {
        private static readonly IContractResolver contractResolver = new CommandResultContractResolver();

        public CommandResultConverter() : base(contractResolver)
        { }
    }
}
