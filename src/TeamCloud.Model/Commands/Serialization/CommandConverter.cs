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
    internal class CommandConverter : TypedConverter<ICommand>
    {
        private static readonly IContractResolver contractResolver = new CommandContractResolver();

        public CommandConverter() : base(contractResolver)
        { }
    }
}
