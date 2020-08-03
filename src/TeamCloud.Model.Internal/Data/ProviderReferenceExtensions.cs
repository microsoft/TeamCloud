/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Internal.Data
{
    public static class ProviderReferenceExtensions
    {
        public static IEnumerable<IEnumerable<ProviderDocument>> Resolve(this IList<ProviderReference> providerReferences, IList<ProviderDocument> providers)
            => providerReferences.AsEnumerable().Resolve(providers);

        public static IEnumerable<IEnumerable<ProviderDocument>> Resolve(this IEnumerable<ProviderReference> providerReferences, IList<ProviderDocument> providers)
        {
            if (providerReferences is null)
                throw new ArgumentNullException(nameof(providerReferences));

            if (providers is null)
                throw new ArgumentNullException(nameof(providers));

            var providerDictionary = providers
                .ToDictionary((provider) => provider.Id);

            var providerBatches = new Dictionary<int, List<ProviderDocument>>()
            {
                { 1, providerReferences
                    .Where(pr => !pr.DependsOn.Any())
                    .Select(pr => providerDictionary.GetValueOrDefault(pr.Id) ?? throw new NullReferenceException($"Could not find provider by id '{pr.Id}'"))
                    .ToList() }
            };

            if (!providerBatches.SelectMany(pb => pb.Value).Any())
            {
                throw new InvalidOperationException("At least one provider reference without a dependency is needed.");
            }

            var providerReferenceQueue = new Queue<ProviderReference>(providerReferences.Where(pr => pr.DependsOn.Any()));
            var providerReferencePickup = default(ProviderReference);

            while (providerReferenceQueue.TryDequeue(out var providerReference))
            {
                if (providerDictionary.TryGetValue(providerReference.Id, out var provider))
                {
                    var providerBatchNumber = GetBatchNumber(providerReference);

                    if (providerBatchNumber == 0)
                    {
                        if (providerReference == providerReferencePickup)
                        {
                            throw new InvalidOperationException($"Circular dependency detected - Unable to process provider '{providerReference.Id}'");
                        }

                        providerReferenceQueue.Enqueue(providerReference);
                        providerReferencePickup ??= providerReference;
                    }
                    else
                    {
                        if (providerBatches.TryGetValue(providerBatchNumber, out var providerBatch))
                        {
                            providerBatch.Add(provider);
                        }
                        else
                        {
                            providerBatches.Add(providerBatchNumber, new List<ProviderDocument>() { provider });
                        }

                        providerReferencePickup = default;
                    }
                }
                else
                {
                    throw new NullReferenceException($"Could not find provider by id '{providerReference.Id}'");
                }
            }

            return providerBatches.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value);

            int GetBatchNumber(ProviderReference providerReference)
            {
                var batchMatches = providerReference.DependsOn
                    .ToDictionary(dependsOn => dependsOn, dependsOn => 0);

                foreach (var kvp in providerBatches)
                {
                    var matches = providerReference.DependsOn
                        .Intersect(kvp.Value.Select(provider => provider.Id));

                    foreach (var match in matches)
                        batchMatches[match] = kvp.Key;
                }

                return batchMatches.Values.Contains(0)
                    ? 0 // at least one dependency was not resolved
                    : batchMatches.Values.Max() + 1;
            }
        }
    }
}
