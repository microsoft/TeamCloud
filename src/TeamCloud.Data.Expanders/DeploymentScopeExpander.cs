/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Adapters;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders
{
    public sealed class DeploymentScopeExpander : DocumentExpander,
        IDocumentExpander<DeploymentScope>
    {
        private readonly IEnumerable<IAdapter> adapters;

        public DeploymentScopeExpander(IEnumerable<IAdapter> adapters) : base(false)
        {
            this.adapters = adapters ?? Enumerable.Empty<IAdapter>();
        }

        public async Task<DeploymentScope> ExpandAsync(DeploymentScope document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            var adapter = adapters
                .FirstOrDefault(a => a.Supports(document));

            if (adapter is IAdapterAuthorize adapterAuthorize)
            {
                document.Authorizable = true;
                document.Authorized = await adapterAuthorize
                    .IsAuthorizedAsync(document)
                    .ConfigureAwait(false);
            }
            else
            {
                document.Authorizable = false;
                document.Authorized = !(adapter is null);
            }

            return document;
        }
    }
}
