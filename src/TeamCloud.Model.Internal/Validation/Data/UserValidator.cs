/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Validation.Data
{
    public sealed class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(obj => obj.Id).NotNull();
            //RuleFor(obj => obj.Role).MustBeUserRole();
        }
    }
}
