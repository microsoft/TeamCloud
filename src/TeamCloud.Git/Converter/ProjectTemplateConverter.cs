/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.Git.Converter
{
    public sealed class ProjectTemplateConverter : YamlTemplateConverter<ProjectTemplate>
    {
        private readonly RepositoryReference repositoryReference;
        private readonly ProjectTemplate projectTemplate;
        private readonly string repositoryLocation;

        public ProjectTemplateConverter(ProjectTemplate projectTemplate, string repositoryLocation, RepositoryReference repositoryReference = null)
        {
            this.projectTemplate = projectTemplate ?? throw new ArgumentNullException(nameof(projectTemplate));
            this.repositoryLocation = repositoryLocation ?? throw new ArgumentNullException(nameof(repositoryLocation));
            this.repositoryReference = repositoryReference ?? projectTemplate.Repository;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var projectJson = JObject.ReadFrom(reader) as JObject;

            foreach (var nameToken in projectJson.SelectTokens("$..name").Reverse().ToArray())
            {
                // (nameToken.Parent.Parent as JObject)?.SetProperty("title", nameToken.ToString());
                (nameToken.Parent.Parent as JObject)?.SetProperty("displayName", nameToken.ToString());
                nameToken.Parent.Remove(); // get rid of the name token as we don't need it anymore
            }

            var inputJsonSchemaToken = GenerateInputJsonSchema(projectJson, serializer);
            projectJson.SetProperty(nameof(ComponentTemplate.InputJsonSchema), inputJsonSchemaToken);

            return TeamCloudSerialize.MergeObject<ProjectTemplate>(projectJson.ToString(), projectTemplate);
        }
    }
}
