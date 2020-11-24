
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
                .When(obj => obj.SubscriptionIds is null || obj.SubscriptionIds.Count == 0);

            RuleFor(obj => obj.SubscriptionIds)
                .MustContainAtLeast(1)
                .When(obj => string.IsNullOrEmpty(obj.ManagementGroupId));
        }
    }
}
