using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ComponentDeploymentType
    {
        Create,

        Delete,

        Custom
    }
}
