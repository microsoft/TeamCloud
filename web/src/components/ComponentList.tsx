// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, useEffect, useState } from 'react';
import { FontIcon, IColumn, Persona, PersonaSize, Stack, Text } from '@fluentui/react';
import { useHistory, useParams } from 'react-router-dom';
import { Component, ComponentTemplate } from 'teamcloud';
import { ContentList, UserPersona } from '.';
import { OrgContext, ProjectContext } from '../Context';
import collaboration from '../img/MSC17_collaboration_010_noBG.png'
import DevOps from '../img/devops.svg';
import GitHub from '../img/github.svg';
import Resource from '../img/resource.svg';
import { ComponentLink } from './ComponentLink';
import { ComponentTemplateLink } from './ComponentTemplateLink';

export interface IComponentListProps {
    onItemInvoked?: (component: Component) => void;
}

export const ComponentList: React.FC<IComponentListProps> = (props) => {

    const history = useHistory();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [items, setItems] = useState<{ component: Component, template: ComponentTemplate }[]>()

    const { org, scopes } = useContext(OrgContext);
    const { components, templates, members, onComponentSelected } = useContext(ProjectContext);

    useEffect(() => {
        if (components && templates && (items === undefined || items.length !== components.length)) {
            setItems(components.map(c => ({ component: c, template: templates.find(t => t.id === c.templateId)! })));
        }
    }, [components, templates, items]);


    const _getTypeImage = (template: ComponentTemplate) => {
        const provider = template.repository.provider.toLowerCase();
        switch (template.type) {
            // case 'Custom': return 'Link';
            // case 'Readme': return 'PageList';
            case 'Environment': return Resource;
            case 'AzureResource': return Resource;
            case 'GitRepository': return provider === 'github' ? GitHub : provider === 'devops' ? DevOps : undefined;
        }
        return undefined;
    };

    const _getTypeIcon = (template: ComponentTemplate) => {
        if (template.type)
            switch (template.type) { // VisualStudioIDELogo32
                case 'Custom': return 'Link'; // Link12, FileSymlink, OpenInNewWindow, VSTSLogo
                case 'Readme': return 'PageList'; // Preview, Copy, FileHTML, FileCode, MarkDownLanguage, Document
                case 'Environment': return 'AzureLogo'; // Processing, Settings, Globe, Repair
                case 'AzureResource': return 'AzureLogo'; // AzureServiceEndpoint
                case 'GitRepository': return 'OpenSource';
                default: return undefined;
            }
    };


    const onRenderNameColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        // const name = item.displayName?.replaceAll('-', ' ');
        return (
            <Stack tokens={{ padding: '5px' }}>
                <Persona
                    text={item.component.displayName ?? undefined}
                    size={PersonaSize.size32}
                    imageUrl={_getTypeImage(item.template)}
                    coinProps={{ styles: { initials: { borderRadius: '4px' } } }}
                    styles={{
                        root: { color: 'inherit' },
                        primaryText: { color: 'inherit', textTransform: 'capitalize' }
                    }} />
            </Stack>
        );
    };


    const onRenderTypeColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return (
            <Stack horizontal >
                <FontIcon iconName={_getTypeIcon(item.template)} className='component-type-icon' />
                <Text styles={{ root: { paddingLeft: '4px' } }}>{item.template.type}</Text>
            </Stack>
        )
    };

    const onRenderTemplateColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return <ComponentTemplateLink componentTemplate={item.template} />
    };


    const onRenderCreatorColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        const creator = members?.find(m => m.user.id === item.component.creator);
        return (
            <UserPersona user={creator?.graphUser} size={PersonaSize.size24} />
            // <Persona
            //     text={creator?.graphUser?.displayName ?? creator?.user.id}
            //     // showSecondaryText
            //     // secondaryText={creator?.graphUser?.mail ?? (creator?.graphUser?.otherMails && creator.graphUser.otherMails.length > 0 ? creator.graphUser.otherMails[0] : undefined)}
            //     imageUrl={creator?.graphUser?.imageUrl}
            //     // styles={{ root: { paddingTop: '24px' } }}
            //     size={PersonaSize.size24} />

            // <Stack horizontal >
            //     <Image src={_getRepoImage(item.template)} styles={{ image: { width: '18px', height: '18px' } }} />
            //     <Text styles={{ root: { paddingLeft: '4px' } }}>{name}</Text>
            // </Stack>
        )
    };

    const onRenderLinkColumn = (item?: { component: Component, template: ComponentTemplate }) => {
        if (!item || !org) return undefined;
        return <ComponentLink component={item.component} />
    };

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 220, isResizable: false, onRender: onRenderNameColumn, styles: { cellName: { paddingLeft: '5px' } } },
        { key: 'type', name: 'Type', minWidth: 150, maxWidth: 150, isResizable: false, onRender: onRenderTypeColumn },
        { key: 'link', name: 'Link', minWidth: 200, maxWidth: 200, onRender: onRenderLinkColumn },
        { key: 'repository', name: 'Template', minWidth: 280, maxWidth: 280, onRender: onRenderTemplateColumn },
        { key: 'scope', name: 'Scope', minWidth: 110, maxWidth: 110, isResizable: false, onRender: (i: { component: Component, template: ComponentTemplate }) => scopes?.find(s => s.id === i.component.deploymentScopeId)?.displayName },
        { key: 'state', name: 'State', minWidth: 120, maxWidth: 120, onRender: (i: { component: Component, template: ComponentTemplate }) => i.component.resourceState },
        // { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
        // { key: 'blank', name: '', minWidth: 40, maxWidth: 40, onRender: (_: ComponentTemplate) => undefined },
        // { key: 'version', name: 'Version', minWidth: 80, maxWidth: 80, onRender: (i: { component: Component, template: ComponentTemplate }) => i.template.repository.version },
        { key: 'requestedBy', name: 'Creator', minWidth: 180, maxWidth: 180, onRender: onRenderCreatorColumn },
    ];


    // const columns: IColumn[] = [
    //     { key: 'displayName', name: 'Name', minWidth: 240, isResizable: false, fieldName: 'displayName' },
    //     { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
    //     { key: 'provider', name: 'Provider', minWidth: 240, fieldName: 'provider' },
    //     { key: 'type', name: 'Type', minWidth: 240, fieldName: 'type' },
    //     { key: 'requestedBy', name: 'Creator', minWidth: 240, fieldName: 'requestedBy' },
    // ];

    const _applyFilter = (item: { component: Component, template: ComponentTemplate }, filter: string): boolean => {
        // const f = filter?.toUpperCase();
        // if (!f) return true;
        return filter ? JSON.stringify(item).toUpperCase().includes(filter.toUpperCase()) : true;
    };

    const _onItemInvoked = (item: { component: Component, template: ComponentTemplate }): void => {
        // console.log(item);
        onComponentSelected(item.component);
        history.push(`/orgs/${orgId}/projects/${projectId}/components/${item.component.slug}`);
    };

    return (
        <ContentList
            columns={columns}
            items={items}
            applyFilter={_applyFilter}
            // onItemInvoked={props.onItemInvoked !== undefined ? (i) => props.onItemInvoked!(i.component) : undefined}
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
            onNoDataButtonClick={() => history.push(`/orgs/${orgId}/projects/${projectId}/components/new`)} />
    );
}
