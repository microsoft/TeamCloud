/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ProviderValidator : AbstractValidator<Provider>
    {
        public ProviderValidator()
        {
            RuleFor(obj => obj.Id).MustBeGuid();
            RuleFor(obj => obj.Url).MustBeUrl();
            RuleFor(obj => obj.AuthCode).NotEmpty();
        }
    }
}
