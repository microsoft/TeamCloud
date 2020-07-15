/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class TeamCloudInstanceValidaor : AbstractValidator<TeamCloudInstance>
    {
        public TeamCloudInstanceValidaor()
        {
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
