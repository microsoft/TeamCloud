/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Azure.Resources;

public static class AzureRoleDefinition
{
    // https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#owner
    public static readonly Guid Owner = Guid.Parse("8e3af657-a8ff-443c-a75c-2fe8c4bcb635");

    // https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#contributor
    public static readonly Guid Contributor = Guid.Parse("b24988ac-6180-42a0-ab88-20f7382dd24c");

    // https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#reader
    public static readonly Guid Reader = Guid.Parse("acdd72a7-3385-48ef-bd42-f606fba81ae7");

    // https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#user-access-administrator
    // public static readonly Guid UserAccessAdministrator = Guid.Parse("18d7d88d-d35e-4fb5-a5c3-7773c20a72d9");

    // https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#devtest-labs-user
    // public static readonly Guid DevTestLabUser = Guid.Parse("76283e04-6283-4c54-8f91-bcf1374a3c64");

    // https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-kubernetes-service-cluster-admin-role
    // public static readonly Guid KubernetesServiceClusterAdmin = Guid.Parse("0ab0b1a8-8aac-4efd-b8c2-3ee1fb270be8");

    // https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-kubernetes-service-cluster-user-role
    // public static readonly Guid KubernetesServiceClusterUser = Guid.Parse("4abbcc35-e782-43d8-92c5-2d3f1bd2253f");
}
