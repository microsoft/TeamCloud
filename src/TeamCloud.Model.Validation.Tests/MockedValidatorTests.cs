using System;
using System.Threading.Tasks;
using FluentValidation;
using Xunit;

namespace TeamCloud.Model.Validation.Tests
{
    public class MockedValidatorTests
    {
        [Fact]
        public void Validate_Exception()
        {
            var validatable = new ValidatableObject(false);

            Assert.Throws<ValidationException>(() => validatable.Validate<ValidatableObjectValidator>(throwOnValidationError: true));
        }

        [Fact]
        public async Task ValidateAsync_Exception()
        {
            var validatable = new ValidatableObject(false);

            await Assert.ThrowsAsync<ValidationException>(() => validatable.ValidateAsync<ValidatableObjectValidator>(throwOnValidationError: true)).ConfigureAwait(false);
        }

        [Fact]
        public void Validate_NotSupported()
        {
            var validatable = new ValidatableObject(false);

            Assert.Throws<NotSupportedException>(() => validatable.Validate<ValidatableObjectValidator2>(throwOnNotValidable: true));
        }

        [Fact]
        public async Task ValidateAsync_NotSupported()
        {
            var validatable = new ValidatableObject(false);

            await Assert.ThrowsAsync<NotSupportedException>(() => validatable.ValidateAsync<ValidatableObjectValidator2>(throwOnNotValidable: true)).ConfigureAwait(false);
        }

        public class ValidatableObject : IValidatable
        {
            public ValidatableObject(bool isValid)
            {
                IsValid = isValid;
            }

            public bool IsValid { get; set; }
        }

        public class ValidatableObjectValidator : AbstractValidator<ValidatableObject>
        {
            public ValidatableObjectValidator()
            {
                RuleFor(obj => obj.IsValid)
                    .Must(isValid => isValid);
            }
        }

        public class ValidatableObject2 : IValidatable
        {
            public ValidatableObject2(bool isValid)
            {
                IsValid = isValid;
            }

            public bool IsValid { get; set; }
        }

        public class ValidatableObjectValidator2 : AbstractValidator<ValidatableObject2>
        {
            public ValidatableObjectValidator2()
            {
                RuleFor(obj => obj.IsValid)
                    .Must(isValid => isValid);
            }
        }
    }
}
