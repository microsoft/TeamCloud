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
            RuleFor(obj => obj.DisplayName)
                .NotEmpty();

            RuleFor(obj => obj.InputData)
                .NotEmpty();
        }
    }
}
