/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class OrganizationDefinitionValidator : AbstractValidator<OrganizationDefinition>
    {
        public OrganizationDefinitionValidator()
        {
            RuleFor(obj => obj.DisplayName).NotEmpty();
        }
    }
}
