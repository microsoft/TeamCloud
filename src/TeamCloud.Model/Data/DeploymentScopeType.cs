using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeploymentScopeType
    {
        AzureResourceManager,

        AzureDevOps,

        GitHub
    }
}
