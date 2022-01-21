/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using TeamCloud.Model.Data;
using TeamCloud.Serialization;
using YamlDotNet.Serialization;

namespace TeamCloud.Git;

public static class Extensions
{
    public static RepositoryReference ParseUrl(this RepositoryReference repository)
    {
        if (repository is null)
            throw new ArgumentNullException(nameof(repository));

        if (repository.IsGitHub())
        {
            repository.Provider = RepositoryProvider.GitHub;
            repository.ParseGitHubUrl();
        }
        else if (repository.IsDevOps())
        {
            repository.Provider = RepositoryProvider.DevOps;
            repository.ParseDevOpsUrl();
        }
        else
        {
            throw new NotSupportedException("Only GitHub and Azure DevOps git repositories are supported. Generic git repositories are not supported.");
        }

        return repository;
    }

    private static RepositoryReference ParseGitHubUrl(this RepositoryReference repository)
    {
        repository.Url = repository.Url
            .Replace("git@", "https://", StringComparison.OrdinalIgnoreCase)
            .Replace("github.com:", "github.com/", StringComparison.OrdinalIgnoreCase);

        if (repository.Url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            repository.Url = repository.Url[0..^4];

        var parts = repository.Url.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
        var index = parts.FindIndex(p => p.Contains("github.com", StringComparison.OrdinalIgnoreCase));

        if (index == -1 || parts.Count < index + 3)
            throw new Exception("Invalid GitHub Repository Url");

        repository.Organization = parts[index + 1];
        repository.Repository = parts[index + 2];
        repository.BaselUrl = repository.Url.Split(repository.Organization).First().TrimEnd('/');

        return repository;
    }

    private static RepositoryReference ParseDevOpsUrl(this RepositoryReference repository)
    {
        repository.Url = repository.Url
            .Replace("git@ssh.", "https://", StringComparison.OrdinalIgnoreCase)
            .Replace(":v3/", "/", StringComparison.OrdinalIgnoreCase);

        if (repository.Url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            repository.Url = repository.Url[0..^4];

        var parts = repository.Url.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
        var index = parts.FindIndex(p => p.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase)
                                      || p.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase));

        if (index == -1)
            throw new Exception("Invalid Azure DevOps Repository Url");

        if (!parts.Remove("_git"))
            repository.Url = repository.Url
                .Replace($"/{parts.Last()}", $"/_git/{parts.Last()}", StringComparison.Ordinal);

        if (parts[index].Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase))
            ++index;

        if (parts.Count < index + 3)
            throw new Exception("Invalid Azure DevOps Repository Url");

        repository.Organization = parts[index].Replace(".visualstudio.com", "", StringComparison.OrdinalIgnoreCase);
        repository.Project = parts[index + 1];
        repository.Repository = parts[index + 2];
        repository.BaselUrl = repository.Url.Split(repository.Project).First().TrimEnd('/');

        return repository;
    }

    private static bool IsGitHub(this RepositoryReference repo)
        => repo?.Url.Contains("github.com", StringComparison.OrdinalIgnoreCase) ?? throw new ArgumentNullException(nameof(repo));

    private static bool IsDevOps(this RepositoryReference repo)
        => repo is null
         ? throw new ArgumentNullException(nameof(repo))
         : repo.Url.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase)
        || repo.Url.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase);

    internal static bool IsBranch(this Microsoft.TeamFoundation.SourceControl.WebApi.GitRef gitRef)
        => gitRef?.Name?.StartsWith("refs/heads/", StringComparison.Ordinal) ?? throw new ArgumentNullException(nameof(gitRef));

    internal static bool IsTag(this Microsoft.TeamFoundation.SourceControl.WebApi.GitRef gitRef)
        => gitRef?.Name?.StartsWith("refs/tags/", StringComparison.Ordinal) ?? throw new ArgumentNullException(nameof(gitRef));

    internal static string ToString(this JSchema schema, Formatting formatting)
    {
        var sb = new StringBuilder();

        using var sw = new StringWriter(sb);
        using var jw = new JsonTextWriter(sw) { Formatting = formatting };

        schema.WriteTo(jw);

        return sb.ToString();
    }

    internal static string ToJson(this IDeserializer deserializer, string yaml)
    {
        if (deserializer is null)
            throw new ArgumentNullException(nameof(deserializer));

        if (string.IsNullOrWhiteSpace(yaml))
            throw new ArgumentException($"'{nameof(yaml)}' cannot be null or whitespace.", nameof(yaml));

        var data = deserializer.Deserialize(new StringReader(yaml));

        return TeamCloudSerialize.SerializeObject(data, new TeamCloudSerializerSettings()
        {
            // ensure we disable the type name handling to get clean json
            TypeNameHandling = TypeNameHandling.None
        });
    }

    internal static void SetProperty(this JObject json, string propertyName, object propertyValue)
    {
        propertyName = TeamCloudNamingStrategy.Default.GetPropertyName(propertyName, false);

        if (propertyValue is null)
        {
            json.Property(propertyName, StringComparison.OrdinalIgnoreCase)?.Remove();
        }
        else
        {
            if (!(propertyValue is JToken propertyToken))
            {
                propertyToken = (propertyValue.GetType().IsValueType || propertyValue.GetType() == typeof(string))
                  ? new JValue(propertyValue)
                  : (JToken)JObject.FromObject(propertyValue);
            }

            if (json.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out var value))
                value.Replace(propertyToken);
            else
                json.Add(propertyName, propertyToken);
        }
    }
}
