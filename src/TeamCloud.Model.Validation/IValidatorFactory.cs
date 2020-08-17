/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using FluentValidation;

namespace TeamCloud.Model.Validation
{
    public interface IValidatorFactory
    {
        void AddValidator<T>(bool merge = true)
            where T : class, IValidator;

        void AddValidators(Assembly assembly, bool merge = true);

        IEnumerable<Type> GetValidatorTypes<T>();

        IEnumerable<Type> GetValidatorTypes(Type typeToValidate);

        IEnumerable<IValidator> GetValidators<T>(IServiceProvider serviceProvider = null);

        IEnumerable<IValidator> GetValidators(Type typeToValidate, IServiceProvider serviceProvider = null);
    }
}
