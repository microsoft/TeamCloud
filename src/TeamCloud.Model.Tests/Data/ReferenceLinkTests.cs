/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Data.Core;
using Xunit;

namespace TeamCloud.Model.Data
{
    public class ReferenceLinkTests
    {
        public ReferenceLinkTests()
        {
            ReferenceLink.BaseUrl = "http://localhost";
        }

        [Fact]
        public void HRefEqualsToString()
        {
            Assert.NotNull(ReferenceLink.BaseUrl);

            var mockOwner = new MockOwner();

            Assert.Equal(mockOwner.Links.Self.HRef, mockOwner.Links.Self.ToString());
        }

        [Fact]
        public void HRefWithToken()
        {
            Assert.NotNull(ReferenceLink.BaseUrl);

            var mockOwner = new MockOwner();
            var mockOwnerIdToFind = Guid.NewGuid();

            var href = mockOwner.Links.Find.ToString((token) => mockOwnerIdToFind.ToString());

            Assert.EndsWith(mockOwnerIdToFind.ToString(), href);
        }

        [Fact]
        public void LinksCalculatedForFreshInstances()
        {
            Assert.NotNull(ReferenceLink.BaseUrl);

            var mockOwner = new MockOwner();

            Assert.StartsWith(ReferenceLink.BaseUrl, mockOwner.Links.Self.ToString());
            Assert.EndsWith(mockOwner.Id.ToString(), mockOwner.Links.Self.ToString());

            mockOwner.Id = Guid.NewGuid();

            Assert.StartsWith(ReferenceLink.BaseUrl, mockOwner.Links.Self.ToString());
            Assert.EndsWith(mockOwner.Id.ToString(), mockOwner.Links.Self.ToString());
        }

        [Fact]
        public void LinksMaterializedOnDeserializedInstances()
        {
            Assert.NotNull(ReferenceLink.BaseUrl);

            var mockOwnerJson = JsonConvert.SerializeObject(new MockOwner());
            var mockOwner = JsonConvert.DeserializeObject<MockOwner>(mockOwnerJson);

            var mockOwnerId = mockOwner.Id;
            mockOwner.Id = Guid.NewGuid(); // this must not affect the self link

            Assert.StartsWith(ReferenceLink.BaseUrl, mockOwner.Links.Self.ToString());
            Assert.EndsWith(mockOwnerId.ToString(), mockOwner.Links.Self.ToString());
        }

        [Fact]
        public void SerializeDeserialize()
        {
            Assert.NotNull(ReferenceLink.BaseUrl);

            var mockOwner = new MockOwner();
            var mockOwnerJson = JObject.FromObject(mockOwner);

            AssertMockOwnerJson(mockOwnerJson);

            var mockOwner2 = mockOwnerJson.ToObject<MockOwner>();
            var mockOwnerJson2 = JObject.FromObject(mockOwner2);

            AssertMockOwnerJson(mockOwnerJson2);

            Assert.Equal(mockOwnerJson, mockOwnerJson2);

            void AssertMockOwnerJson(JObject json)
            {
                Assert.NotNull(json.SelectToken("$._links._self"));
            }
        }

        public interface IMockOwner
        {
            Guid Id { get; set; }
        }

        public class MockOwner : ReferenceLinksAccessor<MockOwner, MockOwnerLinks>, IMockOwner
        {
            public Guid Id { get; set; } = Guid.NewGuid();
        }

        public sealed class MockOwnerLinks : ReferenceLinksContainer<MockOwner, MockOwnerLinks>
        {
            public MockOwnerLinks() : this(null)
            { }

            public MockOwnerLinks(MockOwner context) : base(context)
            {
                SetLink(nameof(Self), new ReferenceLink(()
                    => GetBaseUri()?.AppendPath($"api/foo/{Context?.Id}").ToString()));

                SetLink(nameof(Find), new ReferenceLink(()
                    => GetBaseUri()?.AppendPath($"api/foo/{{?ownerIdOrName}}").ToString()));
            }

            [JsonProperty("_self", Order = int.MinValue)]
            public ReferenceLink Self
            {
                get => GetLink();
                private set => SetLink(link: value);
            }

            public ReferenceLink Find
            {
                get => GetLink();
                private set => SetLink(link: value);
            }

        }
    }
}
