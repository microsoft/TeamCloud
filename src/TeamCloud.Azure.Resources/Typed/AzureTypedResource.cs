/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Azure.Resources.Typed
{
    public abstract class AzureTypedResource : AzureResource
    {
        internal protected AzureTypedResource(string resourceType, string resourceId) : base(resourceId)
        {
            if (string.IsNullOrEmpty(resourceType))
                throw new ArgumentException("Must not NULL or EMPTY", nameof(resourceType));

            if (!ResourceId.ResourceTypeFullName.Equals(resourceType, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"ResourceId '{resourceId}' must contain resource type '{resourceType}'");
        }
    }
}
