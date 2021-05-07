// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { CheckboxVisibility, DetailsList, DetailsListLayoutMode, FontIcon, getTheme, IColumn, IDetailsRowProps, IRenderFunction, SelectionMode, Stack, Text } from '@fluentui/react';
import { ComponentTask } from 'teamcloud';
import { CalloutLabel, ComponentTaskConsole } from '.';
import { useOrg, useProject, useProjectComponent, useProjectComponentTasks, useProjectComponentTemplates, useProjectComponentTask } from '../hooks';

export interface IComponentTaskListProps { }

export const ComponentTaskList: React.FunctionComponent<IComponentTaskListProps> = (props) => {

    const theme = getTheme();
    const history = useHistory();

    const { orgId, projectId, itemId, subitemId } = useParams() as { orgId: string, projectId: string, itemId: string, subitemId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    const { data: component } = useProjectComponent();
    const { data: templates } = useProjectComponentTemplates();
    const { data: componentTask } = useProjectComponentTask();
    const { data: componentTasks } = useProjectComponentTasks();


    useEffect(() => {
        if (!subitemId && org && project && component && componentTasks && componentTasks.length > 0) {
            history.push(`/orgs/${org.slug}/projects/${project.slug}/components/${component.slug}/tasks/${componentTasks[0].id}`);
        }
    }, [org, project, component, componentTasks, componentTask, subitemId, history]);

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

    const _onActiveItemChanged = (item?: ComponentTask, index?: number | undefined) => {
        history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug ?? itemId}/tasks/${item?.id}`);
    };

    const columns: IColumn[] = [
        {
            key: 'componentId', name: 'ComponentId', minWidth: 440, maxWidth: 440, onRender: (t: ComponentTask) => (
                <Stack
                    horizontal
                    verticalAlign='center'
                    horizontalAlign='space-between'
                    tokens={{ childrenGap: '24px' }}
                    styles={{ root: { padding: '5px' } }}>
                    <Stack grow tokens={{ childrenGap: '6px' }}>
                        <Stack.Item>
                            <Text styles={{ root: { color: theme.palette.neutralPrimary } }} variant='medium'>{_getTaskName(t)}</Text>
                        </Stack.Item>
                        <Stack.Item>
                            <Text styles={{ root: { color: theme.palette.neutralSecondary } }} variant='small'>{_getTaskStatus(t)}</Text>
                        </Stack.Item>
                    </Stack>
                    {(t.scheduleId ?? false) && (<Stack.Item><FontIcon iconName='ScheduleEventAction' className='component-task-icon' /></Stack.Item>)}
                    <Stack.Item>
                        <FontIcon iconName={_getStateIcon(t)} className={`deployment-state-icon-${t.resourceState?.toLowerCase() ?? 'pending'}`} />
                    </Stack.Item>
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
                return t.resourceState;
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
                    items={(componentTasks ?? []).sort((a, b) => (b.created?.valueOf() ?? 0) - (a.created?.valueOf() ?? 0))}
                    columns={columns}
                    isHeaderVisible={false}
                    onRenderRow={_onRenderRow}
                    layoutMode={DetailsListLayoutMode.fixedColumns}
                    checkboxVisibility={CheckboxVisibility.hidden}
                    selectionMode={SelectionMode.single}
                    onActiveItemChanged={_onActiveItemChanged}
                    styles={{ focusZone: { minWidth: '1px' }, root: { minWidth: '460px', boxShadow: theme.effects.elevation8 } }}
                />
            </Stack.Item>
            <Stack.Item grow={2}>
                <ComponentTaskConsole task={componentTask} />
            </Stack.Item>
        </Stack >

    );
}
