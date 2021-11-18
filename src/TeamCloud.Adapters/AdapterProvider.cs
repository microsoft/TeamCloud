/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TeamCloud.Model.Data;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Adapters
{
    public sealed class AdapterProvider : IAdapterProvider, IAdapterProviderConfig
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

        IAdapterProviderConfig IAdapterProviderConfig.Register<TAdapter>()
        {
            adapterTypes.Add(typeof(TAdapter));

            var validatorProvider = serviceProvider.GetService<IValidatorProvider>();

            if (validatorProvider is IValidatorProviderConfig config)
            {
                config?.Register(typeof(TAdapter).Assembly);
            }

            return this; // fluent style
        }
    }
}
