/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class UserDefinitionProjectValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionProjectValidator()
        {
            RuleFor(obj => obj.Identifier).MustBeUserIdentifier();
            RuleFor(obj => obj.Role).MustBeProjectUserRole();
        }
    }
}
