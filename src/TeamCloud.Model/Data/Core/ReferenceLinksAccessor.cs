using System;
using Newtonsoft.Json;

namespace TeamCloud.Model.Data.Core
{
    public interface IReferenceLinksAccessor<TContext, TContainer>
        where TContext : class, IReferenceLinksAccessor<TContext, TContainer>
        where TContainer : ReferenceLinksContainer<TContext, TContainer>, new()
    {
        [JsonProperty("_links", Order = int.MaxValue)]
        TContainer Links { get; set; }
    }

    public abstract class ReferenceLinksAccessor<TContext, TContainer>
        : IReferenceLinksAccessor<TContext, TContainer>
        where TContext : class, IReferenceLinksAccessor<TContext, TContainer>
        where TContainer : ReferenceLinksContainer<TContext, TContainer>, new()
    {
        private TContainer links;

        public TContainer Links
        {
            get => links ??= Activator.CreateInstance<TContainer>().SetContext(this as TContext);
            set => links = value?.SetContext(this as TContext) ?? throw new ArgumentNullException();
        }
    }
}