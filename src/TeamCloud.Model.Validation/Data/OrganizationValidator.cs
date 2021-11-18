/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class OrganizationValidator : Validator<Organization>
    {
        public OrganizationValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.DisplayName)
                .NotEmpty();

            RuleFor(obj => obj.SubscriptionId)
                .MustBeGuid();

            RuleFor(obj => obj.Location)
                .NotEmpty();
        }
    }
}
