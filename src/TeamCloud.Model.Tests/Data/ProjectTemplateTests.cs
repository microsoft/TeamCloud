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

namespace TeamCloud.Model.Data;

public sealed class ProjectTemplateTests
{
    public string GetProjectDefinitionAsJson(string suffix)
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
    public void DeserializeProject()
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
            DisplayName = "Foo",
            Repository = repositoryReference
        };

        var porjectPath = "/foo/bar/project.yaml";
        var projectJson = GetProjectDefinitionAsJson("project");

        var projectTemplate2 = TeamCloudSerialize
            .DeserializeObject<ProjectTemplate>(projectJson, new ProjectTemplateConverter(projectTemplate, porjectPath));

        Assert.NotNull(projectTemplate2);
        Assert.IsAssignableFrom<ProjectTemplate>(projectTemplate2);
    }
}
