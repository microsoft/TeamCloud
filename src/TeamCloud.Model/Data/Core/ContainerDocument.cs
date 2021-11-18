/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Serialization;
using TeamCloud.Serialization;
using TeamCloud.Validation;

namespace TeamCloud.Model.Data.Core
{
    [JsonConverter(typeof(ContainerDocumentConverter))]
    public interface IContainerDocument : IIdentifiable, IValidatable, ICloneable
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
        [JsonProperty(Required = Required.Always)]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        DateTime? IContainerDocument.Timestamp { get; set; }

        string IContainerDocument.ETag { get; set; }

        public object Clone()
            => Clone(false);

        public virtual object Clone(bool reset)
        {
            var json = TeamCloudSerialize
                .SerializeObject(this);

            var clone = (ContainerDocument) TeamCloudSerialize
                .DeserializeObject(json, GetType());

            if (reset)
                clone.Id = Guid.NewGuid().ToString();

            return clone;
        }

        public override string ToString()
        {
            var attribute = GetType()
                .GetCustomAttributes(typeof(ContainerPathAttribute), false)
                .FirstOrDefault() as ContainerPathAttribute;

            return attribute?.ResolvePath(this) ?? $"{GetType().Name}@{Id}";
        }
    }
}
