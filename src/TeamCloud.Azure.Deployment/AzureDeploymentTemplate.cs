/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeploymentTemplate
    {
        const string ArtifactsLocationParameterName = "_artifactsLocation";
        const string ArtifactsLocationSasTokenParameterName = "_artifactsLocationSasToken";

        string Template { get; }

        IDictionary<string, object> Parameters { get; }

        IDictionary<string, string> LinkedTemplates { get; }
    }

    public abstract class AzureDeploymentTemplate : IAzureDeploymentTemplate
    {
        public static async Task<T> CreateAsync<T>()
            where T : IAzureDeploymentTemplate, new()
        {
            var template = Activator.CreateInstance<T>();

            if (template is AzureDeploymentTemplate baseTemplate)
                await baseTemplate.OnCreateAsync().ConfigureAwait(false);

            return template;
        }

        public static async Task<T> CreateAsync<T>(IServiceProvider serviceProvider)
            where T : IAzureDeploymentTemplate
        {
            if (serviceProvider is null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var template = ActivatorUtilities.CreateInstance<T>(serviceProvider);

            if (template is AzureDeploymentTemplate baseTemplate)
                await baseTemplate.OnCreateAsync().ConfigureAwait(false);

            return template;
        }

        public string Template { get; protected set; }

        public IDictionary<string, object> Parameters { get; protected set; } = new Dictionary<string, object>();

        public IDictionary<string, string> LinkedTemplates { get; protected set; } = new Dictionary<string, string>();

        protected virtual Task OnCreateAsync() => Task.CompletedTask;
    }


}
