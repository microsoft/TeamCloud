/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Data.Results;

public interface IDataResult : ISuccessResult
{ }

public interface IDataResult<T> : IDataResult
    where T : new()
{
    T Data { get; }
}
