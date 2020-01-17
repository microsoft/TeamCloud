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

namespace TeamCloud.Azure.Deployments.Templates
{
    public abstract class EmbeddedDeploymentTemplate : AzureDeploymentTemplate
    {
        protected override Task OnCreateAsync()
        {
            var templateJson = GetMainTemplate();

            if (string.IsNullOrEmpty(templateJson))
                throw new NotSupportedException($"Unable to create instance of template '{this.GetType()}' - the related template json is empty or does not exist.");

            base.Template = templateJson;
            base.Parameters = GetParameters(templateJson);
            base.LinkedTemplates = GetLinkedTemplates();

            return base.OnCreateAsync();
        }

        private string GetMainTemplate()
            => GetResourceJson($"{this.GetType().FullName}.json");

        private IDictionary<string, string> GetLinkedTemplates()
        {
            var resourceNamePrefix = $"{this.GetType().FullName}_";
            var resourceNameSuffix = ".json";

            return this.GetType().Assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(resourceNamePrefix, StringComparison.Ordinal)
                            && name.EndsWith(resourceNameSuffix, StringComparison.Ordinal))
                .ToDictionary(name => name.Substring(resourceNamePrefix.Length), name => GetResourceJson(name));
        }

        private string GetResourceJson(string resourceName)
        {
            using var stream = this.GetType().Assembly.GetManifestResourceStream(resourceName);

            return stream is null ? null : new StreamReader(stream).ReadToEnd();
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
