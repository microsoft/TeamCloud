/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class TeamCloudInstanceValidator : AbstractValidator<TeamCloudInstance>
    {
        public TeamCloudInstanceValidator()
        {
            RuleFor(obj => obj.Id)
                .MustBeResourceId();

            RuleFor(obj => obj.PartitionKey)
                .NotEmpty();

            RuleFor(obj => obj.Users)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .Must(users => users.Any(user => user.Role == UserRoles.TeamCloud.Admin))
                    .WithMessage("'{PropertyName}' must contain at least one user with the role " + $"'{UserRoles.TeamCloud.Admin}'.");

            RuleFor(obj => obj.Providers)
                .NotEmpty();
        }
    }
}
