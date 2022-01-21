/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Data.Results;

public interface ISuccessResult : IReturnResult
{
    string Location { get; }
}
