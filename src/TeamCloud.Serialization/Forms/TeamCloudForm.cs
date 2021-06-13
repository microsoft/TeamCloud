/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;

namespace TeamCloud.Serialization.Forms
{
    public static class TeamCloudForm
    {
        private static readonly ConcurrentDictionary<Type, Task<JToken>> dataSchemaCache = new ConcurrentDictionary<Type, Task<JToken>>();
        private static readonly ConcurrentDictionary<Type, Task<JToken>> formSchemaCache = new ConcurrentDictionary<Type, Task<JToken>>();

        private static Stream GetResourceStream<TData>(string extension)
            where TData : class, new()
        {
            var resourceName = typeof(TData).Assembly
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.Equals($"{typeof(TData).FullName}.{extension}", StringComparison.OrdinalIgnoreCase));

            return string.IsNullOrEmpty(resourceName)
                ? null
                : typeof(TData).Assembly.GetManifestResourceStream(resourceName);
        }

        public static async Task<JToken> GetDataSchemaAsync<TData>(bool clone = false)
            where TData : class, new()
        {
            var schema = await dataSchemaCache.GetOrAdd(typeof(TData), type =>
            {
                using var schemaStream = GetResourceStream<TData>("schema");

                JToken token;

                if (schemaStream is null)
                {
                    var generator = new JSchemaGenerator()
                    {
                        ContractResolver = new TeamCloudContractResolver()
                    };

                    token = JToken.Parse(generator.Generate(type).ToString());
                }
                else
                {
                    using var streamReader = new StreamReader(schemaStream);
                    using var schemaReader = new JsonTextReader(streamReader);

                    token = JToken.Load(schemaReader);
                }

                foreach (var propertyToken in token.SelectTokens("$..properties.*"))
                {
                    var titleToken = propertyToken.SelectToken("title");

                    if (titleToken is null
                        && propertyToken is JObject propertyObject
                        && propertyToken.Parent is JProperty propertyParent)
                    {
                        var title = PrettyPrintCamelCase(propertyParent.Name);

                        propertyObject.Add("title", new JValue(title));
                    }
                }

                return Task.FromResult(token);

                string PrettyPrintCamelCase(string value)
                {
                    if (string.IsNullOrEmpty(value))
                        return value;

                    var buffer = value.Trim()
                        .Select((c, i) => i == 0 ? char.ToUpper(c, CultureInfo.InvariantCulture) : c)
                        .ToArray();

                    return Regex.Replace(new string(buffer), @"(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", " $1");
                }

            }).ConfigureAwait(false);

            if (clone)
                schema = schema?.DeepClone();

            return schema;
        }

        public static async Task<JToken> GetFormSchemaAsync<TData>(bool clone = false)
            where TData : class, new()
        {
            var form = await formSchemaCache.GetOrAdd(typeof(TData), type =>
            {
                using var formStream = GetResourceStream<TData>("form");

                if (formStream is null)
                {
                    var instance = Activator.CreateInstance<TData>();
                    var instanceJson = TeamCloudSerialize.SerializeObject(instance, new TeamCloudFormConverter<TData>());

                    return Task.FromResult(JToken.Parse(instanceJson));
                }
                else
                {
                    using var streamReader = new StreamReader(formStream);
                    using var formReader = new JsonTextReader(streamReader);

                    return JToken.ReadFromAsync(formReader);
                }

            }).ConfigureAwait(false);

            if (clone)
                form = form?.DeepClone();

            return form;
        }
    }
}
