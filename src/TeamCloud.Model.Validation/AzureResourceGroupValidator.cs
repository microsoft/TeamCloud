/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation
{
    public sealed class AzureResourceGroupValidator : AbstractValidator<AzureResourceGroup>
    {
        public AzureResourceGroupValidator()
        {
            RuleFor(obj => obj.SubscriptionId).MustBeGuid();
            RuleFor(obj => obj.ResourceGroupName).NotEmpty();
            RuleFor(obj => obj.Region).MustBeAzureRegion();
        }
    }
}
