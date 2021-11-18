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
    public sealed class UserValidator : Validator<User>
    {
        public UserValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.Id).NotNull();
            //RuleFor(obj => obj.Role).MustBeUserRole();
        }
    }
}
