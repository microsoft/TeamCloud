/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Commands;

public class CommandMessageValidator : Validator<ICommandMessage>
{
    public CommandMessageValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
    {
        RuleFor(obj => obj.Command)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .SetValidator(ValidatorProvider);
    }
}
