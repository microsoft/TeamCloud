/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Validation.Data
{
    public sealed class UserValidator : AbstractValidator<UserDocument>
    {
        public UserValidator()
        {
            RuleFor(obj => obj.Id).NotNull();
            //RuleFor(obj => obj.Role).MustBeUserRole();
        }
    }
}
