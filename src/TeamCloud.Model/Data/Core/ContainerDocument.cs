using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TeamCloud.Model.Data.Core
{
    public interface IContainerDocument : IIdentifiable
    {
        public static string GetPartitionKeyPath<T>(bool camelCase = false)
            where T : class, IContainerDocument
        {
            var name = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<PartitionKeyAttribute>() != null)
                .Single().Name;

            if (camelCase)
                name = new CamelCasePropertyNamesContractResolver().GetResolvedPropertyName(name);

            return $"/{name}";
        }

        public static IEnumerable<string> GetUniqueKeyPaths<T>(bool camelCase = false)
            where T : class, IContainerDocument
        {
            var resolver = new CamelCasePropertyNamesContractResolver();

            return typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<UniqueKeyAttribute>() != null)
                .Select(p => $"/{(camelCase ? resolver.GetResolvedPropertyName(p.Name) : p.Name)}");
        }

        [DatabaseIgnore]
        [JsonProperty("_timestamp")]
        DateTime? Timestamp { get; set; }

        [DatabaseIgnore]
        [JsonProperty("_etag")]
        string ETag { get; set; }
    }

    public abstract class ContainerDocument : IContainerDocument
    {
        public virtual string Id { get; set; }

        DateTime? IContainerDocument.Timestamp { get; set; }

        string IContainerDocument.ETag { get; set; }
    }

}
