// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { CheckboxVisibility, DetailsList, DetailsListLayoutMode, FontIcon, getTheme, IColumn, IDetailsRowProps, IRenderFunction, SelectionMode, Stack, Text } from '@fluentui/react';
import { OrgContext, ProjectContext } from '../Context';
import { ComponentTask } from 'teamcloud';
import { useInterval } from '../Hooks';
import { api } from '../API';
import { ComponentTaskConsole } from './ComponentTaskConsole';
import { stringify } from 'querystring';

export interface IComponentTaskListProps {

}

export const ComponentTaskList: React.FunctionComponent<IComponentTaskListProps> = (props) => {

    const theme = getTheme();

    const { org } = useContext(OrgContext);
    const { component, componentTasks, templates, onComponentSelected } = useContext(ProjectContext);

    const [task, setTask] = useState<ComponentTask>();
    const [tasks, setTasks] = useState<ComponentTask[]>();
    const [isPolling, setIsPolling] = useState(true);

    useEffect(() => {
        if (componentTasks && task === undefined) {
            console.log('+ setTask');
            setTask(componentTasks.splice(-1)[0]);
        }
    }, [task, componentTasks])


    useEffect(() => {
        if (componentTasks) {
            console.log('+ setTasks');
            setTasks(task ? [task, ...componentTasks.filter(d => d.id !== task.id)] : componentTasks);
        }
    }, [task, componentTasks]);


    useEffect(() => {
        const poll = (tasks ?? []).some((d) => d.resourceState?.toLowerCase() !== 'succeeded' && d.resourceState?.toLowerCase() !== 'failed');
        if (isPolling !== poll) {
            console.log(`+ setPollTask (${poll})`);
            setIsPolling(poll);
        }
    }, [tasks, isPolling])

    useInterval(async () => {

        if (org && component && component.resourceState?.toLowerCase() !== 'succeeded' && component.resourceState?.toLowerCase() !== 'failed') {
            console.log('- refreshComponent');
            const result = await api.getComponent(component.id, component.organization, component.projectId);
            if (result.data) {
                onComponentSelected(result.data);
            } else {
                console.error(result);
            }
            console.log('+ refreshComponent');
        }

        if (org && tasks) {

            let _tasks = await Promise.all(tasks
                .map(async t => {
                    if (t.finished === undefined && t.exitCode === undefined) {
                        console.log(`- refreshTask (${t.id})`);
                        const result = await api.getComponentTask(t.id, org.id, t.projectId, t.componentId);
                        if (result.data) {
                            t = result.data;
                        } else {
                            console.error(result);
                        }
                        console.log(`+ refreshTask (${t.id})`);
                    }
                    return t;
                }));

            setTasks(_tasks);

            if (task && _tasks && _tasks.some(t => t.id === task.id)) {
                setTask(_tasks.find(t => t.id === task.id));
            }
        }

    }, isPolling ? 5000 : undefined);

    const [dots, setDots] = useState('');

    useInterval(() => {
        const d = dots.length < 3 ? `${dots}.` : '';
        setDots(d);
    }, isPolling ? 1000 : undefined);

    const _getStateIcon = (task?: ComponentTask) => {
        if (task?.resourceState)
            switch (task.resourceState) {
                case 'Pending': return 'ProgressLoopOuter'; // UnknownSolid, AwayStatus, DRM, Blocked2
                case 'Initializing': return 'Running'; // Running, Rocket
                case 'Provisioning': return 'Rocket'; // Processing,
                case 'Succeeded': return 'CompletedSolid'; // BoxCheckmarkSolid, CheckboxComposite, Accept, CompletedSolid
                case 'Failed': return 'StatusErrorFull'; // BoxMultiplySolid, Error, ErrorBadge StatusErrorFull, IncidentTriangle
                default: return undefined;
            }
    };

    const columns: IColumn[] = [
        {
            key: 'componentId', name: 'ComponentId', minWidth: 440, maxWidth: 440, onRender: (t: ComponentTask) => (
                <Stack
                    horizontal
                    verticalAlign='center'
                    horizontalAlign='space-between'
                    tokens={{ childrenGap: '20px' }}
                    styles={{ root: { padding: '5px' } }}>
                    <Stack tokens={{ childrenGap: '6px' }}>
                        <Text styles={{ root: { color: theme.palette.neutralPrimary } }} variant='medium'>{_getTaskName(t)}</Text>
                        <Text styles={{ root: { color: theme.palette.neutralSecondary } }} variant='small'>{_getTaskStatus(t)}</Text>
                    </Stack>
                    <FontIcon iconName={_getStateIcon(t)} className={`deployment-state-icon-${t.resourceState?.toLowerCase() ?? 'pending'}`} />
                </Stack>
            )
        }
    ];

    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (rowProps?: IDetailsRowProps, defaultRender?: (rowProps?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (rowProps) rowProps.styles = {
            // root: { borderBottom: (props.noHeader ?? false) && items.length === 1 ? 0 : undefined },
            fields: { alignItems: 'center' }, check: { minHeight: '62px' }, cell: { fontSize: '14px' }
        }
        return defaultRender ? defaultRender(rowProps) : null;
    };

    const _onSelectTask = async (item: ComponentTask): Promise<void> => {
        if (item.output === undefined) {
            item = await _expandTask(item);
        }
        console.log(item);        
        setTask(item);
    };

    const _expandTask = async (task: ComponentTask): Promise<ComponentTask> => {
        console.log(`- expandTask (${task.id})`);
        const result = await api.getComponentTask(task.id, task.organization, task.projectId, task.componentId);
        if (result.data) {
            task = result.data;
        } else {
            console.error(result);
        }
        console.log(`+ expandTask (${task.id})`);
        return task;
    }

    const _getTaskName = (task?: ComponentTask) => {
        if (task) {
            let componentTemplate = templates?.find(t => t.id === component?.templateId);
            let taskTemplate = componentTemplate?.tasks?.find(t => t.id === task.typeName);
            return `${taskTemplate?.displayName ?? task.typeName ?? task.type}: ${task.id}`;
        }
    }

    const _getTaskStatus = (t?: ComponentTask) => {
        if (t?.resourceState) {
            if (t.resourceState.toLowerCase() === 'succeeded' || t.resourceState.toLowerCase() === 'failed') {
                return t.finished ? `${t.resourceState} ${t.finished.toLocaleString()}` : t.resourceState;
            } else {
                return `${t.resourceState} ${dots}`;
            }
        } else if (t?.started) {
            return `Started ${t.started.toLocaleString()}`;
        }
        return undefined;
    };


    return (
        <Stack
            horizontal
            verticalFill
            // horizontalAlign='stretch'
            tokens={{ childrenGap: '20px' }}
            styles={{ root: { height: '100%', padding: '24px 8px' } }}>
            <Stack.Item styles={{
                root: {
                    // minWidth: '40%'
                }
            }}>
                <DetailsList
                    items={(tasks ?? []).sort((a, b) => (b.created?.valueOf() ?? 0) - (a.created?.valueOf() ?? 0))}
                    columns={columns}
                    isHeaderVisible={false}
                    onRenderRow={_onRenderRow}
                    layoutMode={DetailsListLayoutMode.fixedColumns}
                    checkboxVisibility={CheckboxVisibility.hidden}
                    selectionMode={SelectionMode.single}
                    onActiveItemChanged={_onSelectTask}
                    styles={{ focusZone: { minWidth: '1px' }, root: { minWidth: '460px', boxShadow: theme.effects.elevation8 } }}
                />
            </Stack.Item>
            <Stack.Item grow={2}>
                <ComponentTaskConsole task={task} isPolling={isPolling} />
            </Stack.Item>
        </Stack >

    );
}
