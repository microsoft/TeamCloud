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

        public TeamCloudProjectConfiguration Projects { get; set; }

        public List<Provider> Providers { get; set; }

        public List<User> Users { get; set; } = new List<User>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }

    public sealed class TeamCloudConfigurationValidator : AbstractValidator<TeamCloudConfiguration>
    {
        public TeamCloudConfigurationValidator()
        {
            //RuleFor(obj => obj.Version).NotEmpty();
            RuleFor(obj => obj.Projects).NotEmpty();
            RuleFor(obj => obj.Providers).NotEmpty();
            RuleFor(obj => obj.Users).NotEmpty();

            // there must at least one user with role admin
            RuleFor(obj => obj.Users).Must(users => users.Any(u => u.Role == UserRoles.TeamCloud.Admin))
                .WithMessage($"There must be at least one user with the role '{UserRoles.TeamCloud.Admin}'.");
        }
    }


    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class TeamCloudProjectConfiguration
    {
        public TeamCloudProjectAzureConfiguration Azure { get; set; }
    }


    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class TeamCloudProjectAzureConfiguration
    {
        public string Region { get; set; }

        public string ResourceGroupNamePrefix { get; set; }

        public int DefaultSubscriptionCapacity { get; set; }

        public List<TeamCloudProjectSubscriptionConfiguration> Subscriptions { get; set; } = new List<TeamCloudProjectSubscriptionConfiguration>();
    }

    public sealed class TeamCloudProjectAzureConfigurationValidator : AbstractValidator<TeamCloudProjectAzureConfiguration>
    {
        public TeamCloudProjectAzureConfigurationValidator()
        {
            RuleFor(obj => obj.Region).NotEmpty();
            RuleFor(obj => obj.Subscriptions).NotEmpty();
            RuleFor(obj => obj.Subscriptions).Must(obj => obj.Count >= 3);
        }
    }


    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class TeamCloudProjectSubscriptionConfiguration
    {
        public string Id { get; set; }

        public int? Capacity { get; set; }
    }

    public sealed class TeamCloudProjectSubscriptionConfigurationValidator : AbstractValidator<TeamCloudProjectSubscriptionConfiguration>
    {
        public TeamCloudProjectSubscriptionConfigurationValidator()
        {
            RuleFor(obj => obj.Id).Must(Validation.BeGuid);
        }
    }

}