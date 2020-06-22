/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ProviderOutputValidator : AbstractValidator<ProviderOutput>
    {
        public ProviderOutputValidator()
        {
            RuleFor(obj => obj.Properties).NotEmpty();
        }
    }
}
