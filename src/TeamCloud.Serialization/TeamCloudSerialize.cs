/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace TeamCloud.Serialization
{
    public static class TeamCloudSerialize
    {
        public static string SerializeObject(object value, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.SerializeObject(value, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static string SerializeObject(object value, Formatting formatting, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.SerializeObject(value, formatting, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static object DeserializeObject(string value, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.DeserializeObject(value, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static object DeserializeObject(string value, Type type, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.DeserializeObject(value, type, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static T DeserializeObject<T>(string value, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.DeserializeObject<T>(value, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static void PopulateObject(string value, object target, TeamCloudSerializerSettings serializerSettings = null)
            => JsonConvert.PopulateObject(value, target, serializerSettings ?? TeamCloudSerializerSettings.Default);

        public static object MergeObject(string value, object target, TeamCloudSerializerSettings serializerSettings = null, JsonMergeSettings mergeSettings = null)
        {
            var targetJson = JObject.FromObject(target, (serializerSettings ?? TeamCloudSerializerSettings.Default).CreateSerializer());

            targetJson.Merge(JObject.Parse(value), mergeSettings);

            return DeserializeObject(targetJson.ToString(), target.GetType(), serializerSettings);
        }

        public static T MergeObject<T>(string value, T target, TeamCloudSerializerSettings serializerSettings = null, JsonMergeSettings mergeSettings = null)
            => (T)MergeObject(value, (object)target, serializerSettings, mergeSettings);
    }
}
