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
    public sealed class UserDefinition
    {
        public string Email { get; set; }

        public string Role { get; set; }

        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public sealed class UserDefinitionValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionValidator()
        {
            RuleFor(obj => obj.Email).NotEmpty()
                .WithMessage("Email is required");

            RuleFor(obj => obj.Email).EmailAddress()
                .WithMessage("Email must contain a valid email address");

            RuleFor(obj => obj.Role).Must(role => ValidProjectRoles.Contains(role))
                .WithMessage($"Invalid role detected - valid rules: {string.Join(", ", ValidProjectRoles)}");
        }

        private static readonly string[] ValidProjectRoles = new string[]
        {
            UserRoles.Project.Owner,
            UserRoles.Project.Member
        };
    }
}