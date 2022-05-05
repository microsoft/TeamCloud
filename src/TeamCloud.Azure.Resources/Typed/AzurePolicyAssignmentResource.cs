/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager;
// using Microsoft.Azure.Management.ResourceManager.Models;
using TeamCloud.Azure.Resources.Utilities;

namespace TeamCloud.Azure.Resources.Typed;

public sealed class AzurePolicyAssignmentResource : AzureTypedResource
{
    private const string InheritTagFromResourceGroupPolicyName = "cd3aa116-8754-49c9-a813-ad46512ece54";
    private const string InheritTagFromResourceGroupPolicyId = "/providers/Microsoft.Authorization/policyDefinitions/cd3aa116-8754-49c9-a813-ad46512ece54";

    private readonly AsyncLazy<IPolicyAssignment> policyInstance;

    public AzurePolicyAssignmentResource(string resourceId) : base("Microsoft.Authorization/policyAssignments", resourceId)
    {
        policyInstance = new AsyncLazy<IPolicyAssignment>(GetPolicyAsync);
    }

    private async Task<IPolicyAssignment> GetPolicyAsync()
    {
        var session = await AzureResourceService.AzureSessionService
            .CreateSessionAsync(ResourceId.SubscriptionId)
            .ConfigureAwait(false);

        return await session.PolicyAssignments
            .GetByIdAsync(ResourceId.ToString())
            .ConfigureAwait(false);
    }

    public async Task GetByResourceGroupAsync(IResourceGroup resourceGroup)
    {

        // IPolicyDefinition policyDefinition;

        // var policyDefinition = new PolicyDefinitionReference(InheritTagFromResourceGroupPolicy, new {
        //     tagName = "foo"
        // });

        var session = await AzureResourceService.AzureSessionService
            .CreateSessionAsync(ResourceId.SubscriptionId)
            .ConfigureAwait(false);

        var policyDefinition = await session.PolicyDefinitions
            .GetByNameAsync(InheritTagFromResourceGroupPolicyName)
            .ConfigureAwait(false);

        // var fooo = policyDefinition.


        var foo = await session.PolicyAssignments
            .Define("temporary-tags-policy-name")
            .ForScope("resource-group-id")
            .WithPolicyDefinition(policyDefinition)

            // .WithPolicyDefinitionId(InheritTagFromResourceGroupPolicy)
            .WithDefaultMode()
            .CreateAsync()
            .ConfigureAwait(false);

        // var ooo = await PolicyDefinitionsOperationsExtensions.GetBuiltInAsync()

        // var boo = new PolicyDefinitionReference(InheritTagFromResourceGroupPolicy, )
        // var foo = await session.PolicyAssignments
        // .Define("foo").ForResourceGroup(resourceGroup).WithPolicyDefinition()
        // .ForResourceGroup(resourceGroup).WithPolicyDefinitionId(InheritTagFromResourceGroupPolicy).WithDisplayName("foo").WithDefaultMode().CreateAsync()
        // .WithPolicyDefinition(new PolicyDefinition())
        // .WithPolicyDefinitionId("policyDefinitionId")
        // .WithPolicyDefinition(policyDefinition)
    }


}
