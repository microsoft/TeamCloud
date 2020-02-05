/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class ProjectDefinition
    {
        public string Name { get; set; }

        public string ProjectType { get; set; }

        public List<UserDefinition> Users { get; set; } = new List<UserDefinition>();

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public sealed class ProjectDefinitionValidator : AbstractValidator<ProjectDefinition>
    {
        public ProjectDefinitionValidator()
        {
            RuleFor(obj => obj.Name).NotEmpty();
            RuleFor(obj => obj.Users)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .ForEach(user => user.SetValidator(new UserDefinitionValidator()))
                .Must(users => users.Any(user => user.Role == UserRoles.Project.Owner))
                    .WithMessage("'{PropertyName}' must contain at least one user with the role " + $"'{UserRoles.Project.Owner}'.");
        }
    }
}
