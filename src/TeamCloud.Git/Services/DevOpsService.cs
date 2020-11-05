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
using Newtonsoft.Json.Schema;
using TeamCloud.Git.Data;
using TeamCloud.Model.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TeamCloud.Git.Services
{
    public class DevOpsService : IRepositoryService
    {
        private readonly IDeserializer yamlDeserializer;

        public DevOpsService()
        {
            yamlDeserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        }

        private static (string baseUrl, string org, string project, string repo) GetRepoDetails(string url)
        {
            var parts = url.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
            var index = parts.FindIndex(p => p.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase));

            if (index > -1)
            {
                if (parts.Count < index + 4)
                    throw new Exception("Invalid Repository Url");

                var org = parts[index + 1];
                var project = parts[index + 2];
                var hasGit = parts[index + 3].Equals("_git", StringComparison.OrdinalIgnoreCase);
                var repo = parts[index + (hasGit ? 4 : 3)].Replace(".git", "", StringComparison.OrdinalIgnoreCase);
                var baseUrl = url.Split(project).First().TrimEnd('/');

                return (baseUrl, org, project, repo);
            }
            else
            {
                index = parts.FindIndex(p => p.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase));

                if (index == -1 || parts.Count < index + 3)
                    throw new Exception("Invalid Repository Url");

                var org = parts[index].Split(".visualstudio.com").First();
                var project = parts[index + 1];
                var hasGit = parts[index + 2].Equals("_git", StringComparison.OrdinalIgnoreCase);
                var repo = parts[index + (hasGit ? 3 : 2)].Replace(".git", "", StringComparison.OrdinalIgnoreCase);
                var baseUrl = url.Split(project).First().TrimEnd('/');

                return (baseUrl, org, project, repo);
            }
        }


        public async Task<ProjectTemplateDefinition> GetProjectTemplateDefinitionAsync(RepositoryReference repository)
        {
            if (repository is null)
                throw new ArgumentNullException(nameof(repository));

            var (baseUrl, org, project, repo) = GetRepoDetails(repository.Url);

            var creds = new VssBasicCredential(string.Empty, repository.Token);
            using var connection = new VssConnection(new Uri(baseUrl), creds);
            using var client = connection.GetClient<GitHttpClient>();

            var commitId = await ResolveRefAsync(client, project, repo, repository.Version)
                .ConfigureAwait(false);

            var commit = await client
                .GetCommitAsync(project: project, commitId, repo)
                .ConfigureAwait(false);

            var tree = await client
                .GetTreeAsync(project: project, repo, commit.TreeId, recursive: true)
                .ConfigureAwait(false);

            var version = new GitVersionDescriptor
            {
                Version = commitId,
                VersionType = GitVersionType.Commit
            };

            var template = await GetProjectTemplateAsync(client, version, repository, commitId, project, repo, tree.TreeEntries)
                .ConfigureAwait(false);

            var offers = await GetOffersAsync(client, version, project, repo, tree.TreeEntries)
                .ConfigureAwait(false);

            return new ProjectTemplateDefinition
            {
                Template = template,
                Offers = offers
            };
        }

        private async Task<ProjectTemplate> GetProjectTemplateAsync(GitHttpClient client, GitVersionDescriptor version, RepositoryReference repository, string commitId, string project, string repo, IEnumerable<GitTreeEntryRef> tree)
        {
            var projectYamlFileStream = await client
                .GetItemContentAsync(project: project, repo, Constants.ProjectYaml, download: true, versionDescriptor: version)
                .ConfigureAwait(false);

            using var streamReader = new StreamReader(projectYamlFileStream);

            var projectYamlFile = await streamReader
                .ReadToEndAsync()
                .ConfigureAwait(false);

            var projectYaml = yamlDeserializer.Deserialize<ProjectYaml>(projectYamlFile);

            projectYaml.Description = await CheckAndPopulateFileContentAsync(client, version, project, repo, tree, projectYaml.Description)
                .ConfigureAwait(false);

            return projectYaml.ToProjectTemplate(repository, commitId);
        }

        private async Task<List<ComponentOffer>> GetOffersAsync(GitHttpClient client, GitVersionDescriptor version, string project, string repo, IEnumerable<GitTreeEntryRef> tree)
        {
            var offers = new List<ComponentOffer>();

            var componentItems = tree.Where(t => t.RelativePath.EndsWith(Constants.ComponentYaml, StringComparison.Ordinal));

            foreach (var componentItem in componentItems)
            {
                var folder = componentItem.RelativePath.Split(Constants.ComponentYaml).First().TrimEnd('/');

                var componentFileStream = await client
                    .GetItemContentAsync(project: project, repo, componentItem.RelativePath, download: true, versionDescriptor: version)
                    .ConfigureAwait(false);

                using var streamReader = new StreamReader(componentFileStream);

                var componentFile = await streamReader
                    .ReadToEndAsync()
                    .ConfigureAwait(false);

                var componentYaml = yamlDeserializer.Deserialize<ComponentYaml>(componentFile);

                componentYaml.Description = await CheckAndPopulateFileContentAsync(client, version, project, repo, tree, componentYaml.Description, folder)
                    .ConfigureAwait(false);

                foreach (var parameter in componentYaml.Parameters.Where(p => p.Type == JSchemaType.String))
                    parameter.Value = await CheckAndPopulateFileContentAsync(client, version, project, repo, tree, parameter.StringValue, folder)
                        .ConfigureAwait(false);

                offers.Add(componentYaml.ToOffer(repo, folder));
            }

            return offers;
        }

        private static async Task<string> CheckAndPopulateFileContentAsync(GitHttpClient client, GitVersionDescriptor version, string project, string repo, IEnumerable<GitTreeEntryRef> tree, string value, string folder = null)
        {
            if (string.IsNullOrEmpty(value) || !Uri.IsWellFormedUriString(value, UriKind.Relative))
                return value;

            var fileName = value.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            var filePath = string.IsNullOrEmpty(folder) ? fileName : $"{folder}/{fileName}";
            var fileItem = tree.FirstOrDefault(i => i.RelativePath.Equals(filePath, StringComparison.Ordinal));

            if (fileItem is null)
                return value;

            var fileStream = await client
                .GetItemContentAsync(project: project, repo, fileItem.RelativePath, download: true, versionDescriptor: version)
                .ConfigureAwait(false);

            using var streamReader = new StreamReader(fileStream);

            var file = await streamReader
                .ReadToEndAsync()
                .ConfigureAwait(false);

            return string.IsNullOrEmpty(file) ? value : file;
        }

        private static async Task<string> ResolveRefAsync(GitHttpClient client, string project, string repo, string version)
        {
            if (!string.IsNullOrEmpty(version) && Constants.ValidSha1.IsMatch(version))
                return version;

            var refs = await client
                .GetRefsAsync(project: project, repo, filterContains: version ?? "", peelTags: true)
                .ConfigureAwait(false);

            if (!(refs?.Any() ?? false))
                throw new Exception("No matching ref found");

            // use latest tag
            if (string.IsNullOrEmpty(version))
            {
                var tags = refs.Where(r => r.IsTag()).ToList();

                if (!(tags?.Any() ?? false)) // TODO: maybe just use master/main
                    throw new Exception("No tags found");

                tags.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

                return tags.Last().PeeledObjectId;
            }

            var gitRef = refs.First();

            return string.IsNullOrEmpty(gitRef.PeeledObjectId) ? gitRef.ObjectId : gitRef.PeeledObjectId;
        }
    }
}
