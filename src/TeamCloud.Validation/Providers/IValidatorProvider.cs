/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using FluentValidation;

namespace TeamCloud.Validation.Providers
{
    public interface IValidatorProvider
    {
        IEnumerable<IValidator> GetValidators<T>();

        IEnumerable<IValidator> GetValidators(Type typeToValidate);

        IValidator<T> ToValidator<T>() => new CompositeValidator<T>(this);
    }
}
