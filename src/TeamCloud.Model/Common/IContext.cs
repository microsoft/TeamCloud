/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;

namespace TeamCloud.Model.Common
{
    public interface IOrganizationContext
    {
        [JsonProperty(Required = Required.Always)]
        string Organization { get; set; }
    }

    public interface IProjectContext : IOrganizationContext
    {
        [JsonProperty(Required = Required.Always)]
        string ProjectId { get; set; }
    }

    public interface IComponentContext : IProjectContext
    {
        [JsonProperty(Required = Required.Always)]
        string ComponentId { get; set; }
    }

    public interface IDeploymentScopeContext : IProjectContext
    {
        [JsonProperty(Required = Required.Always)]
        string DeploymentScopeId { get; set; }
    }
}
