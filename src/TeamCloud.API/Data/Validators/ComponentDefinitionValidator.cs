/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data.Validators
{
    public class ComponentDefinitionValidator : AbstractValidator<ComponentDefinition>
    {
        public ComponentDefinitionValidator()
        {
            RuleFor(obj => obj.TemplateId)
                .MustBeGuid();

            RuleFor(obj => obj.DeploymentScopeId)
                .MustBeGuid()
                .When(obj => !string.IsNullOrWhiteSpace(obj.DeploymentScopeId));

            RuleFor(obj => obj.InputJson)
                .MustBeJson()
                .When(obj => !string.IsNullOrWhiteSpace(obj.InputJson));
        }
    }
}
