# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

import json
from msrest.serialization import Serializer
from azure.core.pipeline.policies import SansIOHTTPPolicy
from azure.cli.core.profiles import ResourceType
from azure.cli.core.commands.client_factory import get_mgmt_service_client


class JsonCTemplate:
    def __init__(self, template_as_bytes):
        self.template_as_bytes = template_as_bytes


class JSONSerializer(Serializer):
    def body(self, data, data_type, **kwargs):
        if data_type in ('Deployment', 'ScopedDeployment', 'DeploymentWhatIf', 'ScopedDeploymentWhatIf'):
            # Be sure to pass a DeploymentProperties
            template = data.properties.template
            if template:
                data_as_dict = data.serialize()
                data_as_dict["properties"]["template"] = JsonCTemplate(template)

                return data_as_dict
        return super(JSONSerializer, self).body(data, data_type, **kwargs)


class JsonCTemplatePolicy(SansIOHTTPPolicy):

    def on_request(self, request):
        http_request = request.http_request
        if (getattr(http_request, 'data', {}) or {}).get('properties', {}).get('template'):
            template = http_request.data["properties"]["template"]
            if not isinstance(template, JsonCTemplate):
                raise ValueError()

            del http_request.data["properties"]["template"]
            # templateLink nad template cannot exist at the same time in deployment_dry_run mode
            if "templateLink" in http_request.data["properties"].keys():
                del http_request.data["properties"]["templateLink"]
            partial_request = json.dumps(http_request.data)

            http_request.data = partial_request[:-2] + ", template:" + template.template_as_bytes + r"}}"
            http_request.data = http_request.data.encode('utf-8')


def teamcloud_client_factory(cli_ctx, *_):
    from .vendored_sdks.teamcloud import TeamCloudClient
    return get_mgmt_service_client(cli_ctx, TeamCloudClient, subscription_bound=False, base_url_bound=False)


def storage_client_factory(cli_ctx, **_):
    return get_mgmt_service_client(cli_ctx, ResourceType.MGMT_STORAGE)


def web_client_factory(cli_ctx, **_):
    return get_mgmt_service_client(cli_ctx, ResourceType.MGMT_APPSERVICE)


def resource_client_factory(cli_ctx, **_):
    return get_mgmt_service_client(cli_ctx, ResourceType.MGMT_RESOURCE_RESOURCES)


def deployment_client_factory(cli_ctx, **_):

    smc = resource_client_factory(cli_ctx)

    deployment_client = smc.deployments  # This solves the multi-api for you

    deployment_client._serialize = JSONSerializer(
        deployment_client._serialize.dependencies
    )

    # Plug this as default HTTP pipeline
    from azure.core.pipeline import Pipeline
    smc._client._pipeline._impl_policies.append(JsonCTemplatePolicy())
    # Because JsonCTemplatePolicy needs to be wrapped as _SansIOHTTPPolicyRunner, so a new Pipeline is built
    smc._client._pipeline = Pipeline(
        policies=smc._client._pipeline._impl_policies,
        transport=smc._client._pipeline._transport
    )

    return deployment_client


def cosmosdb_client_factory(cli_ctx, **_):
    from azure.mgmt.cosmosdb import CosmosDBManagementClient
    return get_mgmt_service_client(cli_ctx, CosmosDBManagementClient)


def appconfig_client_factory(cli_ctx, **_):
    from azure.mgmt.appconfiguration import AppConfigurationManagementClient
    return get_mgmt_service_client(cli_ctx, AppConfigurationManagementClient)
