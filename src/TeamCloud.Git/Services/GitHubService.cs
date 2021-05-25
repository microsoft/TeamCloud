/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;
using TeamCloud.Git.Caching;
using TeamCloud.Git.Converter;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TeamCloud.Git.Services
{
    internal class GitHubService
    {
        private const string ProductHeaderName = "TeamCloud";
        private static readonly string ProductHeaderVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private readonly GitHubClient client;
        private readonly IDeserializer yamlDeserializer;

        internal GitHubService(IRepositoryCache cache)
        {
            var connection = new Connection(
                new ProductHeaderValue(ProductHeaderName, ProductHeaderVersion),
                new GitHubCache(new HttpClientAdapter(HttpMessageHandlerFactory.CreateDefault), cache));

            client = new GitHubClient(connection);

            yamlDeserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        }

        internal async Task<ProjectTemplate> UpdateProjectTemplateAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            var repository = projectTemplate.Repository;

            if (!string.IsNullOrEmpty(repository.Token))
                client.Credentials = new Credentials(repository.Token);

            var projectYamlFiles = await client.Repository.Content
                .GetAllContents(repository.Organization, repository.Repository, Constants.ProjectYaml)
                .ConfigureAwait(false);

            var projectYamlFile = projectYamlFiles.First();
            var projectYaml = projectYamlFile.Content;
            var projectJson = new Deserializer().ToJson(projectYaml);

            projectTemplate = TeamCloudSerialize.DeserializeObject<ProjectTemplate>(projectJson, new ProjectTemplateConverter(projectTemplate, projectYamlFile.Url));

            var result = await client.Git.Tree
                .GetRecursive(repository.Organization, repository.Repository, repository.Ref)
                .ConfigureAwait(false);

            projectTemplate.Description = await CheckAndPopulateFileContentAsync(repository, result.Tree, projectTemplate.Description)
                .ConfigureAwait(false);

            return projectTemplate;
        }


        internal async IAsyncEnumerable<ComponentTemplate> GetComponentTemplatesAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            var repository = projectTemplate.Repository;

            if (!string.IsNullOrEmpty(repository.Token))
                client.Credentials = new Credentials(repository.Token);

            var result = await client.Git.Tree
                .GetRecursive(repository.Organization, repository.Repository, repository.Ref)
                .ConfigureAwait(false);

            var componentTemplates = result.Tree
                .Where(ti => ti.Path.EndsWith(Constants.ComponentYaml, StringComparison.Ordinal))
                .Select(ti => ResolveComponentTemplate(ti))
                .ToAsyncEnumerable();

            await foreach (var componentTemplate in componentTemplates)
                yield return componentTemplate;

            async Task<ComponentTemplate> ResolveComponentTemplate(TreeItem componentItem)
            {
                var componentYamlFiles = await client.Repository.Content
                    .GetAllContents(repository.Organization, repository.Repository, componentItem.Path)
                    .ConfigureAwait(false);

                var componentYamlFile = componentYamlFiles.First();
                var componentYaml = componentYamlFile.Content;
                var componentJson = new Deserializer().ToJson(componentYaml);

                var componentTemplate = TeamCloudSerialize.DeserializeObject<ComponentTemplate>(componentJson, new ComponentTemplateConverter(projectTemplate, componentItem.Url));

                componentTemplate.Folder = Regex.Replace(componentItem.Path, $"/{Constants.ComponentYaml}$", string.Empty, RegexOptions.IgnoreCase);
                componentTemplate.Description = await CheckAndPopulateFileContentAsync(repository, result.Tree, componentTemplate.Description, componentTemplate.Folder).ConfigureAwait(false);

                return componentTemplate;
            }
        }


        private async Task<string> CheckAndPopulateFileContentAsync(RepositoryReference repo, IReadOnlyList<TreeItem> tree, string value, string folder = null)
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
                    .GetAllContents(repo.Organization, repo.Repository, fileItem.Path)
                    .ConfigureAwait(false);

                return files.FirstOrDefault()?.Content ?? value;
            }
            catch (NotFoundException)
            {
                return value;
            }
        }


        internal async Task<RepositoryReference> GetRepositoryReferenceAsync(RepositoryReference repository)
        {
            if (repository is null)
                throw new ArgumentNullException(nameof(repository));

            if (!string.IsNullOrEmpty(repository.Version) && Constants.ValidSha1.IsMatch(repository.Version))
            {
                repository.Ref = repository.Version;
                repository.Type = RepositoryReferenceType.Hash;

                return repository;
            }

            if (!string.IsNullOrEmpty(repository.Token))
                client.Credentials = new Credentials(repository.Token);

            // use latest release
            if (string.IsNullOrEmpty(repository.Version))
            {
                var releases = await client
                    .Repository
                    .Release
                    .GetAll(repository.Organization, repository.Repository)
                    .ConfigureAwait(false);

                if (!releases.Any())
                    throw new NotFoundException("No releases found", System.Net.HttpStatusCode.NotFound);

                repository.Version = releases
                    .OrderByDescending(rel => rel.CreatedAt)
                    .First()
                    .TagName;
            }

            var references = await client.Git.Reference
                .GetAll(repository.Organization, repository.Repository)
                .ConfigureAwait(false);

            var reference = references
                .FirstOrDefault(r => r.Ref == Constants.BranchRef(repository.Version) || r.Ref == Constants.TagRef(repository.Version));

            switch (reference?.Ref)
            {
                case string branchVersion when branchVersion == Constants.BranchRef(repository.Version):

                    repository.Ref = reference.Object?.Sha ?? Constants.BranchRef(repository.Version);
                    repository.Type = RepositoryReferenceType.Branch;

                    break;

                case string tagVersion when tagVersion == Constants.TagRef(repository.Version):

                    repository.Ref = reference.Object?.Sha ?? Constants.TagRef(repository.Version);
                    repository.Type = RepositoryReferenceType.Tag;

                    break;

                default:

                    throw new NotFoundException($"No branch/tag found by version {repository.Version}", System.Net.HttpStatusCode.NotFound);
            }

            return repository;
        }
    }
}
