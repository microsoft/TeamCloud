// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { IColumn } from '@fluentui/react';
import { useHistory, useParams } from 'react-router-dom';
import { Project, Component } from 'teamcloud';
import { ContentList } from '.';
import collaboration from '../img/MSC17_collaboration_010_noBG.png'

export interface IComponentListProps {
    project?: Project;
    components?: Component[];
}

export const ComponentList: React.FC<IComponentListProps> = (props) => {

    const history = useHistory();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string; };

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 240, isResizable: false, fieldName: 'displayName' },
        { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
        { key: 'provider', name: 'Provider', minWidth: 240, fieldName: 'provider' },
        { key: 'type', name: 'Type', minWidth: 240, fieldName: 'type' },
        { key: 'requestedBy', name: 'Creator', minWidth: 240, fieldName: 'requestedBy' },
    ];

    const _applyFilter = (component: Component, filter: string): boolean => {
        const f = filter?.toUpperCase();
        if (!f) return true;
        return (
            component.displayName?.toUpperCase().includes(f)
            || component.id?.toUpperCase().includes(f)
            || component.description?.toUpperCase().includes(f)
            || component.templateId?.toUpperCase().includes(f)
            || component.provider?.toUpperCase().includes(f)
        ) ?? false

    };

    const _onItemInvoked = (component: Component): void => {
        console.log(component);
    };

    return (
        <ContentList
            columns={columns}
            items={props.components}
            applyFilter={_applyFilter}
            onItemInvoked={_onItemInvoked}
            filterPlaceholder='Filter components'
            buttonText='Create component'
            buttonIcon='Add'
            onButtonClick={() => history.push(`/orgs/${orgId}/projects/${projectId}/components/new`)}
            noDataTitle='You do not have any components yet'
            noDataImage={collaboration}
            noDataDescription='Components are project resources like cloud environments'
            noDataButtonText='Create component'
            noDataButtonIcon='Add'
            onNoDataButtonClick={() => history.push(`/orgs/${orgId}/projects/${projectId}/components/new`)}
        />
    );
}
