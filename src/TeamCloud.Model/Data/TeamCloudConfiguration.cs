/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class TeamCloudConfiguration
    {
        public string Version { get; set; }

        public TeamCloudAzureConfiguration Azure { get; set; }

        public List<TeamCloudProviderConfiguration> Providers { get; set; }

        public List<User> Users { get; set; } = new List<User>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }

    public sealed class TeamCloudConfigurationValidator : AbstractValidator<TeamCloudConfiguration>
    {
        public TeamCloudConfigurationValidator()
        {
            RuleFor(obj => obj.Version).NotEmpty();
            RuleFor(obj => obj.Azure).NotEmpty();
            RuleFor(obj => obj.Providers).NotEmpty();
            RuleFor(obj => obj.Users).NotEmpty();

            // there must at least one user with role admin
            RuleFor(obj => obj.Users).Must(users => users.Any(u => u.Role == UserRoles.TeamCloud.Admin))
                .WithMessage($"There must be at least one user with the role '{UserRoles.TeamCloud.Admin}'.");
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class TeamCloudAzureConfiguration
    {
        public string Region { get; set; }

        public string SubscriptionId { get; set; }

        public AzureIdentity ServicePricipal { get; set; }

        public List<string> SubscriptionPoolIds { get; set; } = new List<string>();

        public int ProjectsPerSubscription { get; set; }

        public string ResourceGroupNamePrefix { get; set; }
    }

    public sealed class TeamCloudAzureConfigurationValidator : AbstractValidator<TeamCloudAzureConfiguration>
    {
        public TeamCloudAzureConfigurationValidator()
        {
            RuleFor(obj => obj.Region).NotEmpty();
            RuleFor(obj => obj.SubscriptionId).Must(Validation.BeGuid);
            RuleFor(obj => obj.ServicePricipal).NotEmpty();
            RuleFor(obj => obj.SubscriptionPoolIds).Must(obj => obj.Count >= 3);

            RuleForEach(obj => obj.SubscriptionPoolIds).Must(Validation.BeGuid);
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class TeamCloudProviderConfiguration: IEquatable<TeamCloudProviderConfiguration>
    {
        public string Id { get; set; }

        public Uri Location { get; set; }

        public string AuthCode { get; set; }

        public bool Optional { get; set; }

        public TeamCloudProviderConfigurationDependencies Dependencies { get; set; }

        public List<string> Events { get; set; } = new List<string>();

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        public bool Equals(TeamCloudProviderConfiguration other) => Id.Equals(other.Id, StringComparison.InvariantCultureIgnoreCase);
    }

    public sealed class TeamCloudProviderConfigurationValidator : AbstractValidator<TeamCloudProviderConfiguration>
    {
        public TeamCloudProviderConfigurationValidator()
        {
            RuleFor(obj => obj.Id).NotEmpty();
            RuleFor(obj => obj.Location).NotEmpty();
            RuleFor(obj => obj.AuthCode).NotEmpty();
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class TeamCloudProviderConfigurationDependencies
    {
        public List<string> Create { get; set; } = new List<string>();

        public List<string> Init { get; set; } = new List<string>();
    }
}