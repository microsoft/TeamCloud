/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.ResourceManager.AppService;
using Flurl;
using Flurl.Http;

namespace TeamCloud.Azure.AppService;

public interface IAppServiceService
{
    Task UpdateContainerAsync(string resourceId, CancellationToken cancellationToken = default);
}

public class AppServiceService : IAppServiceService
{
    private readonly IArmService arm;

    public AppServiceService(IArmService arm)
    {
        this.arm = arm ?? throw new ArgumentNullException(nameof(arm));
    }

    public async Task UpdateContainerAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        var id = new ResourceIdentifier(resourceId);

        var response = await arm
            .GetArmClient(id.SubscriptionId)
            .GetWebSiteResource(id)
            .GetPublishingCredentialsAsync(WaitUntil.Completed, cancellationToken)
            .ConfigureAwait(false);

        var profile = response.Value.Data;

        await $"https://{profile.ScmUri}"
            .AppendPathSegment("/docker/hook")
            .WithBasicAuth(profile.PublishingUserName, profile.PublishingPassword)
            .PostAsync()
            .ConfigureAwait(false);
    }
}