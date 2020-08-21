/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Data.Results
{
    public sealed class ValidationError
    {
        public string Field { get; set; }

        public string Message { get; set; }
    }
}
