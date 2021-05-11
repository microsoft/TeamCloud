/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Serialization
{
    public static class TeamCloudSerialize
    {
        private static TeamCloudSerializerSettings GetTeamCloudSerializerSettingsWithConverters(JsonConverter converter, params JsonConverter[] additionalConverters)
            => new TeamCloudSerializerSettings(converter ?? throw new ArgumentNullException(nameof(converter)), additionalConverters);

        public static string SerializeObject(object value, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.SerializeObject(value, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static string SerializeObject(object value, Formatting formatting, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.SerializeObject(value, formatting, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static object DeserializeObject(string value, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.DeserializeObject(value, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static object DeserializeObject(string value, JsonConverter converter, params JsonConverter[] additionalConverters)
            => DeserializeObject(value, GetTeamCloudSerializerSettingsWithConverters(converter, additionalConverters));

        public static object DeserializeObject(string value, Type type, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.DeserializeObject(value, type, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static object DeserializeObject(string value, Type type, JsonConverter converter, params JsonConverter[] additionalConverters)
            => DeserializeObject(value, type, GetTeamCloudSerializerSettingsWithConverters(converter, additionalConverters));

        public static T DeserializeObject<T>(string value, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.DeserializeObject<T>(value, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static T DeserializeObject<T>(string value, JsonConverter converter, params JsonConverter[] additionalConverters)
            => DeserializeObject<T>(value, GetTeamCloudSerializerSettingsWithConverters(converter, additionalConverters));

        public static void PopulateObject(string value, object target, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.PopulateObject(value, target, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static void PopulateObject(string value, object target, JsonConverter converter, params JsonConverter[] additionalConverters)
            => PopulateObject(value, target, GetTeamCloudSerializerSettingsWithConverters(converter, additionalConverters));

        public static object MergeObject(string value, object target, TeamCloudSerializerSettings serializerSettings = null, JsonMergeSettings mergeSettings = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            var targetJson = JObject.FromObject(target, (serializerSettings ?? TeamCloudSerializerSettings.Default).CreateSerializer());

            targetJson.Merge(JObject.Parse(value), mergeSettings);

            return DeserializeObject(targetJson.ToString(), target.GetType(), serializerSettings);
        }

        public static T MergeObject<T>(string value, T target, TeamCloudSerializerSettings serializerSettings = null, JsonMergeSettings mergeSettings = null)
            => (T)MergeObject(value, (object)target, serializerSettings, mergeSettings);
    }
}
