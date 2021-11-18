using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Validators;

namespace TeamCloud.Validation.Providers
{
    public sealed  class CompositeValidatorDescriptor<T> : IValidatorDescriptor
    {
        private readonly IEnumerable<IValidatorDescriptor> validatorDescriptors;

        internal CompositeValidatorDescriptor(IValidatorProvider validatorProvider)
        {
            validatorDescriptors = validatorProvider
                .GetValidators<T>()
                .Select(validator => validator.CreateDescriptor());
        }

        public ILookup<string, IPropertyValidator> GetMembersWithValidators()
        {
            var compositeLookup = EmptyLookup<string, IPropertyValidator>.Instance;
            
            foreach (var lookup in validatorDescriptors.Select(descriptors => descriptors.GetMembersWithValidators()))
            {
                compositeLookup = compositeLookup.Concat(lookup)
                    .SelectMany(grp => grp.Select(val => new KeyValuePair<string, IPropertyValidator>(grp.Key, val)))
                    .ToLookup(kvp => kvp.Key, kvp => kvp.Value);
            }

            return compositeLookup;
        }


        public string GetName(string property)
            => validatorDescriptors
            .Select(desc => desc.GetName(property))
            .FirstOrDefault(name => !string.IsNullOrEmpty(name));

        public IEnumerable<IValidationRule> GetRulesForMember(string name)
            => validatorDescriptors.SelectMany(desc => desc.GetRulesForMember(name));

        public IEnumerable<IPropertyValidator> GetValidatorsForMember(string name)
            => validatorDescriptors.SelectMany(desc => desc.GetValidatorsForMember(name));
    }
}
