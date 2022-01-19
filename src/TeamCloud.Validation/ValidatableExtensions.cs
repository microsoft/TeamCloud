/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using FluentValidation;
using System.Threading.Tasks;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Validation;

public static class ValidatableExtensions
{
    public static bool TryValidate(this IValidatable validatable, IValidatorProvider validatorFactory, out ValidationResult validationResult)
    {
        if (validatable is null)
            throw new ArgumentNullException(nameof(validatable));

        if (validatorFactory is null)
            throw new ArgumentNullException(nameof(validatorFactory));

        validationResult = validatable.Validate(validatorFactory);

        return validationResult.IsValid;
    }

    public static ValidationResult Validate(this IValidatable validatable, IValidatorProvider validatorFactory, bool throwOnNoValidatorFound = false, bool throwOnValidationError = false)
    {
        if (validatable is null)
            throw new ArgumentNullException(nameof(validatable));

        if (validatorFactory is null)
            throw new ArgumentNullException(nameof(validatorFactory));

        var validators = validatorFactory.GetValidators(validatable.GetType());

        if (validators.Any())
        {
            var context = new ValidationContext<IValidatable>(validatable);

            var validationResult = validators
                .Select(validator => validator.Validate(context))
                .Merge();

            if (!validationResult.IsValid && throwOnValidationError)
                throw validationResult.ToException();

            return validationResult;
        }

        if (throwOnNoValidatorFound)
            throw new NotSupportedException($"Validation of type {validatable.GetType()} is not supported");

        return new ValidationResult();
    }

    public static async Task<ValidationResult> ValidateAsync(this IValidatable validatable, IValidatorProvider validatorFactory, bool throwOnNoValidatorFound = false, bool throwOnValidationError = false)
    {
        if (validatable is null)
            throw new ArgumentNullException(nameof(validatable));

        if (validatorFactory is null)
            throw new ArgumentNullException(nameof(validatorFactory));

        var validators = validatorFactory.GetValidators(validatable.GetType());

        if (validators.Any())
        {
            var context = new ValidationContext<IValidatable>(validatable);

            var validationTasks = validators
                .Select(validator => validator.ValidateAsync(context));

            var validationResults = await Task
                .WhenAll(validationTasks)
                .ConfigureAwait(false);

            var validationResult = validationResults.Merge();

            if (!validationResult.IsValid && throwOnValidationError)
                throw validationResult.ToException();

            return validationResult;
        }

        if (throwOnNoValidatorFound)
            throw new NotSupportedException($"Validation of type {validatable.GetType()} is not supported");

        return new ValidationResult();
    }

    internal static ValidationResult Merge(this IEnumerable<ValidationResult> validationResults)
        => new ValidationResult(validationResults.SelectMany(validationResult => validationResult.Errors));

    public static ValidationException ToException(this ValidationResult validationResult)
        => (validationResult ?? throw new ArgumentNullException(nameof(validationResult))).IsValid ? null : new ValidationException(validationResult.Errors);
}
