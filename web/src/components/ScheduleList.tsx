// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { Checkbox, IColumn, PersonaSize, Stack, Text } from '@fluentui/react';
import { useHistory, useParams } from 'react-router-dom';
import { Component, ComponentTaskTemplate, ComponentTemplate, Schedule } from 'teamcloud';
import { ContentList, UserPersona } from '.';
import { useProjectMembers, useProjectComponentTemplates, useProjectComponents, useProjectSchedules } from '../hooks';

import collaboration from '../img/MSC17_collaboration_010_noBG.png'
import { shiftToLocal } from '../model';

export interface IScheduleListProps {
    onItemInvoked?: (schedule: Schedule, component: Component) => void;
}

export const ScheduleList: React.FC<IScheduleListProps> = (props) => {

    const history = useHistory();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [items, setItems] = useState<{ schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }[]>()

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


    const onRenderTasksColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (!item?.tasks) return undefined;
        const componentIds = item.tasks.map(t => t.component?.id).filter((v, i, a) => a.indexOf(v) === i);
        const componentTexts = componentIds.map(i => (<Text key={i}>{`${components?.find(c => c.id === i)?.displayName} (${item.tasks?.filter(t => t.component?.id === i).map(t => t.taskTemplate?.displayName ?? t.taskTemplate?.typeName).join(', ')})`}</Text>));
        return (<Stack>{componentTexts}</Stack>)
    };

    const onRenderLastRunColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (!item?.schedule.lastRun) return undefined;
        return (<Text>{item.schedule.lastRun.toDateTimeDisplayString(true)}</Text>)
    };

    const onRenderCreatorColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        const creator = members?.find(m => m.user.id === item.schedule.creator);
        return (<UserPersona principal={creator?.graphPrincipal} size={PersonaSize.size24} />)
    };

    const onRenderDaysColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (item?.schedule.utcHour === undefined || item?.schedule.utcHour === undefined || !item.schedule.daysOfWeek) return undefined;

        const refDate = new Date();
        refDate.setUTCHours(item.schedule.utcHour, item.schedule.utcMinute, 0, 0);

        const days = shiftToLocal(item.schedule.daysOfWeek!, refDate);

        return (<Text>{days.names.join(', ')}</Text>);
    };

    const onRenderTimeColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (item?.schedule.utcHour === undefined || item?.schedule.utcHour === undefined) return undefined;
        const now = new Date();
        now.setUTCHours(item.schedule.utcHour, item.schedule.utcMinute, 0, 0);
        return (<Text>{now.toTimeDisplayString(true)}</Text>);
    };

    const onRenderEnabledColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return (<Checkbox styles={{ root: { paddingLeft: '4px' } }} checked={item.schedule.enabled} disabled />);
    };

    const onRenderRecurringColumn = (item?: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return (<Checkbox styles={{ root: { paddingLeft: '2px' } }} checked={item.schedule.recurring} disabled />);
    };

    const columns: IColumn[] = [
        { key: 'enabled', name: 'Enabled', minWidth: 80, maxWidth: 80, onRender: onRenderEnabledColumn },
        { key: 'time', name: 'Time', minWidth: 120, maxWidth: 120, onRender: onRenderTimeColumn },
        { key: 'days', name: 'Days', minWidth: 400, onRender: onRenderDaysColumn },
        { key: 'recurring', name: 'Recurring', minWidth: 90, maxWidth: 90, onRender: onRenderRecurringColumn },
        { key: 'tasks', name: 'Component Tasks', minWidth: 300, onRender: onRenderTasksColumn },
        { key: 'lastRun', name: 'Last Run', minWidth: 220, maxWidth: 220, onRender: onRenderLastRunColumn },
        { key: 'creator', name: 'Creator', minWidth: 180, maxWidth: 180, onRender: onRenderCreatorColumn },
    ];


    const _onItemInvoked = (item: { schedule: Schedule, tasks?: { component?: Component, template?: ComponentTemplate, taskTemplate?: ComponentTaskTemplate }[] }): void => {
        history.push(`/orgs/${orgId}/projects/${projectId}/settings/schedules/${item.schedule.id}`);
    };

    return (
        <ContentList
            columns={columns}
            items={items}
            noCheck
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
