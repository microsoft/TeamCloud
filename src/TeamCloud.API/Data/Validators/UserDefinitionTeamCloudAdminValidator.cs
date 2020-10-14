/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using FluentValidation;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class UserDefinitionTeamCloudAdminValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionTeamCloudAdminValidator()
        {
            RuleFor(obj => obj.Identifier).MustBeUserIdentifier();
            RuleFor(obj => obj.Role)
                .Cascade(CascadeMode.Stop)
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
