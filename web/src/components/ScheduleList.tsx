// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { FontIcon, IColumn, Persona, PersonaSize, Stack, Text } from '@fluentui/react';
import { useHistory, useParams } from 'react-router-dom';
import { Component, ComponentTaskTemplate, ComponentTemplate, Schedule } from 'teamcloud';
import { ContentList, ComponentLink, ComponentTemplateLink, UserPersona } from '.';
import { useOrg, useDeploymentScopes, useProjectMembers, useProjectComponentTemplates, useProjectComponents, useProjectSchedules } from '../hooks';

import collaboration from '../img/MSC17_collaboration_010_noBG.png'
import DevOps from '../img/devops.svg';
import GitHub from '../img/github.svg';
import Resource from '../img/resource.svg';

export interface IScheduleListProps {
    onItemInvoked?: (schedule: Schedule, component: Component) => void;
}

export const ScheduleList: React.FC<IScheduleListProps> = (props) => {

    const history = useHistory();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [items, setItems] = useState<{ schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }[]>()

    const { data: org } = useOrg();
    const { data: scopes } = useDeploymentScopes();
    const { data: members } = useProjectMembers();
    const { data: components } = useProjectComponents();
    const { data: templates } = useProjectComponentTemplates();
    const { data: schedules } = useProjectSchedules();

    useEffect(() => {
        if (schedules && components && templates && (items === undefined || items.length !== schedules.length)) {
            const _items = schedules.map(s => {
                const _tasks = s.componentTasks?.map(ct => {
                    const _component = components.find(c => c.id === ct.componentId);
                    const _template = templates.find(t => t.id === _component?.templateId);
                    const _taskTemplate = _template?.tasks?.find(t => t.id === ct.componentTaskTemplateId);
                    return { component: _component, template: _template, taskTemplate: _taskTemplate };
                });
                return { schedule: s, tasks: _tasks };
            });
            setItems(_items);
        }
    }, [schedules, components, templates, items]);


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


    // const onRenderNameColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
    //     if (!item) return undefined;
    //     return (
    //         <Stack tokens={{ padding: '5px' }}>
    //             <Persona
    //                 text={item.component.displayName ?? undefined}
    //                 size={PersonaSize.size32}
    //                 imageUrl={_getTypeImage(item.template)}
    //                 coinProps={{ styles: { initials: { borderRadius: '4px' } } }}
    //                 styles={{
    //                     root: { color: 'inherit' },
    //                     primaryText: { color: 'inherit', textTransform: 'capitalize' }
    //                 }} />
    //         </Stack>
    //     );
    // };


    // const onRenderTypeColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
    //     if (!item) return undefined;
    //     return (
    //         <Stack horizontal >
    //             <FontIcon iconName={_getTypeIcon(item.template)} className='component-type-icon' />
    //             <Text styles={{ root: { paddingLeft: '4px' } }}>{item.template.type}</Text>
    //         </Stack>
    //     )
    // };

    // const onRenderTemplateColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
    //     if (!item) return undefined;
    //     return <ComponentTemplateLink componentTemplate={item.template} />
    // };


    const onRenderCreatorColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        const creator = members?.find(m => m.user.id === item.schedule.creator);
        return (
            <UserPersona user={creator?.graphUser} size={PersonaSize.size24} />
        )
    };

    const onRenderTimeColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (!item?.schedule.utcHour || !item?.schedule.utcHour) return undefined;
        const now = new Date();
        now.setUTCHours(item.schedule.utcHour, item.schedule.utcMinute, 0, 0)
        return (<Text>{now.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit', timeZoneName: 'short' })}</Text>);
    };

    const columns: IColumn[] = [
        // { key: 'id', name: 'ID', minWidth: 220, isResizable: false, onRender: onRenderNameColumn, styles: { cellName: { paddingLeft: '5px' } } },
        { key: 'time', name: 'Time', minWidth: 120, maxWidth: 120, onRender: onRenderTimeColumn },
        { key: 'days', name: 'Days', minWidth: 300, maxWidth: 300, isResizable: false, onRender: (i: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }) => i.schedule.daysOfWeek?.join(', ') },
        // { key: 'id', name: 'ID', minWidth: 220, isResizable: false, onRender: (i: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }) => i.schedule.id, styles: { cellName: { paddingLeft: '5px' } } },
        // { key: 'type', name: 'Type', minWidth: 150, maxWidth: 150, isResizable: false, onRender: onRenderTypeColumn },
        // { key: 'link', name: 'Link', minWidth: 200, maxWidth: 200, onRender: onRenderLinkColumn },
        // { key: 'repository', name: 'Template', minWidth: 280, maxWidth: 280, onRender: onRenderTemplateColumn },
        { key: 'components', name: 'Components', minWidth: 110, maxWidth: 110, isResizable: false, onRender: (i: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }) => i.tasks?.map(t => t.component?.displayName).join(', ') },
        { key: 'creator', name: 'Creator', minWidth: 180, maxWidth: 180, onRender: onRenderCreatorColumn },
    ];


    const _applyFilter = (item: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, filter: string): boolean => {
        return filter ? JSON.stringify(item).toUpperCase().includes(filter.toUpperCase()) : true;
    };

    const _onItemInvoked = (item: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }): void => {
        // history.push(`/orgs/${orgId}/projects/${projectId}/components/${item.component.slug}`);
    };

    return (
        <ContentList
            columns={columns}
            items={items}
            applyFilter={_applyFilter}
            onItemInvoked={_onItemInvoked}
            filterPlaceholder='Filter schedules'
            buttonText='Create schedule'
            buttonIcon='Add'
            onButtonClick={() => history.push(`/orgs/${orgId}/projects/${projectId}/settings/schedules/new`)}
            noDataTitle='You do not have any schedules yet'
            noDataImage={collaboration}
            noDataDescription='Schedule component tasks'
            noDataButtonText='Create schedule'
            noDataButtonIcon='Add'
            onNoDataButtonClick={() => history.push(`/orgs/${orgId}/projects/${projectId}/settings/schedules/new`)} />
    );
}
