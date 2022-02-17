/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Expanders;

public sealed class ProjectIdentityExpander : DocumentExpander,
    IDocumentExpander<ProjectIdentity>
{
    private readonly IGraphService graphService;

    public ProjectIdentityExpander(IGraphService graphService, TelemetryClient telemetryClient) : base(true, telemetryClient)
    {
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
    }

    public async Task ExpandAsync(ProjectIdentity document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        if (!(document.RedirectUrls?.Any() ?? false))
        {
            document.RedirectUrls = await graphService
                .GetServicePrincipalRedirectUrlsAsync(document.ObjectId.ToString())
                .ConfigureAwait(false);
        }
    }
}
