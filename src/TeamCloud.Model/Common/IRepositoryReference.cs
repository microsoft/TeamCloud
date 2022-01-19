/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Common;

public interface IRepositoryReference
{
    RepositoryReference Repository { get; set; }
}
