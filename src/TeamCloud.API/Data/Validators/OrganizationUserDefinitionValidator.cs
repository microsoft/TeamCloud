/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Validation;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators
{
    public sealed class OrganizationUserDefinitionValidator : Validator<UserDefinition>
    {
        public OrganizationUserDefinitionValidator(IValidatorProvider validatorProvider): base(validatorProvider)
        {
            RuleFor(obj => obj.Identifier).MustBeUserIdentifier();
            RuleFor(obj => obj.Role).MustBeOrganizationUserRole();
        }
    }
}
