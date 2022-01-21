/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Data.Results;

public interface IStatusResult : ISuccessResult, IErrorResult
{
    string State { get; }

    // user-facing
    string StateMessage { get; }
}
