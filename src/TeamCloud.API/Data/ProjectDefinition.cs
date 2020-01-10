/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

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
            RuleFor(obj => obj.Name).NotEmpty();
            RuleFor(obj => obj.Users).NotNull();
        }
    }
}