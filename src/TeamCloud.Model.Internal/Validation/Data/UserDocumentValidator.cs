/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class UserDocumentValidator : AbstractValidator<UserDocument>
    {
        public UserDocumentValidator()
        {
            RuleFor(obj => obj.Id).NotNull();
            //RuleFor(obj => obj.Role).MustBeUserRole();
        }
    }
}
