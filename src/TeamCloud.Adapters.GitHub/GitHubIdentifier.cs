/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Flurl;

namespace TeamCloud.Adapters.GitHub
{
    public sealed class GitHubIdentifier
    {
        private const string DEFAULT_HOSTNAME = "github.com";

        private static readonly Regex RepositoryExpression = new Regex(@"^\/organization\/(.+)\/repository\/(.+)\/$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string SanitizeResourceId(string resourceId, out bool addedTrailingSlash)
        {
            addedTrailingSlash = false;
            resourceId = resourceId?.Trim();

            if (!string.IsNullOrEmpty(resourceId) && !resourceId.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                addedTrailingSlash = true;
                resourceId += "/";
            }

            return resourceId;
        }

        private static string SanitizeRepositoryUrl(string url)
        {
            const string GIT_REPO_SUFFIX = ".git";

            if (string.IsNullOrWhiteSpace(url))
                return url;

            var sanitized = url.Trim();

            if (sanitized.EndsWith(GIT_REPO_SUFFIX, StringComparison.OrdinalIgnoreCase))
                sanitized = sanitized.Substring(0, sanitized.Length - GIT_REPO_SUFFIX.Length);

            return sanitized;
        }

        private static bool IsApiHostname(string hostname)
            => hostname.StartsWith("api.", StringComparison.OrdinalIgnoreCase);

        public static GitHubIdentifier FromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));

            url = SanitizeRepositoryUrl(url);

            if (!Uri.TryCreate(url, UriKind.Absolute, out var urlParsed))
                throw new ArgumentException($"{nameof(url)} must be an absolute URL.", nameof(url));

            var segments = urlParsed.Segments
                .Select(s => s.Trim('/'))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => HttpUtility.UrlDecode(s))
                .Skip(IsApiHostname(urlParsed.Host) ? 1 : 0);

            return new GitHubIdentifier()
            {
                Organization = segments.ElementAtOrDefault(0),
                Repository = segments.ElementAtOrDefault(1)
            };
        }

        public static GitHubIdentifier Parse(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentException($"'{nameof(resourceId)}' cannot be null or whitespace.", nameof(resourceId));

            if (TryParse(resourceId, out var gitHubIdentifier))
                return gitHubIdentifier;

            throw new ArgumentException($"The given string is not a valid GitHub resoure id.", nameof(resourceId));
        }

        public static bool TryParse(string resourceId, out GitHubIdentifier gitHubIdentifier)
        {
            gitHubIdentifier = null;

            if (string.IsNullOrWhiteSpace(resourceId))
                return false;

            foreach (var expression in new Regex[] { RepositoryExpression })
            {
                var match = expression.Match(SanitizeResourceId(resourceId, out var _));

                if (match.Success)
                {
                    gitHubIdentifier = expression switch
                    {
                        Regex exp when (exp == RepositoryExpression) => new GitHubIdentifier()
                        {
                            Organization = match.Groups[1].Value,
                            Repository = match.Groups[2].Value
                        },

                        _ => default
                    };
                }

                if (gitHubIdentifier != null) break;
            }

            return (gitHubIdentifier != null);
        }

        public string Organization { get; set; }

        public string Repository { get; set; }

        public override string ToString()
        {
            var resourceId = "/"
                .AppendPathSegments("organization", Organization)
                .AppendPathSegments("repository", Repository);

            return resourceId.ToString();
        }

    }
}
