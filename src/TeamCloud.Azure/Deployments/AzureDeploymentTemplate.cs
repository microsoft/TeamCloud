/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Azure.Deployments
{
    public interface IAzureDeploymentTemplate
    {
        const string ArtifactsLocationParameterName = "_artifactsLocation";
        const string ArtifactsLocationSasTokenParameterName = "_artifactsLocationSasToken";

        string Template { get; }

        IDictionary<string, object> Parameters { get; }

        IDictionary<string, string> LinkedTemplates { get; }
    }

    public abstract class EmbeddedAzureDeploymentTemplate : IAzureDeploymentTemplate
    {
        private readonly Lazy<IDictionary<string, string>> templates;
        private readonly Lazy<IDictionary<string, object>> parameters;

        protected EmbeddedAzureDeploymentTemplate()
        {
            templates = new Lazy<IDictionary<string, string>>(() => GetTemplates(this.GetType()));
            parameters = new Lazy<IDictionary<string, object>>(() => GetParameters(this.Template));
        }

        private static IDictionary<string, string> GetTemplates(Type templateType)
        {
            string resourceNamePrefix = $"{templateType.FullName}.";
            string resourceNameMain = $"{templateType.FullName}.json";

            var resourceNames = templateType.Assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(resourceNamePrefix, StringComparison.Ordinal));

            return resourceNames.ToDictionary(
                resourceName => resourceName.Equals(resourceNameMain) ? "azuredeploy.json" : resourceName.Replace(resourceNamePrefix, string.Empty),
                resourceName => GetTemplate(resourceName));

            string GetTemplate(string templateName)
            {
                using var stream = templateType.Assembly.GetManifestResourceStream(templateName);
                using var reader = new StreamReader(stream);

                return reader.ReadToEnd();
            }
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

        public virtual string Template
            => templates.Value["azuredeploy.json"];

        public virtual IDictionary<string, object> Parameters
            => parameters.Value;

        public virtual IDictionary<string, string> LinkedTemplates
            => new ReadOnlyDictionary<string, string>(templates.Value.Where(kvp => !kvp.Key.Equals("azuredeploy.json")).ToDictionary());
    }
}
