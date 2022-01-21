/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.AzureDevOps;

public sealed class AzureDevOpsSession : AuthorizationSession
{
    public static readonly string[] Scopes = new string[]
    {
        "vso.analytics",
        "vso.auditlog",
        "vso.auditstreams_delete",
        "vso.auditstreams_manage",
        "vso.build_execute",
        "vso.code_full",
        "vso.code_status",
        "vso.connected_server",
        "vso.dashboards_manage",
        "vso.entitlements",
        "vso.environment_manage",
        "vso.extension.data_write",
        "vso.extension_manage",
        "vso.gallery_acquire",
        "vso.gallery_manage",
        "vso.graph_manage",
        "vso.identity_manage",
        "vso.loadtest_write",
        "vso.machinegroup_manage",
        "vso.memberentitlementmanagement_write",
        "vso.notification_diagnostics",
        "vso.notification_manage",
        "vso.packaging_manage",
        "vso.profile_write",
        "vso.project_manage",
        "vso.release_manage",
        "vso.securefiles_manage",
        "vso.security_manage",
        "vso.serviceendpoint_manage",
        "vso.symbols_manage",
        "vso.taskgroups_manage",
        "vso.test_write",
        "vso.threads_full",
        "vso.tokenadministration",
        "vso.tokens",
        "vso.variablegroups_manage",
        "vso.wiki_write",
        "vso.work_full"
    };

    public AzureDevOpsSession() : this(null)
    { }

    public AzureDevOpsSession(DeploymentScope deploymentScope) : base(GetEntityId(deploymentScope))
    { }

    public string Organization { get; internal set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}
