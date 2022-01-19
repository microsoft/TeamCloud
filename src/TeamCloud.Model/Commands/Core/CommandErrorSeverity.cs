/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Model.Commands.Core;

// for now we only support the severity levels
// listed below. however the values of this
// enumartion aligns with the severity levels
// used by appinsights documented here:
// https://docs.microsoft.com/en-us/dotnet/api/microsoft.applicationinsights.datacontracts.severitylevel?view=azure-dotnet

public enum CommandErrorSeverity
{
    Error = 3,

    Warning = 2
}
