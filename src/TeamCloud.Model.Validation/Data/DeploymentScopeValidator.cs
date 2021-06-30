/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Linq;
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
        }
    }
}
