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

        public AzureResourceGroup ResourceGroup { get; set; }

        public string ApplicationInsightsKey { get; set; }

        public List<User> Users { get; set; } = new List<User>();

        public List<Guid> ProjectIds { get; set; } = new List<Guid>();

        public TeamCloudConfiguration Configuration { get; set; }
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
            RuleFor(obj => obj.Configuration).NotEmpty();

            RuleForEach(obj => obj.ProjectIds).NotEmpty();
        }
    }
}