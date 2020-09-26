/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Azure.Resources;
using Xunit;

namespace TeamCloud.Azure.Tests.Resources
{
    public class AzureResourceIdentifierTests
    {
        [Theory]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG")]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/")]
        public void ParseResourceGroupId(string resourceId)
        {
            var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId);

            Assert.Equal(Guid.Empty, resourceIdentifier.SubscriptionId);
            Assert.Equal("TestRG", resourceIdentifier.ResourceGroup);
        }

        [Theory]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/TestResourceName")]
        [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/TestResourceName/")]
        public void ParseCustomResourceProviderResourceId(string resourceId)
        {
            var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId);

            Assert.Equal(Guid.Empty, resourceIdentifier.SubscriptionId);
            Assert.Equal("TestRG", resourceIdentifier.ResourceGroup);
            Assert.True(resourceIdentifier.ResourceTypes.Count == 2);

            Assert.Equal("resourceProviders", resourceIdentifier.ResourceTypes[0].Key);
            Assert.Equal("TestProviderName", resourceIdentifier.ResourceTypes[0].Value);

            Assert.Equal("TestResourceType", resourceIdentifier.ResourceTypes[1].Key);
            Assert.Equal("TestResourceName", resourceIdentifier.ResourceTypes[1].Value);
        }
    }
}
