/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;

namespace TeamCloud.Model.Common
{
    public interface ITags
    {
        IDictionary<string, string> Tags { get; set; }
    }
}
