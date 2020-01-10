/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;

namespace TeamCloud.API.Data
{
    public sealed class ProjectDefinitionValidator : AbstractValidator<ProjectDefinition>
    {
        public ProjectDefinitionValidator()
        {
            RuleFor(obj => obj.Name).NotEmpty();
            RuleFor(obj => obj.Users).NotNull();
        }
    }
}