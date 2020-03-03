/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public class ProviderReferenceValidator : AbstractValidator<ProviderReference>
    {
        public ProviderReferenceValidator()
        {
            RuleFor(obj => obj.Id).NotEmpty();
        }
    }
}
