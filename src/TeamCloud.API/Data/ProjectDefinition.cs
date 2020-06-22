/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
                .ForEach(user => user.SetValidator(new UserDefinitionProjectValidator()));
        }
    }
}
