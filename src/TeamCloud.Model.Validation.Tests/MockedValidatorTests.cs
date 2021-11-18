/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.Model.Common;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;
using Xunit;

namespace TeamCloud.Model.Validation.Tests
{
    public class MockedValidatorTests
    {
        private readonly IValidatorProvider validatorProvider;

        public MockedValidatorTests()
        {
            var serviceCollection = new ServiceCollection().AddTeamCloudValidationProvider(config =>
            {
                config.Register(Assembly.GetExecutingAssembly());
            });

            validatorProvider = serviceCollection.BuildServiceProvider().GetService<IValidatorProvider>();
        }

        public static IEnumerable<object[]> GetValidObjects()
        {
            yield return new object[] { new ValidatableObject(true) };
            yield return new object[] { new ValidatableObjectWithInheritance(true) };
        }

        public static IEnumerable<object[]> GetInvalidObjects()
        {
            yield return new object[] { new ValidatableObject(false) };
            yield return new object[] { new ValidatableObjectWithInheritance(false) };
        }

        [Theory]
        [MemberData(nameof(GetValidObjects))]
        public void Validate_Success(IValidatable validatable)
        {
            Assert.True(validatable.Validate(validatorProvider).IsValid);
        }

        [Theory]
        [MemberData(nameof(GetInvalidObjects))]
        public void Validate_Fail(IValidatable validatable)
        {
            Assert.False(validatable.Validate(validatorProvider).IsValid);
        }

        [Theory]
        [MemberData(nameof(GetInvalidObjects))]
        public void Validate_Exception(IValidatable validatable)
        {
            Assert.Throws<ValidationException>(() => validatable.Validate(validatorProvider, throwOnValidationError: true));
        }

        [Fact]
        public void Validate_NotSupported()
        {
            var validatable = new ValidatableObjectWithoutValidator();

            Assert.Throws<NotSupportedException>(() => validatable.Validate(validatorProvider, throwOnNoValidatorFound: true));
        }

        [Theory]
        [MemberData(nameof(GetValidObjects))]
        public async Task ValidateAsync_Success(IValidatable validatable)
        {
            Assert.True((await validatable.ValidateAsync(validatorProvider).ConfigureAwait(false)).IsValid);
        }

        [Theory]
        [MemberData(nameof(GetInvalidObjects))]
        public async Task ValidateAsync_Fail(IValidatable validatable)
        {
            Assert.False((await validatable.ValidateAsync(validatorProvider).ConfigureAwait(false)).IsValid);
        }

        [Theory]
        [MemberData(nameof(GetInvalidObjects))]
        public async Task ValidateAsync_Exception(IValidatable validatable)
        {
            await Assert.ThrowsAsync<ValidationException>(() => validatable.ValidateAsync(validatorProvider, throwOnValidationError: true)).ConfigureAwait(false);
        }

        [Fact]
        public async Task ValidateAsync_NotSupported()
        {
            var validatable = new ValidatableObjectWithoutValidator();

            await Assert.ThrowsAsync<NotSupportedException>(() => validatable.ValidateAsync(validatorProvider, throwOnNoValidatorFound: true)).ConfigureAwait(false);
        }

        public class ValidatableObject : IValidatable
        {
            public ValidatableObject(bool isValid)
            {
                IsValid = isValid;
            }

            public bool IsValid { get; set; }
        }

        public class ValidatableObjectValidator : Validator<ValidatableObject>
        {
            public ValidatableObjectValidator(IValidatorProvider validatorProvider) : base(validatorProvider)
            {
                RuleFor(obj => obj.IsValid)
                    .Must(isValid => isValid);
            }
        }

        public class ValidatableObjectWithInheritance : ValidatableObject
        {
            public ValidatableObjectWithInheritance(bool isValid)
                : base(isValid)
            { }
        }

        public class ValidatableObjectWithoutValidator : IValidatable
        { }

    }
}
