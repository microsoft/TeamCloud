/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Validation.Data
{
    public sealed class TeamCloudInstanceValidator : AbstractValidator<TeamCloudInstance>
    {
        public TeamCloudInstanceValidator()
        {
            RuleFor(obj => obj.Id)
                .NotNull();
        }
    }
}
