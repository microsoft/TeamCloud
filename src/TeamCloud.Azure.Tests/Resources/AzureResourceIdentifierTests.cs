/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Azure.Resources;
using Xunit;

namespace TeamCloud.Azure.Tests.Resources;

public class AzureResourceIdentifierTests
{
    private static readonly Guid SUBSCRIPTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Theory]
    [InlineData("/providers/Microsoft.Management/managementGroups/11111111-1111-1111-1111-111111111111")]
    [InlineData("/providers/Microsoft.Management/managementGroups/11111111-1111-1111-1111-111111111111/")]
    public void ParseManagementGroupId(string resourceId)
    {
        var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId);

        Assert.Equal(Guid.Empty, resourceIdentifier.SubscriptionId);
        Assert.Null(resourceIdentifier.ResourceGroup);

        Assert.Equal("managementGroups", resourceIdentifier.ResourceTypes[0].Key);
        Assert.Equal("11111111-1111-1111-1111-111111111111", resourceIdentifier.ResourceTypes[0].Value);
    }

    [Theory]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/TestRG")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/TestRG/")]
    public void ParseResourceGroupId(string resourceId)
    {
        var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId);

        Assert.Equal(SUBSCRIPTION_ID, resourceIdentifier.SubscriptionId);
        Assert.Equal("TestRG", resourceIdentifier.ResourceGroup);
    }

    [Theory]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/TestResourceName")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/TestResourceName/")]
    public void ParseResourceId(string resourceId)
    {
        var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId);

        Assert.Equal(SUBSCRIPTION_ID, resourceIdentifier.SubscriptionId);
        Assert.Equal("TestRG", resourceIdentifier.ResourceGroup);
        Assert.True(resourceIdentifier.ResourceTypes.Count == 2);

        Assert.Equal("resourceProviders", resourceIdentifier.ResourceTypes[0].Key);
        Assert.Equal("TestProviderName", resourceIdentifier.ResourceTypes[0].Value);

        Assert.Equal("TestResourceType", resourceIdentifier.ResourceTypes[1].Key);
        Assert.Equal("TestResourceName", resourceIdentifier.ResourceTypes[1].Value);
    }

    [Theory]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/")]
    public void ParseResourceIdUnnamed(string resourceId)
    {
        var resourceIdentifier = AzureResourceIdentifier.Parse(resourceId, allowUnnamedResource: true);

        Assert.Equal(SUBSCRIPTION_ID, resourceIdentifier.SubscriptionId);
        Assert.Equal("TestRG", resourceIdentifier.ResourceGroup);
        Assert.True(resourceIdentifier.ResourceTypes.Count == 2);
        Assert.Null(resourceIdentifier.ResourceName);

        Assert.Equal("resourceProviders", resourceIdentifier.ResourceTypes[0].Key);
        Assert.Equal("TestProviderName", resourceIdentifier.ResourceTypes[0].Value);

        Assert.Equal("TestResourceType", resourceIdentifier.ResourceTypes[1].Key);
        Assert.Null(resourceIdentifier.ResourceTypes[1].Value);
    }

    [Theory]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/TestRG/providers/Microsoft.CustomProviders/resourceProviders/TestProviderName/TestResourceType/")]
    public void ParseResourceIdUnnamedNotAllowed(string resourceId)
    {
        Assert.Throws<ArgumentException>(() => AzureResourceIdentifier.Parse(resourceId, allowUnnamedResource: false));
    }
}
