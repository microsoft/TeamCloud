/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data.Providers;

public sealed class DocumentExpanderProvider : IDocumentExpanderProvider
{
    private readonly IServiceProvider serviceProvider;

    public DocumentExpanderProvider(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IEnumerable<IDocumentExpander> GetExpanders(IContainerDocument document, bool includeOptional)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        return serviceProvider
            .GetServices<IDocumentExpander>()
            .Where(s => (includeOptional || s.Optional == includeOptional) && s.CanExpand(document));
    }
}
