/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters
{
    public sealed class AdapterProvider : IAdapterProvider, IAdapterConfiguration
    {
        private static readonly HashSet<Type> adapterTypes = new HashSet<Type>();

        private readonly IServiceProvider serviceProvider;

        internal AdapterProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IEnumerable<IAdapter> GetAdapters()
            => adapterTypes.Select(adapterType => (IAdapter)ActivatorUtilities.CreateInstance(serviceProvider, adapterType));

        public IAdapter GetAdapter(DeploymentScopeType deploymentScopeType)
            => GetAdapters().SingleOrDefault(adapter => adapter.Type == deploymentScopeType);

        IAdapterConfiguration IAdapterConfiguration.Register<TAdapter>()
        {
            adapterTypes.Add(typeof(TAdapter));

            return this; // fluent style
        }
    }
}
