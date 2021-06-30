/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.Git.Converter
{
    public abstract class YamlTemplateConverter<T> : JsonConverter
        where T : class, new()
    {

        internal YamlTemplateConverter()
        { }

        public override bool CanConvert(Type objectType)
            => typeof(T) == objectType;


        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        protected JToken GeneratePermissionDictionary(JObject json, JsonSerializer serializer)
        {
            var keyValuePairs = json
                .SelectTokens("$.permissions[*]")
                .OfType<JObject>()
                .Select(t =>
                {
                    var role = Enum.Parse<ProjectUserRole>(t.GetValue("role", StringComparison.OrdinalIgnoreCase)?.ToString().Trim(), true);
                    var permission = t.GetValue("permission", StringComparison.OrdinalIgnoreCase)?.ToString().Trim();

                    return new KeyValuePair<ProjectUserRole, string>(role, permission);
                });

            var dictionary = keyValuePairs
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(grp => grp.Key, grp => grp.Select(kvp => kvp.Value).Distinct());

            return JObject.Parse(TeamCloudSerialize.SerializeObject(dictionary, new TeamCloudSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None
            }));
        }

        protected JToken GenerateInputJsonSchema(JObject json, JsonSerializer serializer)
        {
            var inputSchema = new JSchema() { Type = JSchemaType.Object };

            foreach (var parameter in json.SelectTokens("parameters[*]").OfType<JObject>())
            {
                var parameterId = parameter.GetValue("id", StringComparison.OrdinalIgnoreCase)?.ToString();

                if (parameter.TryGetValue("required", out var parameterRequiredToken))
                {
                    if (parameterRequiredToken is JValue parameterRequiredValue && (bool)parameterRequiredValue)
                    {
                        // register this parameter as required
                        inputSchema.Required.Add(parameterId);
                    }

                    // remove the required information to
                    // avoid deserialization issues
                    parameterRequiredToken.Parent.Remove();
                }

                var parameterType = Enum.Parse<JSchemaType>(parameter.GetValue("type", StringComparison.OrdinalIgnoreCase)?.ToString(), true);

                if (parameter.TryGetValue("default", StringComparison.OrdinalIgnoreCase, out var parameterDefaultToken))
                {
                    parameter.SetProperty("value", null); // delete any existing parameter
                    parameterDefaultToken.Parent.Replace(new JProperty("value", ConvertToSchemaTypeValue(parameterType, parameterDefaultToken)));
                }

                if (parameter.TryGetValue("allowed", StringComparison.OrdinalIgnoreCase, out var parameterAllowedToken))
                {
                    parameter.SetProperty("enum", null); // delete any existing parameter
                    parameterAllowedToken.Parent.Replace(new JProperty("enum", ConvertToSchemaTypeValue(parameterType, parameterAllowedToken)));
                }

                inputSchema.Properties.Add(parameterId, parameter.ToObject<JSchema>(serializer));
            }

            json.SetProperty("parameters", null);

            return new JValue(inputSchema.ToString(Formatting.None));

            JToken ConvertToSchemaTypeValue(JSchemaType schemaType, JToken value)
            {
                if (value is null)
                {
                    return value;
                }
                else if (value is JArray valueArray)
                {
                    return new JArray(valueArray.Values().Select(v => ConvertToSchemaTypeValue(schemaType, v)).ToArray());
                }
                else
                {
                    return schemaType switch
                    {
                        JSchemaType.Number => new JValue(double.TryParse($"{value}", out double result) ? result : 0),
                        JSchemaType.Integer => new JValue(int.TryParse($"{value}", out int result) ? result : 0),
                        JSchemaType.Boolean => new JValue(bool.TryParse($"{value}", out bool result) ? result : false),
                        _ => value
                    };
                }
            }
        }
    }
}
