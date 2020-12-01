// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Checkbox, IColumn } from '@fluentui/react';
import { DeploymentScope } from 'teamcloud';
import { useHistory, useParams } from 'react-router-dom';
import { ContentList } from '.';
import collaboration from '../img/MSC17_collaboration_010_noBG.png'

export interface IDeploymentScopeListProps {
    scopes?: DeploymentScope[];
}

export const DeploymentScopeList: React.FC<IDeploymentScopeListProps> = (props) => {

    const history = useHistory();
    const { orgId } = useParams() as { orgId: string };

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 240, fieldName: 'displayName' },
        { key: 'isDefault', name: 'Default', minWidth: 240, onRender: (s: DeploymentScope) => <Checkbox checked={s.isDefault} disabled /> },
        { key: 'subscriptionIds', name: 'Subscriptions', minWidth: 340, fieldName: 'subscriptionIds' },
        { key: 'managementGroupId', name: 'Management Group', minWidth: 340, onRender: (s: DeploymentScope) => s.managementGroupId?.split('/')[s.managementGroupId?.split('/').length - 1] },
    ];

    const _onItemInvoked = (scope: DeploymentScope): void => {
        console.log(scope);
    };

    return (
        <ContentList
            columns={columns}
            items={props.scopes}
            onItemInvoked={_onItemInvoked}
            filterPlaceholder='Filter members'
            buttonText='New scope'
            buttonIcon='Add'
            onButtonClick={() => history.push(`/orgs/${orgId}/settings/scopes/new`)}
            noDataTitle='You do not have any deployment scopes yet'
            noDataImage={collaboration}
            noDataDescription='Deployment Scopes are...'
            noDataButtonText='Create scope'
            noDataButtonIcon='Add'
            onNoDataButtonClick={() => history.push(`/orgs/${orgId}/settings/scopes/new`)}
        />
    );
}
