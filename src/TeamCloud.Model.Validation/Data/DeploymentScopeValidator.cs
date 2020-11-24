
/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class DeploymentScopeValidator : AbstractValidator<DeploymentScope>
    {
        public DeploymentScopeValidator()
        {
            RuleFor(obj => obj.Organization)
                .MustBeGuid();

            RuleFor(obj => obj.DisplayName)
                .NotEmpty();

            RuleFor(obj => obj.ManagementGroupId)
                .NotNull()
                .When(obj => obj.SubscriptionIds is null || obj.SubscriptionIds.Count == 0)
                .WithMessage("'{PropertyName}' must be a valid, non-empty GUID if no Subscription IDs are provided.");

            RuleFor(obj => obj.SubscriptionIds)
                .Must(list => list.Count >= 1)
                .When(obj => string.IsNullOrEmpty(obj.ManagementGroupId))
                .WithMessage("'{PropertyName}' must contain at least 1 item/s if no Management Group ID is provided.");
        }
    }
}
