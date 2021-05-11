/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;

namespace TeamCloud.Git.Converter
{
    public sealed class ComponentTemplateConverter : YamlTemplateConverter<ComponentTemplate>
    {
        private readonly RepositoryReference repositoryReference;
        private readonly ProjectTemplate projectTemplate;
        private readonly string repositoryLocation;

        public ComponentTemplateConverter(ProjectTemplate projectTemplate, string repositoryLocation, RepositoryReference repositoryReference = null)
        {
            this.projectTemplate = projectTemplate ?? throw new ArgumentNullException(nameof(projectTemplate));
            this.repositoryLocation = repositoryLocation ?? throw new ArgumentNullException(nameof(repositoryLocation));
            this.repositoryReference = repositoryReference ?? projectTemplate.Repository;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var componentJson = JObject.ReadFrom(reader) as JObject;

            // augment the component json by some project template level data
            componentJson.SetProperty(nameof(ComponentTemplate.Id), repositoryLocation.ToGuid().ToString());
            componentJson.SetProperty(nameof(ComponentTemplate.ParentId), projectTemplate.Id);
            componentJson.SetProperty(nameof(ComponentTemplate.Organization), projectTemplate.Organization);
            componentJson.SetProperty(nameof(ComponentTemplate.Repository), repositoryReference);

            var inputJsonSchemaToken = GenerateInputJsonSchema(componentJson, serializer);
            componentJson.SetProperty(nameof(ComponentTemplate.InputJsonSchema), inputJsonSchemaToken);

            var permissionDictionaryToken = GeneratePermissionDictionary(componentJson, serializer);
            componentJson.SetProperty(nameof(ComponentTemplate.Permissions), permissionDictionaryToken);

            var componentType = Enum.Parse<ComponentType>(componentJson.GetValue("type", StringComparison.OrdinalIgnoreCase)?.ToString(), true) switch
            {
                ComponentType.Environment => typeof(ComponentEnvironmentTemplate),
                ComponentType.Repository => typeof(ComponentRepositoryTemplate),
                _ => typeof(ComponentTemplate)
            };

            return TeamCloudSerialize.DeserializeObject(componentJson.ToString(), componentType);
        }
    }
}
