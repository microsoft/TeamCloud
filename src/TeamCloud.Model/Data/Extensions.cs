/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Data
{
    public static class Extensions
    {
        public static void MergeTags(this ITags resource, IDictionary<string, string> tags, bool overwriteExistingValues = true)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (resource.Tags is null)
                resource.Tags = new Dictionary<string, string>();

            if (overwriteExistingValues)
            {
                tags.ToList().ForEach(t => resource.Tags[t.Key] = t.Value);
            }
            else
            {
                var keyValuePairs = resource.Tags
                                .Concat(tags);

                resource.Tags = keyValuePairs
                    .GroupBy(kvp => kvp.Key)
                    .Where(kvp => kvp.First().Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
            }
        }

        public static void MergeProperties(this IProperties resource, IDictionary<string, string> properties, bool overwriteExistingValues = true)
        {
            if (resource is null)
                throw new ArgumentNullException(nameof(resource));

            if (resource.Properties is null)
                resource.Properties = new Dictionary<string, string>();

            if (overwriteExistingValues)
            {
                properties.ToList().ForEach(t => resource.Properties[t.Key] = t.Value);
            }
            else
            {
                var keyValuePairs = resource.Properties
                                .Concat(properties);

                resource.Properties = keyValuePairs
                    .GroupBy(kvp => kvp.Key)
                    .Where(kvp => kvp.First().Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.First().Value);
            }
        }
    }
}
