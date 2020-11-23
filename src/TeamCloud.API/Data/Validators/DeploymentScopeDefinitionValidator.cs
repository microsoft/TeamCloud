/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using FluentValidation;

namespace TeamCloud.API.Data.Validators
{
    public sealed class DeploymentScopeDefinitionValidator : AbstractValidator<DeploymentScopeDefinition>
    {
        public DeploymentScopeDefinitionValidator()
        {
            RuleFor(obj => obj.DisplayName).NotEmpty();

            RuleFor(obj => obj.ManagementGroupId)
                .Must(BeGuid)
                .When(obj => obj.SubscriptionIds is null || obj.SubscriptionIds.Count == 0)
                .WithMessage("'{PropertyName}' must be a valid, non-empty GUID if no Subscription IDs are provided.");

            RuleFor(obj => obj.SubscriptionIds)
                .Must(list => list.Count >= 1)
                .When(obj => string.IsNullOrEmpty(obj.ManagementGroupId))
                .WithMessage("'{PropertyName}' must contain at least 1 item/s if no Management Group ID is provided.");
        }
        private static bool BeGuid(string guid)
            => !string.IsNullOrEmpty(guid)
            && Guid.TryParse(guid, out var outGuid)
            && !outGuid.Equals(Guid.Empty);
    }
}
