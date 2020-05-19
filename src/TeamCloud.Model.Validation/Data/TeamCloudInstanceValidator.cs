/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class TeamCloudInstanceValidator : AbstractValidator<TeamCloudInstance>
    {
        public TeamCloudInstanceValidator()
        {
            RuleFor(obj => obj.Id)
                .MustBeResourceId();

            RuleFor(obj => obj.PartitionKey)
                .NotEmpty();

            RuleFor(obj => obj.Providers)
                .NotEmpty();
        }
    }
}
