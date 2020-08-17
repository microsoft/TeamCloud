/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class UserDefinition
    {
        public string Identifier { get; set; }

        public string Role { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    public sealed class UserDefinitionProjectValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionProjectValidator()
        {
            RuleFor(obj => obj.Identifier).MustBeUserIdentifier();
            RuleFor(obj => obj.Role).MustBeProjectUserRole();
        }
    }

    public sealed class UserDefinitionTeamCloudValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionTeamCloudValidator()
        {
            RuleFor(obj => obj.Identifier).MustBeUserIdentifier();
            RuleFor(obj => obj.Role).MustBeTeamCloudUserRole();
        }
    }

    public sealed class UserDefinitionTeamCloudAdminValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionTeamCloudAdminValidator()
        {
            RuleFor(obj => obj.Identifier).MustBeUserIdentifier();
            RuleFor(obj => obj.Role)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .Must(BeAdminUserRole)
                .WithMessage("'{PropertyName}' must be Admin.");
        }

        private static bool BeAdminUserRole(string role)
            => !string.IsNullOrEmpty(role)
            && Enum.TryParse<TeamCloudUserRole>(role, true, out var tcRole)
            && tcRole == TeamCloudUserRole.Admin;
    }
}
