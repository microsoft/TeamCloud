/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class ProjectLinkDefinitionValidator : AbstractValidator<ProjectLinkDefinition>
    {
        public ProjectLinkDefinitionValidator()
        {
            RuleFor(obj => obj.Id)
                .MustBeGuid();

            RuleFor(obj => obj.HRef)
                .MustBeUrl();

            RuleFor(obj => obj.Title)
                .NotEmpty();
        }
    }
}
