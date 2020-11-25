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
using Octokit.Internal;
using TeamCloud.Git.Caching;
using TeamCloud.Git.Data;
using TeamCloud.Model.Data;
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

        // internal async Task<ProjectTemplate> GetProjectTemplateAsync(RepositoryReference repository)
        // {
        //     if (repository is null)
        //         throw new ArgumentNullException(nameof(repository));

        //     if (!string.IsNullOrEmpty(repository.Token))
        //         client.Credentials = new Credentials(repository.Token);

        //     var trees = await client.Git.Tree
        //         .GetRecursive(repository.Organization, repository.Repository, repository.Ref)
        //         .ConfigureAwait(false);

        //     var template = await GetProjectTemplateAsync(repository, trees.Tree)
        //         .ConfigureAwait(false);

        //     return template;
        // }

        // private async Task<ProjectTemplate> GetProjectTemplateAsync(RepositoryReference repo, IReadOnlyList<TreeItem> tree)
        // {
        //     var projectYamlFile = await client.Repository.Content
        //         .GetAllContents(repo.Organization, repo.Repository, Constants.ProjectYaml)
        //         .ConfigureAwait(false);

        //     var projectYaml = yamlDeserializer.Deserialize<ProjectYaml>(projectYamlFile.First().Content);

        //     projectYaml.Description = await CheckAndPopulateFileContentAsync(repo, tree, projectYaml.Description)
        //         .ConfigureAwait(false);

        //     return projectYaml.ToProjectTemplate(repo);
        // }


        internal async Task<ProjectTemplate> UpdateProjectTemplateAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            var repository = projectTemplate.Repository;

            if (!string.IsNullOrEmpty(repository.Token))
                client.Credentials = new Credentials(repository.Token);

            var trees = await client.Git.Tree
                .GetRecursive(repository.Organization, repository.Repository, repository.Ref)
                .ConfigureAwait(false);

            return await UpdateProjectTemplateAsync(projectTemplate, trees.Tree)
                .ConfigureAwait(false);
        }

        private async Task<ProjectTemplate> UpdateProjectTemplateAsync(ProjectTemplate projectTemplate, IReadOnlyList<TreeItem> tree)
        {
            var repository = projectTemplate.Repository;

            var projectYamlFile = await client.Repository.Content
                .GetAllContents(repository.Organization, repository.Repository, Constants.ProjectYaml)
                .ConfigureAwait(false);

            var projectYaml = yamlDeserializer.Deserialize<ProjectYaml>(projectYamlFile.First().Content);

            projectYaml.Description = await CheckAndPopulateFileContentAsync(repository, tree, projectYaml.Description)
                .ConfigureAwait(false);

            return projectTemplate.UpdateFromYaml(projectYaml);
        }


        internal async Task<List<ComponentTemplate>> GetComponentTemplatesAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            var repository = projectTemplate.Repository;

            if (!string.IsNullOrEmpty(repository.Token))
                client.Credentials = new Credentials(repository.Token);

            var trees = await client.Git.Tree
                .GetRecursive(repository.Organization, repository.Repository, repository.Ref)
                .ConfigureAwait(false);

            return await GetComponentTemplatesAsync(projectTemplate, trees.Tree)
                .ConfigureAwait(false);
        }

        private async Task<List<ComponentTemplate>> GetComponentTemplatesAsync(ProjectTemplate projectTemplate, IReadOnlyList<TreeItem> tree)
        {
            var repository = projectTemplate.Repository;

            var componenetTemplates = new List<ComponentTemplate>();

            var componentItems = tree.Where(t => t.Path.EndsWith(Constants.ComponentYaml, StringComparison.Ordinal));

            foreach (var componentItem in componentItems)
            {
                var folder = componentItem.Path.Split(Constants.ComponentYaml).First().TrimEnd('/');

                var componentFiles = await client.Repository.Content
                    .GetAllContents(repository.Organization, repository.Repository, componentItem.Path)
                    .ConfigureAwait(false);

                var componentYaml = yamlDeserializer.Deserialize<ComponentYaml>(componentFiles.First().Content);

                componentYaml.Description = await CheckAndPopulateFileContentAsync(repository, tree, componentYaml.Description, folder)
                    .ConfigureAwait(false);

                foreach (var parameter in componentYaml.Parameters.Where(p => p.Type == JSchemaType.String))
                    parameter.Value = await CheckAndPopulateFileContentAsync(repository, tree, parameter.StringValue, folder)
                        .ConfigureAwait(false);

                componenetTemplates.Add(componentYaml.ToComponentTemplate(projectTemplate, folder));
            }

            return componenetTemplates;
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
                var latest = await client
                    .Repository
                    .Release
                    .GetLatest(repository.Organization, repository.Repository)
                    .ConfigureAwait(false);

                if (latest?.TagName is null)
                    throw new NotFoundException("No releases found", System.Net.HttpStatusCode.NotFound);

                repository.Version = latest.TagName;
            }

            try
            {
                var tag = await client.Git.Reference
                    .Get(repository.Organization, repository.Repository, Constants.TagRef(repository.Version))
                    .ConfigureAwait(false);

                repository.Ref = tag.Object?.Sha ?? Constants.TagRef(repository.Version);
                repository.Type = RepositoryReferenceType.Tag;

                return repository;
            }
            catch (NotFoundException)
            {
                try
                {
                    var branch = await client.Git.Reference
                        .Get(repository.Organization, repository.Repository, Constants.BranchRef(repository.Version))
                        .ConfigureAwait(false);

                    repository.Ref = branch.Object?.Sha ?? Constants.BranchRef(repository.Version);
                    repository.Type = RepositoryReferenceType.Branch;

                    return repository;
                }
                catch (NotFoundException)
                {
                    throw;
                }
            }
        }
    }
}
