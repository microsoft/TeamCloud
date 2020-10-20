/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Commands.Core;

namespace TeamCloud.Model.Validation.Commands
{
    public class CommandMessageValidator : AbstractValidator<ICommandMessage>
    {
        public CommandMessageValidator()
        {
            RuleFor(obj => obj.Command)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .SetValidator(new CommandValidator());
        }
    }
}
