/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ProviderPropertiesValidator : AbstractValidator<ProviderProperties>
    {
        public ProviderPropertiesValidator()
        {
            RuleFor(obj => obj.ProviderId).NotEmpty();
            RuleFor(obj => obj.Properties).NotEmpty();
        }
    }
}
