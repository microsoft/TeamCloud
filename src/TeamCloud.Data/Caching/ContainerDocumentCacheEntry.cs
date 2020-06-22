/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using TeamCloud.Model.Internal.Data.Core;

namespace TeamCloud.Data.Caching
{
    public sealed class ContainerDocumentCacheEntry<T>
        where T : class, IContainerDocument, new()
    {
        internal static string Serialize(ContainerDocumentCacheEntry<T> obj)
        {
            if (obj is null) return null;

            return JsonConvert.SerializeObject(obj);
        }

        internal static ContainerDocumentCacheEntry<T> Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            return JsonConvert.DeserializeObject<ContainerDocumentCacheEntry<T>>(json);
        }

        public ContainerDocumentCacheEntry()
        { }

        public ContainerDocumentCacheEntry(T containerDocument, string etag = default)
            : this()
        {
            Value = containerDocument ?? throw new System.ArgumentNullException(nameof(containerDocument));
            ETag = etag;
        }

        public ContainerDocumentCacheEntry(ItemResponse<T> response)
            : this(response?.Resource, response?.ETag)
        { }

        public T Value { get; set; }

        public string ETag { get; set; }
    }
}
