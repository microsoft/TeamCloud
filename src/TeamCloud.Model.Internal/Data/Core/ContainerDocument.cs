/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Internal.Data.Serialization;

namespace TeamCloud.Model.Internal.Data.Core
{
    [JsonConverter(typeof(ContainerDocumentConverter))]
    public interface IContainerDocument : IIdentifiable, IValidatable
    {
        [DatabaseIgnore]
        [JsonProperty("_timestamp")]
        DateTime? Timestamp { get; set; }

        [DatabaseIgnore]
        [JsonProperty("_etag")]
        string ETag { get; set; }
    }

    public abstract class ContainerDocument : IContainerDocument
    {
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        DateTime? IContainerDocument.Timestamp { get; set; }

        string IContainerDocument.ETag { get; set; }
    }
}
