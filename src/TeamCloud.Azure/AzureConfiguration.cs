/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Extensions.DependencyInjection;

namespace TeamCloud.Azure
{
    public interface IAzureConfiguration
    {
        IServiceCollection Services { get; }
    }

    internal sealed class AzureConfiguration : IAzureConfiguration
    {
        public AzureConfiguration(IServiceCollection services)
        {
            Services = services ?? throw new System.ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
