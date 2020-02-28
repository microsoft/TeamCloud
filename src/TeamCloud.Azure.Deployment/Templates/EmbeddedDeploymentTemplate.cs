/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Azure.Deployment.Templates
{
    public abstract class EmbeddedDeploymentTemplate : AzureDeploymentTemplate
    {
        private const string TemplateSuffix = ".json";

        protected override Task OnCreateAsync()
        {
            base.Template = GetMainTemplate();
            base.Parameters = GetParameters(Template);
            base.LinkedTemplates = GetLinkedTemplates();

            return base.OnCreateAsync();
        }

        private string GetMainTemplate()
            => GetResourceJson($"{this.GetType().FullName}.json");

        private IDictionary<string, string> GetLinkedTemplates()
        {
            var templatePrefix = $"{this.GetType().FullName}_";

            return this.GetType().Assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(templatePrefix, StringComparison.Ordinal)
                            && name.EndsWith(TemplateSuffix, StringComparison.Ordinal))
                .ToDictionary(name => name.Substring(templatePrefix.Length), name => GetResourceJson(name));
        }

        private string GetResourceJson(string resourceName)
        {
            using var stream = this.GetType().Assembly.GetManifestResourceStream(resourceName);

            if (stream is null)
            {
                var availableResources = this.GetType().Assembly.GetManifestResourceNames()
                    .Where(name => name.StartsWith(this.GetType().FullName, StringComparison.Ordinal)
                                && name.EndsWith(TemplateSuffix, StringComparison.Ordinal))
                    .Select(name => name);

                throw new ArgumentException($"Unable to find embedded template by resource name '{resourceName}' - available resources: {string.Join(", ", availableResources)}", nameof(resourceName));
            }

            return new StreamReader(stream).ReadToEnd();
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
