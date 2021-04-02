/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data.CosmosDb
{
    public sealed class CosmosDbDocumentExpanderProvider : IDocumentExpanderProvider
    {
        private readonly IServiceProvider serviceProvider;

        public CosmosDbDocumentExpanderProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IEnumerable<IDocumentExpander> GetExpanders(IContainerDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            return serviceProvider
                .GetServices<IDocumentExpander>()
                .Where(s => s.CanExpand(document));
        }
    }
}
