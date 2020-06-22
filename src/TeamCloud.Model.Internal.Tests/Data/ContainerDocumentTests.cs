/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json.Linq;
using TeamCloud.Model.Internal.Data.Core;
using Xunit;

namespace TeamCloud.Model.Internal.Data
{
    public class ContainerDocumentTests
    {
        [Theory]
        [InlineData(typeof(TeamCloudInstance))]
        [InlineData(typeof(Project))]
        [InlineData(typeof(ProjectType))]
        [InlineData(typeof(Provider))]
        [InlineData(typeof(User))]
        public void Serialize(Type type)
        {
            var containerDocument = (IContainerDocument)Activator.CreateInstance(type);

            containerDocument.Id = Guid.NewGuid().ToString();

            var containerDocumentJson = JObject.FromObject(containerDocument);

            Assert.Null(containerDocumentJson.SelectToken("$._etag"));
            Assert.Null(containerDocumentJson.SelectToken("$._timestamp"));

            var containerDocument2 = (IContainerDocument)containerDocumentJson.ToObject(type);

            Assert.Equal(containerDocument.ETag, containerDocument2.ETag);
            Assert.Equal(containerDocument.Timestamp, containerDocument2.Timestamp);
        }

        [Theory]
        [InlineData(typeof(TeamCloudInstance))]
        [InlineData(typeof(Project))]
        [InlineData(typeof(ProjectType))]
        [InlineData(typeof(Provider))]
        [InlineData(typeof(User))]
        public void SerializeWithMetadata(Type type)
        {
            var containerDocument = (IContainerDocument)Activator.CreateInstance(type);

            containerDocument.Id = Guid.NewGuid().ToString();
            containerDocument.ETag = Guid.NewGuid().ToString();
            containerDocument.Timestamp = DateTime.UtcNow;

            var containerDocumentJson = JObject.FromObject(containerDocument);

            Assert.NotNull(containerDocumentJson.SelectToken("$._etag"));
            Assert.NotNull(containerDocumentJson.SelectToken("$._timestamp"));

            var containerDocument2 = (IContainerDocument)containerDocumentJson.ToObject(type);

            Assert.Equal(containerDocument.ETag, containerDocument2.ETag);
            Assert.Equal(containerDocument.Timestamp, containerDocument2.Timestamp);
        }
    }
}
