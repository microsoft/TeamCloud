/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

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
        public List<ProjectType> ProjectTypes { get; set; } = new List<ProjectType>();

        public List<Provider> Providers { get; set; } = new List<Provider>();

        public List<User> Users { get; set; } = new List<User>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    public sealed class TeamCloudConfigurationValidator : AbstractValidator<TeamCloudConfiguration>
    {
        public TeamCloudConfigurationValidator()
        {
            //RuleFor(obj => obj.Version).NotEmpty();
            RuleFor(obj => obj.ProjectTypes).NotEmpty();
            RuleFor(obj => obj.Providers).NotEmpty();
            RuleFor(obj => obj.Users).NotEmpty();

            // there must at least one user with role admin
            RuleFor(obj => obj.Users).Must(users => users.Any(u => u.Role == UserRoles.TeamCloud.Admin))
                .WithMessage($"There must be at least one user with the role '{UserRoles.TeamCloud.Admin}'.");

            // must have a single projectType set as default
            // RuleFor(obj => obj.ProjectTypes).Must(types => types.Any(t => t.Default))
            //     .WithMessage("There must be at least one ProjectType with default set to true.");

            // each projectType provider id must match a valid teamcloud provider id
            RuleFor(obj => obj.ProjectTypes).Must((config, types) => types
                .All(type => type.Providers
                    .All(typeProvider => config.Providers
                        .Any(provider => provider.Id == typeProvider.Id))))
            .WithMessage("All provider ids on ProjectTypes must match a declared provider's id.");

            // each provider dependency must match a valid provider id
            RuleFor(obj => obj.Providers).Must((config, providers) => providers
                .All(provider => provider.Dependencies.Create
                    .All(dependency => config.Providers
                        .Any(provider => provider.Id == dependency))
                && provider.Dependencies.Init
                    .All(dependency => config.Providers
                        .Any(provider => provider.Id == dependency))))
            .WithMessage("All provider dependencies must match a valid provider id.");

            // each provider event must match a valid provider id
            RuleFor(obj => obj.Providers).Must((config, providers) => providers
                .All(provider => provider.Events
                    .All(evnt => config.Providers
                        .Any(provider => provider.Id == evnt))))
            .WithMessage("All provider events must match a valid provider id.");
        }
    }
}
