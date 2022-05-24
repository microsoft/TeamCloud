//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters;

public interface IAdapterRunner
{
    Task<Dictionary<string, string>> GetSecretsAsync(DeploymentScope deploymentScope, Component component);

    Task<Dictionary<string, string>> GetCredentialsAsync(DeploymentScope deploymentScope, Component component);

    Task<Dictionary<string, string>> GetEnvironmentAsync(DeploymentScope deploymentScope, Component component);
}
