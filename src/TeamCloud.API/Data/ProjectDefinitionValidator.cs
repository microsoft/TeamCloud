/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
using FluentValidation;
using TeamCloud.Model;

namespace TeamCloud.API.Data
{
    public class ProjectDefinitionValidator : AbstractValidator<ProjectDefinition>
    {
        public ProjectDefinitionValidator()
        {
            // project name is required
            RuleFor(obj => obj.Name).NotEmpty()
                .WithMessage("name is required");

            // project cannot be empty
            RuleFor(obj => obj.Users).NotEmpty()
                .WithMessage("users are required");

            // there must at least one user with role admin
            RuleFor(obj => obj.Users).Must(users => users.Any(user => user.Role == UserRoles.Project.Owner))
                .WithMessage($"There must be at least one user with the role '{UserRoles.Project.Owner}'.");
        }
    }
}
