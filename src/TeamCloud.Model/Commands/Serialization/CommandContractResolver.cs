/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Serialization.Resolver;

namespace TeamCloud.Model.Commands.Serialization
{
    internal class CommandContractResolver : SuppressContractResolver<CommandConverter>
    { }
}
