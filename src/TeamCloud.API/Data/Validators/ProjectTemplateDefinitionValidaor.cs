/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators;

public sealed class ProjectTemplateDefinitionValidator : Validator<ProjectTemplateDefinition>
{
    public ProjectTemplateDefinitionValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        RuleFor(obj => obj.Repository)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(ValidatorProvider);
    }
}
