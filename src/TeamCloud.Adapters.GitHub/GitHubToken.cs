/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;
using User = Octokit.User;

namespace TeamCloud.Adapters.GitHub
{
    public sealed class GitHubToken : AuthorizationToken
    {
        internal static string FormatOrganizationUrl(string organization)
        {
            if (string.IsNullOrWhiteSpace(organization))
                return null;

            if (Uri.TryCreate(organization, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                return organization;

            return $"https://github.com/{organization}";
        }

        public GitHubToken() : this(null)
        { }

        public GitHubToken(DeploymentScope deployementScope) : base(GetEntityId(deployementScope))
        { }

        public string Name { get; set; }

        public string Slug { get; set; }

        public bool Suspended { get; set; }

        public bool Enabled
            => !Suspended && long.TryParse(ApplicationId, out _) && long.TryParse(InstallationId, out _);

        [JsonProperty("id")]
        public string ApplicationId { get; set; }

        public string InstallationId { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        public string AccessToken { get; set; }

        [JsonIgnore]
        public DateTime? AccessTokenExpires { get; set; }

        [JsonIgnore]
        public bool AccessTokenExpired
            => AccessTokenExpires.HasValue ? AccessTokenExpires < DateTime.UtcNow : true;

        [JsonProperty("webhook_secret")]
        public string WebhookSecret { get; set; }

        public string Pem { get; set; }

        public User Owner { get; set; }
    }
}
