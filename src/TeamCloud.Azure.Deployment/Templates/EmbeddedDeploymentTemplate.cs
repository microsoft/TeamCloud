/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Azure.Deployment.Templates
{
    public abstract class EmbeddedDeploymentTemplate : AzureDeploymentTemplate
    {
        private const string TemplateSuffix = ".json";

        protected EmbeddedDeploymentTemplate()
        {
            Template = GetMainTemplate(this.GetType());
            LinkedTemplates = GetLinkedTemplates(this.GetType());
            Parameters = GetParameters(Template);
        }

        private static string GetMainTemplate(Type templateType)
            => GetResourceJson(templateType, $"{templateType.FullName}.json", true);

        private static IDictionary<string, string> GetLinkedTemplates(Type templateType)
        {
            var templatePrefix = $"{templateType.FullName}_";

            return templateType.Assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(templatePrefix, StringComparison.Ordinal)
                            && name.EndsWith(TemplateSuffix, StringComparison.Ordinal))
                .ToDictionary(name => name.Substring(templatePrefix.Length), name => GetResourceJson(templateType, name));
        }

        private static string GetResourceJson(Type templateType, string resourceName, bool enforceVersionUpdate = false)
        {
            using var stream = templateType.Assembly.GetManifestResourceStream(resourceName);

            if (stream is null)
            {
                var availableResources = templateType.Assembly.GetManifestResourceNames()
                    .Where(name => name.StartsWith(templateType.FullName, StringComparison.Ordinal)
                                && name.EndsWith(TemplateSuffix, StringComparison.Ordinal))
                    .Select(name => name);

                throw new ArgumentException($"Unable to find embedded template by resource name '{resourceName}' - available resources: {string.Join(", ", availableResources)}", nameof(resourceName));
            }

            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);

            var content = JObject.Load(jsonReader);

            if (!(content.SelectToken("$.contentVersion") is JValue contentVersion))
            {
                content.Children().First().AddAfterSelf(new JProperty("contentVersion", "0.0.0.0"));

                contentVersion = content.SelectToken("$.contentVersion") as JValue;
            }

            if (enforceVersionUpdate || contentVersion.ToString(CultureInfo.InvariantCulture).Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase))
            {
                contentVersion.Value = templateType.Assembly.GetName().Version.ToString(4);
            }

            return content.ToString();
        }

        private static IDictionary<string, object> GetParameters(string template)
        {
            if (!string.IsNullOrEmpty(template))
            {
                var templateParameters = JObject.Parse(template).SelectToken("$.parameters");

                return templateParameters?.Children<JProperty>()
                    .Select(token => new KeyValuePair<string, object>(token.Name, null))
                    .ToDictionary();
            }

            return new Dictionary<string, object>();
        }
    }
}
