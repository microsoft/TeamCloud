/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using TeamCloud.Git.Converter;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TeamCloud.Git.Services;

internal class DevOpsService
{
    private readonly IDeserializer yamlDeserializer;

    internal DevOpsService()
    {
        yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    }

    internal async Task<ProjectTemplate> UpdateProjectTemplateAsync(ProjectTemplate projectTemplate)
    {
        if (projectTemplate is null)
            throw new ArgumentNullException(nameof(projectTemplate));

        var repository = projectTemplate.Repository;

        var creds = new VssBasicCredential(string.Empty, repository.Token);
        using var connection = new VssConnection(new Uri(repository.BaselUrl), creds);
        using var client = connection.GetClient<GitHttpClient>();

        var commit = await client
            .GetCommitAsync(project: repository.Project, repository.Ref, repository.Repository)
            .ConfigureAwait(false);

        var result = await client
            .GetTreeAsync(project: repository.Project, repository.Repository, commit.TreeId, recursive: true)
            .ConfigureAwait(false);

        var projectYamlFile = await client
            .GetItemAsync(project: repository.Project, repository.Repository, Constants.ProjectYaml, download: true, versionDescriptor: repository.VersionDescriptor())
            .ConfigureAwait(false);

        var projectYaml = projectYamlFile.Content;
        var projectJson = new Deserializer().ToJson(projectYaml);

        TeamCloudSerialize.PopulateObject(projectJson, projectTemplate, new ProjectTemplateConverter(projectTemplate, projectYamlFile.Path));

        projectTemplate.Description = await CheckAndPopulateFileContentAsync(client, repository, result.TreeEntries, projectTemplate.Description)
            .ConfigureAwait(false);

        return projectTemplate;
    }

    internal async Task<ComponentTemplate> GetComponentTemplateAsync(ProjectTemplate projectTemplate, string templateId)
    {
        if (projectTemplate is null)
            throw new ArgumentNullException(nameof(projectTemplate));

        if (!Guid.TryParse(templateId, out var templateIdParsed))
            return null;

        var repository = projectTemplate.Repository;

        var creds = new VssBasicCredential(string.Empty, repository.Token);
        using var connection = new VssConnection(new Uri(repository.BaselUrl), creds);
        using var client = connection.GetClient<GitHttpClient>();

        var commit = await client
            .GetCommitAsync(project: repository.Project, repository.Ref, repository.Repository)
            .ConfigureAwait(false);

        var result = await client
            .GetTreeAsync(project: repository.Project, repository.Repository, commit.TreeId, recursive: true)
            .ConfigureAwait(false);

        var componentTemplateItem = result.TreeEntries
            .FirstOrDefault(ti => ti.Url.ToGuid().Equals(templateIdParsed));

        if (componentTemplateItem is null)
            return null;

        return await ResolveComponentTemplateAsync(projectTemplate, repository, client, result, componentTemplateItem)
            .ConfigureAwait(false);
    }

    internal async IAsyncEnumerable<ComponentTemplate> GetComponentTemplatesAsync(ProjectTemplate projectTemplate)
    {
        if (projectTemplate is null)
            throw new ArgumentNullException(nameof(projectTemplate));

        var repository = projectTemplate.Repository;

        var creds = new VssBasicCredential(string.Empty, repository.Token);
        using var connection = new VssConnection(new Uri(repository.BaselUrl), creds);
        using var client = connection.GetClient<GitHttpClient>();

        var commit = await client
            .GetCommitAsync(project: repository.Project, repository.Ref, repository.Repository)
            .ConfigureAwait(false);

        var result = await client
            .GetTreeAsync(project: repository.Project, repository.Repository, commit.TreeId, recursive: true)
            .ConfigureAwait(false);

        var componentTemplateTasks = result.TreeEntries
            .Where(t => t.RelativePath.EndsWith(Constants.ComponentYaml, StringComparison.Ordinal))
            .Select(te => ResolveComponentTemplateAsync(projectTemplate, repository, client, result, te))
            .ToList();

        while (componentTemplateTasks.Any())
        {
            var componentTemplateTask = await Task
                .WhenAny(componentTemplateTasks)
                .ConfigureAwait(false);

            try
            {
                yield return await componentTemplateTask.ConfigureAwait(false);
            }
            finally
            {
                componentTemplateTasks.Remove(componentTemplateTask);
            }
        }
    }

    private static async Task<ComponentTemplate> ResolveComponentTemplateAsync(ProjectTemplate projectTemplate, RepositoryReference repository, GitHttpClient client, GitTreeRef tree, GitTreeEntryRef treeItem)
    {
        var componentYamlFile = await client
            .GetItemAsync(project: repository.Project, repository.Repository, treeItem.RelativePath, download: true, versionDescriptor: repository.VersionDescriptor())
            .ConfigureAwait(false);

        var componentYaml = componentYamlFile.Content;
        var componentJson = new Deserializer().ToJson(componentYaml);
        var componentTemplate = TeamCloudSerialize.DeserializeObject<ComponentTemplate>(componentJson, new ComponentTemplateConverter(projectTemplate, componentYamlFile.Path));

        var folder = treeItem.RelativePath.Split(Constants.ComponentYaml).First().TrimEnd('/');

        componentTemplate.Description = await CheckAndPopulateFileContentAsync(client, repository, tree.TreeEntries, componentTemplate.Description, folder)
            .ConfigureAwait(false);

        return componentTemplate;
    }


    private static async Task<string> CheckAndPopulateFileContentAsync(GitHttpClient client, RepositoryReference repo, IEnumerable<GitTreeEntryRef> tree, string value, string folder = null)
    {
        if (string.IsNullOrEmpty(value) || !Uri.IsWellFormedUriString(value, UriKind.Relative))
            return value;

        var fileName = value.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        var filePath = string.IsNullOrEmpty(folder) ? fileName : $"{folder}/{fileName}";
        var fileItem = tree.FirstOrDefault(i => i.RelativePath.Equals(filePath, StringComparison.Ordinal));

        if (fileItem is null)
            return value;

        var fileStream = await client
            .GetItemContentAsync(project: repo.Project, repo.Repository, fileItem.RelativePath, download: true, versionDescriptor: repo.VersionDescriptor())
            .ConfigureAwait(false);

        using var streamReader = new StreamReader(fileStream);

        var file = await streamReader
            .ReadToEndAsync()
            .ConfigureAwait(false);

        return string.IsNullOrEmpty(file) ? value : file;
    }

    internal static async Task<RepositoryReference> GetRepositoryReferenceAsync(RepositoryReference repository)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (!string.IsNullOrEmpty(repository.Version) && Constants.ValidSha1.IsMatch(repository.Version))
        {
            repository.Ref = repository.Version;
            repository.Type = RepositoryReferenceType.Hash;

            return repository;
        }

        var creds = new VssBasicCredential(string.Empty, repository.Token);

        using var connection = new VssConnection(new Uri(repository.BaselUrl), creds);
        using var client = connection.GetClient<GitHttpClient>();

        var refs = await client
            .GetRefsAsync(project: repository.Project, repository.Repository, filterContains: repository.Version ?? "", peelTags: true)
            .ConfigureAwait(false);

        if (!(refs?.Any() ?? false))
            throw new Exception("No matching ref found");

        // use latest tag
        if (string.IsNullOrEmpty(repository.Version))
        {
            var tags = refs.Where(r => r.IsTag()).ToList();

            if (!(tags?.Any() ?? false)) // TODO: maybe just use master/main
                throw new Exception("No tags found");

            tags.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            var tag = tags.Last();

            repository.Ref = tag.PeeledObjectId;
            repository.Version = tag.Name;
            repository.Type = RepositoryReferenceType.Tag;
        }
        else
        {
            var gitRef = refs.First();

            repository.Ref = string.IsNullOrEmpty(gitRef.PeeledObjectId) ? gitRef.ObjectId : gitRef.PeeledObjectId;

            if (gitRef.IsTag())
                repository.Type = RepositoryReferenceType.Tag;

            if (gitRef.IsBranch())
                repository.Type = RepositoryReferenceType.Branch;
        }

        return repository;
    }
}
