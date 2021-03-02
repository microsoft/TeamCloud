# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=unused-argument, protected-access, too-many-lines

from knack.util import CLIError
from knack.log import get_logger

logger = get_logger(__name__)


def _ensure_base_url(client, base_url):
    client._client._base_url = base_url

# TeamCloud


def teamcloud_update(cmd, version=None, prerelease=False):  # pylint: disable=too-many-statements, too-many-locals
    import os
    import tempfile
    import shutil
    from azure.cli.core import CommandIndex
    from azure.cli.core.extension import get_extension
    from azure.cli.core.extension.operations import _add_whl_ext, _augment_telemetry_with_ext_info
    from ._deploy_utils import get_github_release

    release = get_github_release(cmd.cli_ctx, 'TeamCloud', version=version, prerelease=prerelease)
    asset = next((a for a in release['assets']
                  if 'py3-none-any.whl' in a['browser_download_url']), None)

    download_url = asset['browser_download_url'] if asset else None

    if not download_url:
        raise CLIError(
            'Could not find extension .whl asset on release {}. '
            'Specify a specific prerelease version with --version '
            'or use latest prerelease with --pre'.format(release['tag_name']))

    extension_name = 'tc'
    ext = get_extension(extension_name)
    cur_version = ext.get_version()
    # Copy current version of extension to tmp directory in case we need to restore it after a failed install.
    backup_dir = os.path.join(tempfile.mkdtemp(), extension_name)
    extension_path = ext.path
    logger.debug('Backing up the current extension: %s to %s', extension_path, backup_dir)
    shutil.copytree(extension_path, backup_dir)
    # Remove current version of the extension
    shutil.rmtree(extension_path)
    # Install newer version
    try:
        _add_whl_ext(cli_ctx=cmd.cli_ctx, source=download_url)
        logger.debug('Deleting backup of old extension at %s', backup_dir)
        shutil.rmtree(backup_dir)
        # This gets the metadata for the extension *after* the update
        _augment_telemetry_with_ext_info(extension_name)
    except Exception as err:
        logger.error('An error occurred whilst updating.')
        logger.error(err)
        logger.debug('Copying %s to %s', backup_dir, extension_path)
        shutil.copytree(backup_dir, extension_path)
        raise CLIError('Failed to update. Rolled {} back to {}.'.format(
            extension_name, cur_version))
    CommandIndex().invalidate()


def teamcloud_info(cmd, client, base_url):
    _ensure_base_url(client, base_url)
    return client.get_team_cloud_instance()


def status_get(cmd, client, base_url, tracking_id, project=None):
    _ensure_base_url(client, base_url)
    return client.get_project_status(project, tracking_id) if project else client.get_status(tracking_id)


# Orgs

def org_create(cmd, client, base_url, name, location=None):
    from .vendored_sdks.teamcloud.models import OrganizationDefinition
    from azure.cli.core.commands.client_factory import get_subscription_id

    subscription_id = get_subscription_id(cmd.cli_ctx)

    payload = OrganizationDefinition(display_name=name, subscription_id=subscription_id, location=location)

    return _create(cmd, client, base_url, client.create_organization, payload)


def org_delete(cmd, client, base_url, org):
    return _delete(cmd, client, base_url, client.delete_organization, org)


def org_list(cmd, client, base_url):
    return _list(cmd, client, base_url, client.get_organizations)


def org_get(cmd, client, base_url, org):
    return _get(cmd, client, base_url, client.get_organization, org)


# Deployment Scopes

def deployment_scope_create(cmd, client, base_url, org, scope, subscriptions, default=False):
    from .vendored_sdks.teamcloud.models import DeploymentScopeDefinition

    payload = DeploymentScopeDefinition(display_name=scope, subscription_ids=subscriptions, is_default=default)

    return _create(cmd, client, base_url, client.create_deployment_scope, payload, org=org)


def deployment_scope_delete(cmd, client, base_url, org, scope):
    return _delete(cmd, client, base_url, client.delete_deployment_scope, scope, org=org)


def deployment_scope_list(cmd, client, base_url, org):
    return _list(cmd, client, base_url, client.get_deployment_scopes, org=org)


def deployment_scope_get(cmd, client, base_url, org, scope):
    return _get(cmd, client, base_url, client.get_deployment_scope, scope, org=org)


# Project Templates

def project_template_create(cmd, client, base_url, org, template, repo_url, repo_version=None, repo_token=None):
    from .vendored_sdks.teamcloud.models import ProjectTemplateDefinition, RepositoryDefinition
    repository = RepositoryDefinition(url=repo_url, version=repo_version, token=repo_token)
    payload = ProjectTemplateDefinition(display_name=template, repository=repository)
    return _create(cmd, client, base_url, client.create_project_template, payload, org=org)


def project_template_delete(cmd, client, base_url, org, template):
    return _delete(cmd, client, base_url, client.delete_project_template, template, org=org)


def project_template_list(cmd, client, base_url, org):
    return _list(cmd, client, base_url, client.get_project_templates, org=org)


def project_template_get(cmd, client, base_url, org, template):
    return _get(cmd, client, base_url, client.get_project_template, template, org=org)


# Common

def _create(cmd, client, base_url, func, payload, org=None, project=None):
    _ensure_base_url(client, base_url)
    return func(org, project, payload) if org and project else func(org, payload) if org else func(payload)


def _delete(cmd, client, base_url, func, item, org=None, project=None):
    _ensure_base_url(client, base_url)
    return func(org, project, item) if org and project else func(org, item) if org else func(item)


def _list(cmd, client, base_url, func, org=None, project=None):
    _ensure_base_url(client, base_url)
    return func(org, project) if org and project else func(org) if org else func()


def _get(cmd, client, base_url, func, item, org=None, project=None):
    _ensure_base_url(client, base_url)
    return func(org, project, item) if org and project else func(org, item) if org else func(item)
