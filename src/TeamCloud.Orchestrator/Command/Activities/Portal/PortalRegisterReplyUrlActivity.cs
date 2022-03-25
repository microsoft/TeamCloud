//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Serialization;
using TeamCloud.Microsoft.Graph;

namespace TeamCloud.Orchestrator.Command.Activities.Portal;

public sealed class PortalRegisterReplyUrlActivity
{
    private readonly IGraphService graphService;

    public PortalRegisterReplyUrlActivity(IGraphService graphService)
    {
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
    }

    [FunctionName(nameof(PortalRegisterReplyUrlActivity))]
    [RetryOptions(3)]
    public async Task Run(
    [ActivityTrigger] IDurableActivityContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        try
        {
            var organization = context.GetInput<Input>().Organization;
            var replyUrl = context.GetInput<Input>().ReplyUrl;

            if (!string.IsNullOrEmpty(organization?.PortalUrl) && !string.IsNullOrEmpty(replyUrl))
            {
                var identifier = $"Portal/{organization.Id}";

                var replyUrls = await graphService
                    .GetServicePrincipalRedirectUrlsAsync(identifier)
                    .ConfigureAwait(false);

                if (!replyUrls.Contains(replyUrl, StringComparer.OrdinalIgnoreCase))
                {
                    await graphService
                        .SetServicePrincipalRedirectUrlsAsync(identifier, replyUrls.Append(replyUrl).ToArray())
                        .ConfigureAwait(false);
                }
            }
        }
        catch (Exception exc)
        {
            throw exc.AsSerializable();
        }
    }

    internal struct Input
    {
        public Organization Organization { get; set; }

        public string ReplyUrl { get; set; }
    }
}
