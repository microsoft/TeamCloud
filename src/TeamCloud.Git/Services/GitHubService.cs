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
    internal class GitHubService
    {
        private const string ProductHeaderName = "TeamCloud";
        private static readonly string ProductHeaderVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private readonly GitHubClient client;
        private readonly IDeserializer yamlDeserializer;

        internal GitHubService()
        {
            client = new GitHubClient(new ProductHeaderValue(ProductHeaderName, ProductHeaderVersion));
            yamlDeserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        }

        internal async Task<ProjectTemplate> GetProjectTemplateAsync(RepositoryReference repository)
        {
            if (repository is null)
                throw new ArgumentNullException(nameof(repository));

            if (!string.IsNullOrEmpty(repository.Token))
                client.Credentials = new Credentials(repository.Token);

            var trees = await client.Git.Tree
                .GetRecursive(repository.Organization, repository.Repository, repository.Ref)
                .ConfigureAwait(false);

            var template = await GetProjectTemplateAsync(repository, trees.Tree)
                .ConfigureAwait(false);

            return template;
        }

        internal async Task<List<ComponentOffer>> GetComponentOffersAsync(RepositoryReference repository)
        {
            if (repository is null)
                throw new ArgumentNullException(nameof(repository));

            if (!string.IsNullOrEmpty(repository.Token))
                client.Credentials = new Credentials(repository.Token);

            var trees = await client.Git.Tree
                .GetRecursive(repository.Organization, repository.Repository, repository.Ref)
                .ConfigureAwait(false);

            var offers = await GetOffersAsync(repository, trees.Tree)
                .ConfigureAwait(false);

            return offers;
        }

        // internal async Task<ProjectTemplateRepository> GetProjectTemplateRepositoryAsync(RepositoryReference repository)
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

        //     var offers = await GetOffersAsync(repository, trees.Tree)
        //         .ConfigureAwait(false);

        //     return new ProjectTemplateRepository
        //     {
        //         Template = template,
        //         Offers = offers
        //     };
        // }

        private async Task<ProjectTemplate> GetProjectTemplateAsync(RepositoryReference repo, IReadOnlyList<TreeItem> tree)
        {
            var projectYamlFile = await client.Repository.Content
                .GetAllContents(repo.Organization, repo.Repository, Constants.ProjectYaml)
                .ConfigureAwait(false);

            var projectYaml = yamlDeserializer.Deserialize<ProjectYaml>(projectYamlFile.First().Content);

            projectYaml.Description = await CheckAndPopulateFileContentAsync(repo, tree, projectYaml.Description)
                .ConfigureAwait(false);

            return projectYaml.ToProjectTemplate(repo);
        }

        private async Task<List<ComponentOffer>> GetOffersAsync(RepositoryReference repo, IReadOnlyList<TreeItem> tree)
        {
            var offers = new List<ComponentOffer>();

            var componentItems = tree.Where(t => t.Path.EndsWith(Constants.ComponentYaml, StringComparison.Ordinal));

            foreach (var componentItem in componentItems)
            {
                var folder = componentItem.Path.Split(Constants.ComponentYaml).First().TrimEnd('/');

                var componentFiles = await client.Repository.Content
                    .GetAllContents(repo.Organization, repo.Repository, componentItem.Path)
                    .ConfigureAwait(false);

                var componentYaml = yamlDeserializer.Deserialize<ComponentYaml>(componentFiles.First().Content);

                componentYaml.Description = await CheckAndPopulateFileContentAsync(repo, tree, componentYaml.Description, folder)
                    .ConfigureAwait(false);

                foreach (var parameter in componentYaml.Parameters.Where(p => p.Type == JSchemaType.String))
                    parameter.Value = await CheckAndPopulateFileContentAsync(repo, tree, parameter.StringValue, folder)
                        .ConfigureAwait(false);

                offers.Add(componentYaml.ToOffer(repo, folder));
            }

            return offers;
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
