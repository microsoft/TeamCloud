using System;
using FluentValidation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Validation
{
    public abstract class Validator<T> : AbstractValidator<T>
    {
        protected Validator(IValidatorProvider validatorProvider)
        {
            ValidatorProvider = validatorProvider ?? throw new ArgumentNullException(nameof(validatorProvider));
        }

        protected IValidatorProvider ValidatorProvider { get; }
    }
}
