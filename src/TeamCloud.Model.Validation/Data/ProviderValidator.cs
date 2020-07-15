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
            RuleFor(obj => obj.Id).MustBeProviderId();
            RuleFor(obj => obj.Url).MustBeUrl();

            RuleFor(obj => obj.Version)
                .MustBeVersionString()
                .When(obj => !string.IsNullOrEmpty(obj.Version));

            RuleFor(obj => obj.ResourceGroup)
                .SetValidator(new AzureResourceGroupValidator())
                .When(obj => !(obj.ResourceGroup is null));

        }
    }
}
