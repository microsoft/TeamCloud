/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TeamCloud.Azure.Deployment;

public static class Extensions
{
    public static IAzureConfiguration AddDeployment(this IAzureConfiguration azureConfiguration)
    {
        if (azureConfiguration is null)
            throw new ArgumentNullException(nameof(azureConfiguration));

        azureConfiguration.Services
            .TryAddSingleton<IAzureDeploymentService, AzureDeploymentService>();

        return azureConfiguration;
    }

    public static void SetDeploymentArtifactsProvider<T>(this IAzureConfiguration azureConfiguration)
        where T : class, IAzureDeploymentArtifactsProvider
        => (azureConfiguration ?? throw new ArgumentNullException(nameof(azureConfiguration)))
        .AddDeployment().Services.AddSingleton<IAzureDeploymentArtifactsProvider, T>();

    internal static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> collection)
        => new Dictionary<TKey, TValue>(collection);

    internal static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        => new ReadOnlyDictionary<TKey, TValue>(dictionary);

    private static readonly AzureDeploymentState[] ProgressStates = new AzureDeploymentState[]
    {
                    AzureDeploymentState.Accepted,
                    AzureDeploymentState.Running,
                    AzureDeploymentState.Deleting
    };

    public static bool IsProgressState(this AzureDeploymentState state)
        => ProgressStates.Contains(state);

    private static readonly AzureDeploymentState[] FinalStates = new AzureDeploymentState[]
    {
            AzureDeploymentState.Succeeded,
            AzureDeploymentState.Cancelled,
            AzureDeploymentState.Failed
    };

    public static bool IsFinalState(this AzureDeploymentState state)
        => FinalStates.Contains(state);

    private static readonly AzureDeploymentState[] ErrorStates = new AzureDeploymentState[]
    {
            AzureDeploymentState.Cancelled,
            AzureDeploymentState.Failed
    };

    public static bool IsErrorState(this AzureDeploymentState state)
        => ErrorStates.Contains(state);
}
