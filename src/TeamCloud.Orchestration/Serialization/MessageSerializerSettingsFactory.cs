using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using TeamCloud.Serialization;

namespace TeamCloud.Orchestration.Serialization
{
    public sealed class MessageSerializerSettingsFactory : IMessageSerializerSettingsFactory
    {
        // CAUTION - this factory class is used by the durable function runtime to resolve
        // the serializer settings when orchestrations, activities, etc. serializing and
        // deserializing their state as JSON. as we require to persist type informations
        // for our command handling, we override the default behavior and return our
        // TeamCloud default serializer settings.

        public JsonSerializerSettings CreateJsonSerializerSettings() => TeamCloudSerializerSettings.Default;
    }
}
