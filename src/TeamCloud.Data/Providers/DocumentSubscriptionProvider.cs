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

public sealed class DocumentSubscriptionProvider : IDocumentSubscriptionProvider
{
    private readonly IServiceProvider serviceProvider;

    public DocumentSubscriptionProvider(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IEnumerable<IDocumentSubscription> GetSubscriptions(IContainerDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        return serviceProvider
            .GetServices<IDocumentSubscription>()
            .Where(s => s.CanHandle(document));
    }
}
