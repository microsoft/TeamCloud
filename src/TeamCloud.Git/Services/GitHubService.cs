/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
using Octokit;
using TeamCloud.Git.Data;
using TeamCloud.Model.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TeamCloud.Git.Services
{
    public class GitHubService : IRepositoryService
    {
        private const string ProductHeaderName = "TeamCloud";
        private static readonly string ProductHeaderVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private readonly GitHubClient client;
        private readonly IDeserializer yamlDeserializer;

        public GitHubService()
        {
            client = new GitHubClient(new ProductHeaderValue(ProductHeaderName, ProductHeaderVersion));
            yamlDeserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        }

        private static (string org, string repo) GetRepoDetails(string url)
        {
            if (url.Contains("github.com:", StringComparison.OrdinalIgnoreCase))
                url = url.Replace("github.com:", "github.com/", StringComparison.OrdinalIgnoreCase);

            var parts = url.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
            var index = parts.FindIndex(p => p.Contains("github.com", StringComparison.OrdinalIgnoreCase));

            if (index == -1 || parts.Count < index + 3)
                throw new Exception("Invalid Repository Url");

            var org = parts[index + 1];
            var repo = parts[index + 2].Replace(".git", "", StringComparison.OrdinalIgnoreCase);

            return (org, repo);
        }

        public async Task<ProjectTemplateDefinition> GetProjectTemplateDefinitionAsync(RepositoryReference repository)
        {
            if (repository is null)
                throw new ArgumentNullException(nameof(repository));

            var (org, repo) = GetRepoDetails(repository.Url);

            if (!string.IsNullOrEmpty(repository.Token))
                client.Credentials = new Credentials(repository.Token);

            var sha = await ResolveRefAsync(org, repo, repository.Version)
                .ConfigureAwait(false);

            var trees = await client.Git.Tree
                .GetRecursive(org, repo, sha)
                .ConfigureAwait(false);

            var template = await GetProjectTemplateAsync(repository, sha, org, repo, trees.Tree)
                .ConfigureAwait(false);

            var offers = await GetOffersAsync(org, repo, trees.Tree)
                .ConfigureAwait(false);

            return new ProjectTemplateDefinition
            {
                Template = template,
                Offers = offers
            };
        }

        private async Task<ProjectTemplate> GetProjectTemplateAsync(RepositoryReference repository, string sha, string org, string repo, IReadOnlyList<TreeItem> tree)
        {
            var projectYamlFile = await client.Repository.Content
                .GetAllContents(org, repo, Constants.ProjectYaml)
                .ConfigureAwait(false);

            var projectYaml = yamlDeserializer.Deserialize<ProjectYaml>(projectYamlFile.First().Content);

            projectYaml.Description = await CheckAndPopulateFileContentAsync(org, repo, tree, projectYaml.Description)
                .ConfigureAwait(false);

            return projectYaml.ToProjectTemplate(repository, sha);
        }

        private async Task<List<ComponentOffer>> GetOffersAsync(string org, string repo, IReadOnlyList<TreeItem> tree)
        {
            var offers = new List<ComponentOffer>();

            var componentItems = tree.Where(t => t.Path.EndsWith(Constants.ComponentYaml, StringComparison.Ordinal));

            foreach (var componentItem in componentItems)
            {
                var folder = componentItem.Path.Split(Constants.ComponentYaml).First().TrimEnd('/');

                var componentFiles = await client.Repository.Content
                    .GetAllContents(org, repo, componentItem.Path)
                    .ConfigureAwait(false);

                var componentYaml = yamlDeserializer.Deserialize<ComponentYaml>(componentFiles.First().Content);

                componentYaml.Description = await CheckAndPopulateFileContentAsync(org, repo, tree, componentYaml.Description, folder)
                    .ConfigureAwait(false);

                foreach (var parameter in componentYaml.Parameters.Where(p => p.Type == JSchemaType.String))
                    parameter.Value = await CheckAndPopulateFileContentAsync(org, repo, tree, parameter.StringValue, folder)
                        .ConfigureAwait(false);

                offers.Add(componentYaml.ToOffer(repo, folder));
            }

            return offers;
        }

        private async Task<string> CheckAndPopulateFileContentAsync(string org, string repo, IReadOnlyList<TreeItem> tree, string value, string folder = null)
        {
            if (string.IsNullOrEmpty(value) || !Uri.IsWellFormedUriString(value, UriKind.Relative))
                return value;

            var fileName = value.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            var filePath = string.IsNullOrEmpty(folder) ? fileName : $"{folder}/{fileName}";
            var fileItem = tree.FirstOrDefault(i => i.Path.Equals(filePath, StringComparison.Ordinal));

            if (fileItem is null)
                return value;

            try
            {
                var files = await client.Repository.Content
                    .GetAllContents(org, repo, fileItem.Path)
                    .ConfigureAwait(false);

                return files.FirstOrDefault()?.Content ?? value;
            }
            catch (NotFoundException)
            {
                return value;
            }
        }

        private async Task<string> ResolveRefAsync(string owner, string repo, string version)
        {
            if (!string.IsNullOrEmpty(version) && Constants.ValidSha1.IsMatch(version))
                return version;

            // use latest release
            if (string.IsNullOrEmpty(version))
            {
                var latest = await client
                    .Repository
                    .Release
                    .GetLatest(owner, repo)
                    .ConfigureAwait(false);

                if (latest?.TagName is null)
                    throw new NotFoundException("No releases found", System.Net.HttpStatusCode.NotFound);

                version = latest.TagName;
            }

            try
            {
                var tag = await client.Git.Reference
                    .Get(owner, repo, Constants.TagRef(version))
                    .ConfigureAwait(false);

                return tag.Object?.Sha ?? Constants.TagRef(version);
            }
            catch (NotFoundException)
            {
                try
                {
                    var branch = await client.Git.Reference
                        .Get(owner, repo, Constants.BranchRef(version))
                        .ConfigureAwait(false);

                    return branch.Object?.Sha ?? Constants.BranchRef(version);
                }
                catch (NotFoundException)
                {
                    throw;
                }
            }
        }
    }
}
