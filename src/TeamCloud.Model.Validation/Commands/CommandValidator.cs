/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Model.Validation.Commands
{
    public class CommandValidator : Validator<ICommand>
    {
        public CommandValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
        {
            RuleFor(obj => obj.User)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .SetValidator(ValidatorProvider);

            RuleFor(obj => obj.Payload)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .SetValidator(ValidatorProvider);
        }
    }
}
