/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders;

public sealed class UserExpander : DocumentExpander,
    IDocumentExpander<User>
{
    private readonly IAzureDirectoryService azureDirectoryService;

    public UserExpander(IAzureDirectoryService azureDirectoryService, TelemetryClient telemetryClient) : base(true, telemetryClient)
    {
        this.azureDirectoryService = azureDirectoryService ?? throw new System.ArgumentNullException(nameof(azureDirectoryService));
    }

    public Task ExpandAsync(User document)
    {
        if (document is null)
            throw new System.ArgumentNullException(nameof(document));

        var tasks = new List<Task>()
            {
                FetchAsync(async () =>
                {
                    document.DisplayName ??= await azureDirectoryService
                        .GetDisplayNameAsync(document.Id)
                        .ConfigureAwait(false);
                }),
                FetchAsync(async () =>
                {
                    document.LoginName ??= await azureDirectoryService
                        .GetLoginNameAsync(document.Id)
                        .ConfigureAwait(false);
                }),
                FetchAsync(async () =>
                {
                    document.MailAddress ??= await azureDirectoryService
                        .GetMailAddressAsync(document.Id)
                        .ConfigureAwait(false);
                }),
            };

        return tasks.WhenAll();

        Task FetchAsync(Func<Task> callback) => callback();
    }
}
