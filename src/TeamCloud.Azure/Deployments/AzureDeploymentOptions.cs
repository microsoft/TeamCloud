/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace TeamCloud.Azure.Deployments
{
    public interface IAzureDeploymentOptions
    {
        public string DefaultLocation { get; }
    }
}
