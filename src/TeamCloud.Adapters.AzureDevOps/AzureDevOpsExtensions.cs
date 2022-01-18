/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.VisualStudio.Services.WebApi;
using TeamCloud.Http;

namespace TeamCloud.Adapters.AzureDevOps
{
    public static class AzureDevOpsExtensions
    {


        internal static bool TryMatch(this Regex expression, string input, out Match match)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            match = input is null
                ? Match.Empty
                : expression.Match(input);

            return match.Success;
        }

        internal static bool TryMatch(this Regex expression, string input, int groupNumber, out Group group)
        {
            group = TryMatch(expression, input, out var match) && groupNumber <= match.Groups.Count
                ? match.Groups[groupNumber]
                : null;

            return (group is not null);
        }

        internal static bool TryMatch(this Regex expression, string input, string groupName, out Group group)
        {
            group = TryMatch(expression, input, out var match)
                ? match.Groups.Cast<Group>().FirstOrDefault(g => g.Name == groupName)
                : null;

            return (group is not null);
        }

        internal static string UrlDecode(this string source)
            => HttpUtility.UrlDecode(source ?? string.Empty);

        internal static Dictionary<string, string[]> ToDictionary(this NameValueCollection collection)
            => collection.Cast<string>().ToDictionary(key => key, key => collection.GetValues(key));

        internal static bool TryGetValue(this NameValueCollection collection, string key, out string value)
        {
            value = collection.AllKeys.Contains(key)
                ? collection.Get(key) : default;

            return value != default;
        }

        internal static async Task<string> GetBadRequestErrorDescriptionAsync(this HttpResponseMessage httpResponseMessage)
        {
            try
            {
                var json = await httpResponseMessage
                    .ReadAsJsonAsync()
                    .ConfigureAwait(false);

                return json?.SelectToken("$..ErrorDescription")?.ToString() ?? json.ToString();
            }
            catch
            {
                return null;
            }
        }

        internal static bool IsActive(this OperationStatus operationStatus)
            => operationStatus == OperationStatus.NotSet || operationStatus == OperationStatus.Queued || operationStatus == OperationStatus.InProgress;

        internal static bool IsFinal(this OperationStatus operationStatus)
            => !operationStatus.IsActive();

        internal static Task WhenAll(this IEnumerable<Task> tasks)
            => Task.WhenAll(tasks);

        internal static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
            => Task.WhenAll(tasks);

        internal static async IAsyncEnumerable<T> AsContinuousCollectionAsync<T>(
            this IPagedList<T> page,
            Func<string, Task<IPagedList<T>>> getNextPage)
        {
            if (page is null)
                throw new ArgumentNullException(nameof(page));

            if (getNextPage is null)
                throw new ArgumentNullException(nameof(getNextPage));

            var currentPage = page;

            do
            {
                if (!(currentPage?.Any() ?? false))
                {
                    yield break;
                }

                foreach (var element in currentPage)
                {
                    yield return element;
                }

            } while (currentPage.ContinuationToken is not null &&
                    (currentPage = await getNextPage(currentPage.ContinuationToken).ConfigureAwait(false)) is not null);
        }

        internal static async IAsyncEnumerable<GraphUser> AsContinuousCollectionAsync(
            this PagedGraphUsers page,
            Func<string, Task<PagedGraphUsers>> getNextPage)
        {
            if (page is null)
                throw new ArgumentNullException(nameof(page));

            if (getNextPage is null)
                throw new ArgumentNullException(nameof(getNextPage));

            var currentPage = page;

            do
            {
                if (!(currentPage.GraphUsers?.Any() ?? false))
                {
                    yield break;
                }

                foreach (var element in currentPage.GraphUsers)
                {
                    yield return element;
                }

            } while ((currentPage.ContinuationToken?.Any() ?? false) &&
                    (currentPage = await getNextPage(currentPage.ContinuationToken.First()).ConfigureAwait(false)) is not null);
        }

        internal static async IAsyncEnumerable<GraphUser> ListAllUsersAsync(this GraphHttpClient client)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            var graphUsersPage = await client
                .ListUsersAsync()
                .ConfigureAwait(false);

            await foreach (var graphUser in graphUsersPage.AsContinuousCollectionAsync((continuationToken) => client.ListUsersAsync(continuationToken: continuationToken)))
                yield return graphUser;
        }

        internal static async IAsyncEnumerable<GraphGroup> AsContinuousCollectionAsync(
            this PagedGraphGroups page,
            Func<string, Task<PagedGraphGroups>> getNextPage)
        {
            if (page is null)
                throw new ArgumentNullException(nameof(page));

            if (getNextPage is null)
                throw new ArgumentNullException(nameof(getNextPage));

            var currentPage = page;

            do
            {
                if (!(currentPage.GraphGroups?.Any() ?? false))
                {
                    yield break;
                }

                foreach (var element in currentPage.GraphGroups)
                {
                    yield return element;
                }

            } while ((currentPage.ContinuationToken?.Any() ?? false) &&
                    (currentPage = await getNextPage(currentPage.ContinuationToken.First()).ConfigureAwait(false)) is not null);
        }

        internal static async IAsyncEnumerable<GraphGroup> ListAllGroupsAsync(this GraphHttpClient client, string scopeDescriptor = null)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            var graphGroupsPage = await client
                .ListGroupsAsync(scopeDescriptor)
                .ConfigureAwait(false);

            await foreach (var graphGroup in graphGroupsPage.AsContinuousCollectionAsync((continuationToken) => client.ListGroupsAsync(scopeDescriptor, continuationToken: continuationToken)))
                yield return graphGroup;
        }

        internal static async Task<string> GenerateProjectNameAsync(this ProjectHttpClient client, string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException($"'{nameof(projectName)}' cannot be null or whitespace.", nameof(projectName));

            foreach (var projectNameLookup in GetProjectNames())
            {
                TeamProject project;

                try
                {
                    project = await client.GetProject(projectNameLookup).ConfigureAwait(false);
                }
                catch
                {
                    project = null;
                }

                if (project is null)
                {
                    return projectNameLookup;
                }
            }

            throw new NotSupportedException($"The project name {projectName} is not unique.");

            IEnumerable<string> GetProjectNames()
            {
                yield return projectName.Trim();

                for (int i = 1; i < 10000; i++)
                    yield return $"{projectName.Trim()} {i:#0000}";
            }
        }

        internal static async Task<bool> HasCommitsAsync(this GitHttpClient client, Guid repositoryId)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            var commits = await client
                .GetCommitsAsync(repositoryId, new GitQueryCommitsCriteria() { Top = 1 })
                .ConfigureAwait(false);

            return commits.Any();
        }

        private static ProjectReference ToVariableProjectReference(this TeamProject project)
            => new ProjectReference() { Id = project.Id, Name = project.Name };

        internal static Task<VariableGroup> AddVariableGroupAsync(this TaskAgentHttpClient client, TeamProject project, string name, IDictionary<string, VariableValue> variables, string description = null)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            if (project is null)
                throw new ArgumentNullException(nameof(project));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));

            if (variables is null)
                throw new ArgumentNullException(nameof(variables));

            if (!variables.Any())
                throw new ArgumentException($"'{nameof(name)}' must contain at least one variable.", nameof(variables));

            return client.AddVariableGroupAsync(new VariableGroupParameters()
            {
                Name = name,
                Description = description,
                Type = "Vsts",
                Variables = variables,
                VariableGroupProjectReferences = new List<VariableGroupProjectReference>()
                {
                    new VariableGroupProjectReference()
                    {
                        Name = name,
                        Description = description,
                        ProjectReference = project.ToVariableProjectReference()
                    }
                }
            });
        }

        internal static Task<VariableGroup> UpdateVariableGroupAsync(this TaskAgentHttpClient client, TeamProject project, VariableGroup group)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            if (project is null)
                throw new ArgumentNullException(nameof(project));

            if (group is null)
                throw new ArgumentNullException(nameof(group));

            var variableGroupProjectReferenced = group.VariableGroupProjectReferences ?? new List<VariableGroupProjectReference>();

            if (!variableGroupProjectReferenced.Any(r => project.Id.Equals(r.ProjectReference?.Id)))
            {
                variableGroupProjectReferenced.Add(new VariableGroupProjectReference()
                {
                    Name = group.Name,
                    Description = group.Description,
                    ProjectReference = project.ToVariableProjectReference()
                });
            }

            return client.UpdateVariableGroupAsync(group.Id, new VariableGroupParameters()
            {
                Name = group.Name,
                Description = group.Description,
                Type = group.Type,
                Variables = group.Variables,
                VariableGroupProjectReferences = variableGroupProjectReferenced
            });
        }
    }
}
