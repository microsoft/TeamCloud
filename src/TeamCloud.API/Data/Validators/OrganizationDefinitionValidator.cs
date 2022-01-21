/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators;

public sealed class OrganizationDefinitionValidator : Validator<OrganizationDefinition>
{
    public OrganizationDefinitionValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        RuleFor(obj => obj.DisplayName).NotEmpty();
    }
}
