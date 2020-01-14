/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Model.Data
{
    public static class Validation
    {
        public static bool BeGuid(string guid)
            => !string.IsNullOrEmpty(guid) && Guid.TryParse(guid, out var _);
    }
}
