/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters
{
    public interface IAdapterProvider
    {
        IEnumerable<IAdapter> GetAdapters();

        IAdapter GetAdapter(DeploymentScopeType deploymentScopeType);
    }
}
