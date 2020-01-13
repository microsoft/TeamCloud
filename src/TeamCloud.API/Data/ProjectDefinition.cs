/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model;

namespace TeamCloud.API
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class ProjectDefinition
    {
        public string Name { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public List<UserDefinition> Users { get; set; } = new List<UserDefinition>();
    }

    public sealed class ProjectDefinitionValidator : AbstractValidator<ProjectDefinition>
    {
        public ProjectDefinitionValidator()
        {
            // project name is required
            RuleFor(obj => obj.Name).NotEmpty()
                .WithMessage("Name is required");

            RuleFor(obj => obj.Tags).NotEmpty().WithMessage("Tags must not be null.");

            // project cannot be empty
            RuleFor(obj => obj.Users).NotEmpty()
                .WithMessage("Users are required");

            // there must at least one user with role admin
            RuleFor(obj => obj.Users).Must(users => users.Any(user => user.Role == UserRoles.Project.Owner))
                .WithMessage($"There must be at least one user with the role '{UserRoles.Project.Owner}'.");
        }
    }
}