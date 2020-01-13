/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model.Data
{
    public interface Identifiable
    {
        Guid Id { get; set; }
    }
}
