/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators;

public sealed class ProjectDefinitionValidator : Validator<ProjectDefinition>
{
    public ProjectDefinitionValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        RuleFor(obj => obj.DisplayName).NotEmpty();
        RuleFor(obj => obj.Template).MustBeGuid(); //TODO: May not need to require this if there is a default template
        RuleFor(obj => obj.TemplateInput).NotEmpty();
        RuleFor(obj => obj.Users)
            .ForEach(user => user.SetValidator(ValidatorProvider));
    }
}
