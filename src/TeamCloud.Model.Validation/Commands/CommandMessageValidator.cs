/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Commands;

namespace TeamCloud.Model.Validation.Commands
{
    public class CommandMessageValidator : AbstractValidator<ICommandMessage>
    {
        public CommandMessageValidator()
        {
            RuleFor(obj => obj.Command)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .SetValidator(new CommandValidator());
        }
    }
}
