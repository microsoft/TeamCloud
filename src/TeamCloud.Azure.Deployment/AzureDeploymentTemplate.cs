﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace TeamCloud.Azure.Deployment;

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
    public static async Task<T> CreateAsync<T>(IServiceProvider serviceProvider = null)
        where T : IAzureDeploymentTemplate
    {
        var template = serviceProvider is null
            ? Activator.CreateInstance<T>()
            : ActivatorUtilities.CreateInstance<T>(serviceProvider);

        if (template is AzureDeploymentTemplate baseTemplate)
            await baseTemplate.OnCreateAsync().ConfigureAwait(false);

        return template;
    }

    private static Version GetTemplateVersion(string template)
    {
        if (string.IsNullOrEmpty(template))
            return null;

        var templateVersion = JObject
            .Parse(template)
            .SelectToken("$.contentVersion")?
            .ToString();

        if (Version.TryParse(templateVersion, out Version version))
            return version;

        return null;
    }


    public string Template { get; protected set; }

    public Version TemplateVersion => GetTemplateVersion(Template);

    public IDictionary<string, object> Parameters { get; protected set; } = new Dictionary<string, object>();

    public IDictionary<string, string> LinkedTemplates { get; protected set; } = new Dictionary<string, string>();

    protected virtual Task OnCreateAsync() => Task.CompletedTask;
}
