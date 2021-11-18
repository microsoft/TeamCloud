/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using FluentValidation;
using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.API.Data.Validators
{
    public class ComponentTaskDefinitionValidator : Validator<ComponentTaskDefinition>
    {
        public ComponentTaskDefinitionValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.TaskId)
                .NotNull()
                .NotEqual(ComponentTaskType.Create.ToString(), StringComparer.OrdinalIgnoreCase)
                .NotEqual(ComponentTaskType.Delete.ToString(), StringComparer.OrdinalIgnoreCase);

            RuleFor(obj => obj.InputJson)
                .MustBeJson()
                .When(obj => !string.IsNullOrWhiteSpace(obj.InputJson));
        }
    }
}
