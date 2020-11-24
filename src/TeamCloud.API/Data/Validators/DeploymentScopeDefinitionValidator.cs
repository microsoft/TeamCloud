/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class DeploymentScopeDefinitionValidator : AbstractValidator<DeploymentScopeDefinition>
    {
        public DeploymentScopeDefinitionValidator()
        {
            RuleFor(obj => obj.DisplayName).NotEmpty();

            RuleFor(obj => obj.ManagementGroupId)
                .NotNull()
                .When(obj => obj.SubscriptionIds is null || obj.SubscriptionIds.Count == 0)
                .WithMessage("'{PropertyName}' must be a valid, non-empty GUID if no Subscription IDs are provided.");

            RuleFor(obj => obj.SubscriptionIds)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .When(obj => string.IsNullOrEmpty(obj.ManagementGroupId))
                .WithMessage("'{PropertyName}' must contain at least 1 item/s if no Management Group ID is provided.")
                .Must(list => list.Count >= 1)
                .When(obj => string.IsNullOrEmpty(obj.ManagementGroupId))
                .WithMessage("'{PropertyName}' must contain at least 1 item/s if no Management Group ID is provided.");
        }
    }
}
