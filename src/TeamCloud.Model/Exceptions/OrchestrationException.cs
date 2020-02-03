/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;

namespace TeamCloud.Model.Exceptions
{
    public class OrchestrationException : Exception
    {
        public List<ProviderException> ProviderExceptions { get; set; }


    }
}
