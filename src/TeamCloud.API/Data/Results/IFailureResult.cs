/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;

namespace TeamCloud.API.Data.Results;

public interface IFailureResult : IReturnResult
{
    IList<ResultError> Errors { get; }
}
