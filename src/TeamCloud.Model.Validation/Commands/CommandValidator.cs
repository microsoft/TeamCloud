using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using TeamCloud.Model.Commands;

namespace TeamCloud.Model.Validation.Commands
{
    public class CommandValidator : AbstractValidator<ICommand>
    {
        public CommandValidator()
        {
            RuleFor(obj => obj.User)
                .NotNull();
        }
    }
}
