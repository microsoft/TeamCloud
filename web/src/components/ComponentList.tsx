// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { IColumn, Stack, Text } from '@fluentui/react';
import { useNavigate, useParams } from 'react-router-dom';
import { Component, ComponentTemplate } from 'teamcloud';
import { ContentList, ComponentLink, ComponentTemplateLink, UserPersona } from '.';
import { useDeploymentScopes, useProjectMembers, useProjectComponentTemplates, useProjectComponents } from '../hooks';

import collaboration from '../img/MSC17_collaboration_010_noBG.png'
import { ComponentIcon } from './ComponentIcon';

export interface IComponentListProps {
    // onItemInvoked?: (component: Component) => void;
}

export const ComponentList: React.FC<IComponentListProps> = (props) => {

    const navigate = useNavigate();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [items, setItems] = useState<{ component: Component, template: ComponentTemplate }[]>()

    const { data: scopes } = useDeploymentScopes();
    const { data: members } = useProjectMembers();
    const { data: components } = useProjectComponents();
    const { data: templates } = useProjectComponentTemplates();

    useEffect(() => {
        if (components && templates && (items === undefined || items.length !== components.length)) {
            setItems(components.map(c => ({ component: c, template: templates.find(t => t.id === c.templateId)! })));
        }
    }, [components, templates, items]);

    const onRenderNameColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return <Stack horizontal tokens={{ childrenGap: '10px' }} >
            <ComponentIcon component={item.component} />
            <Text>{item.component?.displayName}</Text>
        </Stack>
    };

    const onRenderTypeColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return <ComponentLink component={item.component} />
    };

    const onRenderTemplateColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return <ComponentTemplateLink componentTemplate={item.template} />
    };


    const onRenderCreatorColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        const creator = members?.find(m => m.user.id === item.component.creator);
        return (
            <UserPersona principal={creator?.graphPrincipal} showSecondaryText />
        )
    };

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 220, isResizable: false, onRender: onRenderNameColumn, styles: { cellName: { paddingLeft: '5px' } } },
        { key: 'type', name: 'Resource', minWidth: 250, maxWidth: 250, onRender: onRenderTypeColumn },
        { key: 'repository', name: 'Template', minWidth: 250, maxWidth: 250, onRender: onRenderTemplateColumn },
        { key: 'scope', name: 'Scope', minWidth: 250, maxWidth: 250, isResizable: false, onRender: (i: { component: Component, template: ComponentTemplate }) => scopes?.find(s => s.id === i.component.deploymentScopeId)?.displayName },
        { key: 'state', name: 'State', minWidth: 120, maxWidth: 120, onRender: (i: { component: Component, template: ComponentTemplate }) => i.component.resourceState },
        { key: 'requestedBy', name: 'Creator', minWidth: 200, maxWidth: 200, onRender: onRenderCreatorColumn },
    ];


    const _onItemInvoked = (item: { component: Component, template: ComponentTemplate }): void => {
        navigate(`/orgs/${orgId}/projects/${projectId}/components/${item.component.slug}`);
    };

    return (
        <ContentList
            columns={columns}
            items={items}
            onItemInvoked={_onItemInvoked}
            filterPlaceholder='Filter components'
            buttonText='Create component'
            buttonIcon='Add'
            onButtonClick={() => navigate(`/orgs/${orgId}/projects/${projectId}/components/new`)}
            noDataTitle='You do not have any components yet'
            noDataImage={collaboration}
            noDataDescription='Components are project resources like cloud environments'
            noDataButtonText='Create component'
            noDataButtonIcon='Add'
            onNoDataButtonClick={() => navigate(`/orgs/${orgId}/projects/${projectId}/components/new`)} />
    );
}
