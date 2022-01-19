/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace TeamCloud.Validation.Providers;

public sealed class CompositeValidator<T> : IValidator<T>
{
    private readonly IValidatorProvider validatorProvider;

    internal CompositeValidator(IValidatorProvider validatorProvider) : base()
    {
        this.validatorProvider = validatorProvider ?? throw new ArgumentNullException(nameof(validatorProvider));
    }

    public CascadeMode CascadeMode { get; set; }

    public bool CanValidateInstancesOfType(Type type)
        => typeof(T).IsAssignableFrom(type);

    public IValidatorDescriptor CreateDescriptor()
        => new CompositeValidatorDescriptor<T>(validatorProvider);

    public ValidationResult Validate(T instance)
        => Validate(new ValidationContext<T>(instance));

    public ValidationResult Validate(IValidationContext context)
    {
        var validationResults = validatorProvider
            .GetValidators<T>()
            .Select(validator => validator.Validate(context));

        return validationResults.Merge();
    }

    public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default)
        => ValidateAsync(new ValidationContext<T>(instance), cancellation);

    public async Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
    {
        var validationResults = await validatorProvider
            .GetValidators<T>()
            .Select(validator => validator.ValidateAsync(context))
            .WhenAll()
            .ConfigureAwait(false);

        return validationResults.Merge();
    }
}
