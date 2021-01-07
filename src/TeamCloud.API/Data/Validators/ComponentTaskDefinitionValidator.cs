/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using FluentValidation;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Data.Validators
{
    public class ComponentTaskDefinitionValidator : AbstractValidator<ComponentTaskDefinition>
    {
        public ComponentTaskDefinitionValidator()
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
