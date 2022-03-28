//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using TeamCloud.Model.Data;
using TeamCloud.Orchestration;
using TeamCloud.Orchestrator.Command.Activities.Organizations;
using TeamCloud.Serialization;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model.Commands.Core;
using Flurl;

namespace TeamCloud.Orchestrator.Command.Activities.Portal;

public sealed class PortalGrantPermissonsActivity
{
    private readonly IGraphService graphService;

    public PortalGrantPermissonsActivity(IGraphService graphService)
    {
        this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
    }

    [FunctionName(nameof(PortalGrantPermissonsActivity))]
    [RetryOptions(3)]
    public async Task<bool> Run(
    [ActivityTrigger] IDurableActivityContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        try
        {
            var organization = context.GetInput<Input>().Organization;
            var organizationPortalIdentity = $"Portal/{organization.Id}";

            switch (organization.Portal)
            {
                case PortalType.Backstage:

                    var resourcePermissions = new Dictionary<string, Dictionary<Guid, AccessType>>()
                    {

                        { "00000003-0000-0000-c000-000000000000", new Dictionary<Guid, AccessType>() // Graph API
                            {
                                { Guid.Parse("5f8c59db-677d-491f-a6b8-5f174b11ec1d"), AccessType.Scope }, // Group.Read.All
                                { Guid.Parse("bc024368-1153-4739-b217-4326f2e966d0"), AccessType.Scope }, // GroupMember.Read.All
                                { Guid.Parse("485be79e-c497-4b35-9400-0e3fa7f2a5d4"), AccessType.Scope }, // Team.ReadBasic.All
                                { Guid.Parse("2497278c-d82d-46a2-b1ce-39d4cdde5570"), AccessType.Scope }, // TeamMember.Read.All
                                { Guid.Parse("e1fe6dd8-ba31-4d61-89e7-88639da4683d"), AccessType.Scope }, // User.Read
                                { Guid.Parse("a154be20-db9c-4678-8ab7-66f6cc099a59"), AccessType.Scope }  // User.Read.All
                            }
                        }
                    };

                    await graphService
                        .SetResourcePermissionsAsync(organizationPortalIdentity, resourcePermissions)
                        .ConfigureAwait(false);

                    return true;
            }

            return false;
        }
        catch (Exception exc)
        {
            throw exc.AsSerializable();
        }
    }

    internal struct Input
    {
        public Organization Organization { get; set; }
    }
}
