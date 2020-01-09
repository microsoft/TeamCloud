/**
*  Copyright (c) Microsoft Corporation.
*  Licensed under the MIT License.
*/

using System;

namespace TeamCloud.Model
{
    public interface Identifiable
    {
        Guid Id { get; set; }
    }
}
