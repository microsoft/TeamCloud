/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;

namespace TeamCloud.API.Data
{
    public sealed class UserDefinitionValidator : AbstractValidator<UserDefinition>
    {
        public UserDefinitionValidator()
        {
            RuleFor(obj => obj.Email).NotEmpty();
            RuleFor(obj => obj.Role).NotEmpty();
            RuleFor(obj => obj.Tags).NotNull();
        }
    }
}