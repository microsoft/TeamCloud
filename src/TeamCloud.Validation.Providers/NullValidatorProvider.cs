using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentValidation;
using TeamCloud.Validation;

namespace TeamCloud.Validation.Providers
{
    public sealed class NullValidatorProvider : IValidatorProvider
    {
        public static IValidatorProvider Instance { get; } = new NullValidatorProvider();

        private NullValidatorProvider() { }

        public IEnumerable<IValidator> GetValidators<T>()
            => Enumerable.Empty<IValidator>();

        public IEnumerable<IValidator> GetValidators(Type typeToValidate)
            => Enumerable.Empty<IValidator>();
    }
}
