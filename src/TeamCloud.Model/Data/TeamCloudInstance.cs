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
    public sealed class TeamCloudInstance : IContainerDocument
    {
        public string Id = Constants.CosmosDb.TeamCloudInstanceId;

        public string PartitionKey => Id;

        [JsonIgnore]
        public List<string> UniqueKeys => new List<string> { };

        public AzureResourceGroup ResourceGroup { get; set; }

        public string ApplicationInsightsKey { get; set; }

        public List<User> Users { get; set; } = new List<User>();

        public List<Guid> ProjectIds { get; set; } = new List<Guid>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public List<Provider> Providers { get; set; } = new List<Provider>();

        public TeamCloudInstance()
        { }

        public TeamCloudInstance(TeamCloudConfiguration config)
        {
            Users = config.Users;
            Tags = config.Tags;
            Properties = config.Properties;
            Providers = config.Providers;
        }
    }

    public sealed class TeamCloudInstanceValidator : AbstractValidator<TeamCloudInstance>
    {
        public TeamCloudInstanceValidator()
        {
            RuleFor(obj => obj.Id).NotEmpty();
            RuleFor(obj => obj.PartitionKey).NotEmpty();
            RuleFor(obj => obj.ApplicationInsightsKey).NotEmpty();
            RuleFor(obj => obj.Users).NotEmpty();

            // there must at least one user with role admin
            RuleFor(obj => obj.Users).Must(users => users.Any(u => u.Role == UserRoles.TeamCloud.Admin))
                .WithMessage($"There must be at least one user with the role '{UserRoles.TeamCloud.Admin}'.");

            RuleFor(obj => obj.ProjectIds).NotEmpty();

            RuleFor(obj => obj.Providers).NotEmpty();

            RuleForEach(obj => obj.ProjectIds).NotEmpty();
        }
    }
}
