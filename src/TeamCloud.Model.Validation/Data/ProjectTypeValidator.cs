/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ProjectTypeValidator : AbstractValidator<ProjectType>
    {
        public ProjectTypeValidator()
        {
            RuleFor(obj => obj.Id)
                .MustBeResourcId();

            RuleFor(obj => obj.Region)
                .MustBeAzureRegion();

            RuleFor(obj => obj.Subscriptions)
                .ForEach(sub => sub.MustBeGuid());

            RuleFor(obj => obj.Providers)
                .MustContainAtLeast(1, pr => !pr.DependsOn.Any(), "Provider without dependency")
                .ForEach(provider => provider.SetValidator(new ProviderReferenceValidator()));

            RuleFor(obj => obj.Providers)
                .Must(NoUnresolvableDependencies)
                .WithMessage("Must not contain unresolvable provider dependencies.");

            RuleFor(obj => obj.Providers)
                .Must(NoCircularDependencies)
                .WithMessage("Must not contain circular provider dependecies.");
        }

        private bool NoUnresolvableDependencies(IList<ProviderReference> providerReferences)
        {
            var providerReferenceDictionary = providerReferences
                .ToDictionary(pr => pr.Id);

            var providerReferenceDependencies = providerReferences
                .SelectMany(pr => pr.DependsOn);

            return providerReferenceDependencies
                .All(prd => providerReferenceDictionary.ContainsKey(prd));
        }

        private bool NoCircularDependencies(IList<ProviderReference> providerReferences)
        {
            var providerReferenceDictionary = providerReferences
                .ToDictionary(pr => pr.Id);

            var providerReferenceResolved = providerReferences
                .Where(pr => !pr.DependsOn.Any())
                .Select(pr => pr.Id)
                .ToList();

            var providerReferenceQueue = new Queue<ProviderReference>(providerReferences.Where(pr => pr.DependsOn.Any()));
            var providerReferencePickup = default(ProviderReference);

            while (providerReferenceQueue.TryDequeue(out var providerReference))
            {
                if (providerReference.DependsOn.All(id => providerReferenceResolved.Contains(id)))
                {
                    providerReferenceResolved.Add(providerReference.Id);
                    providerReferencePickup = default;
                }
                else if (providerReference == providerReferencePickup)
                {
                    // circular dependency detected
                    return false;
                }
                else
                {
                    providerReferenceQueue.Enqueue(providerReference);
                    providerReferencePickup ??= providerReference;
                }
            }

            return true;
        }
    }
}
