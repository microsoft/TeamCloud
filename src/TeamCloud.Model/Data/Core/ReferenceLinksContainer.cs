/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Serialization;

namespace TeamCloud.Model.Data.Core
{
    [JsonConverter(typeof(ReferenceLinksContainerConverter))]
    public abstract class ReferenceLinksContainer<TContext, TImplementation>
        where TContext : class
        where TImplementation : ReferenceLinksContainer<TContext, TImplementation>
    {
        private readonly ConcurrentDictionary<string, ReferenceLink> links
            = new ConcurrentDictionary<string, ReferenceLink>();

        private readonly WeakReference<TContext> reference;

        protected ReferenceLinksContainer(TContext context = null)
        {
            reference = new WeakReference<TContext>(context);
        }

        protected TContext Context
            => reference.TryGetTarget(out TContext context) ? context : null;

        public TImplementation SetContext(TContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (!ReferenceEquals(Context, context))
            {
                if (Context is not null)
                    throw new NotSupportedException("The context cannot be changed once it was set.");

                reference.SetTarget(context);
            }

            return (TImplementation)this;
        }

        protected Uri GetBaseUri()
            => Uri.TryCreate(ReferenceLink.BaseUrl, UriKind.Absolute, out var baseUri) ? baseUri : null;

        protected ReferenceLink GetLink([CallerMemberName] string name = null)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            return links.GetOrAdd(name, (n) => new ReferenceLink());
        }

        protected ReferenceLink SetLink([CallerMemberName] string name = null, ReferenceLink link = null, bool replace = false)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (link is null)
            {
                if (links.TryGetValue(name, out var l))
                {
                    link = UpdateLink(l);
                }
            }
            else
            {
                link = links.AddOrUpdate(name, link, (n, l) =>
                {
                    return replace ? link : UpdateLink(l);
                });
            }

            return link;

            ReferenceLink UpdateLink(ReferenceLink existingLink)
            {
                existingLink.HRef = link?.Materialized ?? false ? link.HRef : null;

                return existingLink;
            }
        }
    }
}
