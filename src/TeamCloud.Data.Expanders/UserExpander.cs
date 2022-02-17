/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders;

public sealed class UserExpander : DocumentExpander,
    IDocumentExpander<User>
{
    private readonly IGraphService graphService;

    public UserExpander(IGraphService graphService, TelemetryClient telemetryClient) : base(true, telemetryClient)
    {
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
    }

    public Task ExpandAsync(User document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var tasks = new List<Task>()
            {
                FetchAsync(async () =>
                {
                    document.DisplayName ??= await graphService
                        .GetDisplayNameAsync(document.Id)
                        .ConfigureAwait(false);
                }),
                FetchAsync(async () =>
                {
                    document.LoginName ??= await graphService
                        .GetLoginNameAsync(document.Id)
                        .ConfigureAwait(false);
                }),
                FetchAsync(async () =>
                {
                    document.MailAddress ??= await graphService
                        .GetMailAddressAsync(document.Id)
                        .ConfigureAwait(false);
                }),
            };

        return tasks.WhenAll();

        static Task FetchAsync(Func<Task> callback) => callback();
    }
}
