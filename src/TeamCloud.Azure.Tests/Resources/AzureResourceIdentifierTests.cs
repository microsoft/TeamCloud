using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
