// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Checkbox, IColumn, DefaultButton } from '@fluentui/react';
import { DeploymentScope } from 'teamcloud';
import { ContentList } from '.';
import { useOrg, useDeploymentScopes } from '../hooks';

import collaboration from '../img/MSC17_collaboration_010_noBG.png'
import { useAuthorizeDeployemntScope } from '../hooks/useAuthorizeDeploymentScope';

export const DeploymentScopeList: React.FC = () => {

    const navigate = useNavigate();

    const { data: org } = useOrg();
    const { data: scopes } = useDeploymentScopes();

    const authorizeDeploymentScope = useAuthorizeDeployemntScope();

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 200, fieldName: 'displayName' },
        { key: 'type', name: 'Type', minWidth: 200, fieldName: 'type' },
        { key: 'authorized', name: 'Authorized', minWidth: 100, onRender: (s: DeploymentScope) => <Checkbox checked={s.authorized} disabled /> },
        { key: 'isDefault', name: 'Default', minWidth: 100, onRender: (s: DeploymentScope) => <Checkbox checked={s.isDefault} disabled /> },
        { key: 'subscriptionIds', name: 'Subscriptions', minWidth: 280, fieldName: 'subscriptionIds' },
        { key: 'managementGroupId', name: 'Management Group', minWidth: 240, onRender: (s: DeploymentScope) => s.managementGroupId?.split('/')[s.managementGroupId?.split('/').length - 1] },
        { key: 'authorize', name: '', minWidth: 100, onRender: (s: DeploymentScope) => s.authorizable === true ? <DefaultButton text='authorize' iconProps={{ iconName: 'Permissions' }} onClick={() => _onItemAuthorize(s)} /> : <></> }
    ];

    const _onItemAuthorize = async (scope: DeploymentScope): Promise<void> => {
        if (scope?.authorizeUrl) {
            let authorizedScope = await authorizeDeploymentScope(scope);
            window.open(authorizedScope?.authorizeUrl ?? scope?.authorizeUrl, "_blank");
            // window.open(scope?.authorizeUrl, "_self");
        }
    };

    const _onItemInvoked = (scope: DeploymentScope): void => {
        console.log(scope);
    };

    return org ? (
        <ContentList
            columns={columns}
            items={scopes ?? undefined}
            onItemInvoked={_onItemInvoked}
            filterPlaceholder='Filter deployment scopes'
            buttonText='New scope'
            buttonIcon='Add'
            onButtonClick={() => navigate(`/orgs/${org.slug}/settings/scopes/new`)}
            noDataTitle='You do not have any deployment scopes yet'
            noDataImage={collaboration}
            noDataDescription='Deployment Scopes are...'
            noDataButtonText='Create scope'
            noDataButtonIcon='Add'
            onNoDataButtonClick={() => navigate(`/orgs/${org.slug}/settings/scopes/new`)} />
    ) : <></>;
}
