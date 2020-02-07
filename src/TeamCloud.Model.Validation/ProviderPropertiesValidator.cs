/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation
{
    public sealed class ProviderPropertiesValidator : AbstractValidator<ProviderProperties>
    {
        public ProviderPropertiesValidator()
        {
            // RuleFor(obj => obj.ProviderId).NotEmpty();
            RuleFor(obj => obj.Properties).NotEmpty();
        }
    }
}
