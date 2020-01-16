/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model.Data
{
    public interface IIdentifiable
    {
        Guid Id { get; set; }
    }
}
