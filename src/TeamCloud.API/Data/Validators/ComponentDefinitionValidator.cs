/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators;

public class ComponentDefinitionValidator : Validator<ComponentDefinition>
{
    public ComponentDefinitionValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        RuleFor(obj => obj.TemplateId)
            .MustBeGuid();

        RuleFor(obj => obj.DeploymentScopeId)
            .MustBeGuid()
            .When(obj => !string.IsNullOrWhiteSpace(obj.DeploymentScopeId));

        RuleFor(obj => obj.InputJson)
            .MustBeJson()
            .When(obj => !string.IsNullOrWhiteSpace(obj.InputJson));
    }
}
