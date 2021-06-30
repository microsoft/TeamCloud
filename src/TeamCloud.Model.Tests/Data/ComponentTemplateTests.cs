/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TeamCloud.Git.Converter;
using TeamCloud.Serialization;
using Xunit;
using YamlDotNet.Serialization;

namespace TeamCloud.Model.Data
{
    public sealed class ComponentTemplateTests
    {
        protected string GetComponentDefinitionAsJson(string suffix)
        {
            var resourceName = GetType().Assembly
                .GetManifestResourceNames()
                .FirstOrDefault(name => name.Equals($"{GetType().FullName}_{suffix}.yaml", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(resourceName))
                throw new NullReferenceException($"Component definition not found.");

            using var resourceStream = GetType().Assembly
                .GetManifestResourceStream(resourceName);

            using var resourceReader = new StreamReader(resourceStream);

            var data = new Deserializer().Deserialize(resourceReader);

            return TeamCloudSerialize.SerializeObject(data, new TeamCloudSerializerSettings()
            {
                // ensure we disable the type name handling to get clean json
                TypeNameHandling = TypeNameHandling.None
            });
        }

        [Fact]
        public void DeserializeEnvironment()
        {
            var repositoryReference = new RepositoryReference()
            {
                Url = "https://github.com/foo/bar.git",
                Provider = RepositoryProvider.GitHub,
                Type = RepositoryReferenceType.Unknown
            };

            var projectTemplate = new ProjectTemplate()
            {
                Id = Guid.NewGuid().ToString(),
                Organization = Guid.NewGuid().ToString(),
                Repository = repositoryReference
            };

            var componentPath = "/foo/bar/component.yaml";
            var componentJson = GetComponentDefinitionAsJson("environment");

            var componentTemplate = TeamCloudSerialize
                .DeserializeObject<ComponentTemplate>(componentJson, new ComponentTemplateConverter(projectTemplate, componentPath));

            Assert.NotNull(componentTemplate);
            Assert.IsAssignableFrom<ComponentEnvironmentTemplate>(componentTemplate);

            Assert.NotNull(componentTemplate.Configuration);
            Assert.IsAssignableFrom<ComponentEnvironmentConfiguration>(componentTemplate.Configuration);
        }

        [Fact]
        public void DeserializeRepository()
        {
            var repositoryReference = new RepositoryReference()
            {
                Url = "https://github.com/foo/bar.git",
                Provider = RepositoryProvider.GitHub,
                Type = RepositoryReferenceType.Unknown
            };

            var projectTemplate = new ProjectTemplate()
            {
                Id = Guid.NewGuid().ToString(),
                Organization = Guid.NewGuid().ToString(),
                Repository = repositoryReference
            };

            var componentPath = "/foo/bar/component.yaml";
            var componentJson = GetComponentDefinitionAsJson("repository");

            var componentTemplate = TeamCloudSerialize
                .DeserializeObject<ComponentTemplate>(componentJson, new ComponentTemplateConverter(projectTemplate, componentPath));

            Assert.NotNull(componentTemplate);
            Assert.IsAssignableFrom<ComponentRepositoryTemplate>(componentTemplate);

            Assert.NotNull(componentTemplate.Configuration);
            Assert.IsAssignableFrom<ComponentRepositoryConfiguration>(componentTemplate.Configuration);
        }
    }
}
