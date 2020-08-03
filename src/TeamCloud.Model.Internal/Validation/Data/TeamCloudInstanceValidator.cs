/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Validation;
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.Model.Internal.Validation.Data
{
    public sealed class TeamCloudInstanceValidator : AbstractValidator<TeamCloudInstanceDocument>
    {
        public TeamCloudInstanceValidator()
        {
            RuleFor(obj => obj.Id)
                .NotNull();

            RuleFor(obj => obj.Version)
                .MustBeVersionString()
                .When(obj => !string.IsNullOrEmpty(obj.Version));

            RuleFor(obj => obj.ResourceGroup)
                .SetValidator(new AzureResourceGroupValidator())
                .When(obj => !(obj.ResourceGroup is null));

            RuleForEach(obj => obj.Tags).MustBeValidTag();
        }
    }
}
