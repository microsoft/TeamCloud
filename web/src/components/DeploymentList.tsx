// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { CheckboxVisibility, DetailsList, DetailsListLayoutMode, FontIcon, getTheme, IColumn, IDetailsRowProps, IRenderFunction, SelectionMode, Stack, Text } from '@fluentui/react';
import { OrgContext, ProjectContext } from '../Context';
import { ComponentDeployment } from 'teamcloud';
import { useInterval } from '../Hooks';
import { api } from '../API';
import { DeploymentConsole } from './DeploymentConsole';

export interface IDeploymentListProps {

}

export const DeploymentList: React.FunctionComponent<IDeploymentListProps> = (props) => {

    const theme = getTheme();

    const { org } = useContext(OrgContext);
    const { component, componentDeployments } = useContext(ProjectContext);

    const [deployment, setDeployment] = useState<ComponentDeployment>();
    const [deployments, setDeployments] = useState<ComponentDeployment[]>();
    const [isPolling, setIsPolling] = useState(true);
    // const [output, setOutput] = useState<string>(' ');

    useEffect(() => {
        if (componentDeployments && deployment === undefined) {
            console.log('+ setDeployment');
            setDeployment(componentDeployments[0]);
        }
    }, [deployment, componentDeployments])


    useEffect(() => {
        if (componentDeployments) {
            console.log('+ setDeployments');
            setDeployments(deployment ? [deployment, ...componentDeployments.filter(d => d.id !== deployment.id)] : componentDeployments);
        }
    }, [deployment, componentDeployments]);


    useEffect(() => {
        const poll = (
            org !== undefined
            && component !== undefined
            && component.resourceState?.toLowerCase() !== 'succeeded'
            && component.resourceState?.toLowerCase() !== 'failed'
            && deployment !== undefined
            && deployment.finished === undefined
            && deployment.exitCode === undefined);
        if (isPolling !== poll) {
            console.log(`+ setPollDeployment (${poll})`);
            setIsPolling(poll);
        }
    }, [org, deployment, component, isPolling])


    // useEffect(() => {
    //     if (deployment?.output) {
    //         console.log('+ setOutput');
    //         setOutput(deployment.output);
    //     }
    // }, [deployment])


    useInterval(async () => {
        console.log('...hello...');
        if (org && deployment && deployment.finished === undefined && deployment.exitCode === undefined) {
            console.log('- refreshDeployment');
            const result = await api.getProjectDeployment(deployment.id, org.id, deployment.projectId, deployment.componentId);
            if (result.data) {
                setDeployment(result.data);
            } else {
                console.error(result);
            }
            console.log('+ refreshDeployment');
        }
    }, isPolling ? 3000 : undefined);

    const [dots, setDots] = useState('');

    useInterval(() => {
        const d = dots.length < 3 ? `${dots}.` : '';
        setDots(d);
    }, isPolling ? 1000 : undefined);

    // useEffect(() => {
    //     if (deployment?.started) {
    //         console.log(`toDateString ${deployment.started.toDateString()}`);
    //         console.log(`toTimeString ${deployment.started.toTimeString()}`);
    //         console.log(`toLocaleDateString ${deployment.started.toLocaleDateString()}`);
    //         console.log(`toLocaleString ${deployment.started.toLocaleString()}`);
    //         console.log(`toLocaleTimeString ${deployment.started.toLocaleTimeString()}`);
    //         console.log(`toString ${deployment.started.toString()}`);
    //         console.log(`toISOString ${deployment.started.toISOString()}`);
    //         console.log(`toUTCString ${deployment.started.toUTCString()}`);
    //     }
    // }, [deployment]);


    const _getStateIcon = (deployment?: ComponentDeployment) => {
        if (deployment?.resourceState)
            switch (deployment.resourceState) {
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
            key: 'componentId', name: 'ComponentId', minWidth: 440, maxWidth: 440, onRender: (d: ComponentDeployment) => (
                <Stack
                    horizontal
                    verticalAlign='center'
                    horizontalAlign='space-between'
                    tokens={{ childrenGap: '20px' }}
                    styles={{ root: { padding: '5px' } }}>
                    <Stack tokens={{ childrenGap: '6px' }}>
                        <Text styles={{ root: { color: theme.palette.neutralPrimary } }} variant='medium'>{_getDeploymentName(d)}</Text>
                        <Text styles={{ root: { color: theme.palette.neutralSecondary } }} variant='small'>{_getDeploymentStatus(d)}</Text>
                    </Stack>
                    <FontIcon iconName={_getStateIcon(d)} className={`deployment-state-icon-${d.resourceState?.toLowerCase() ?? 'pending'}`} />
                </Stack>
            )
        },
        // { key: 'projectId', name: 'ProjectId', minWidth: 200, fieldName: 'projectId' },
        // { key: 'started', name: 'Started', minWidth: 200, fieldName: 'started' },
        // { key: 'finished', name: 'Finished', minWidth: 200, fieldName: 'finished' },
        // { key: 'output', name: 'Output', minWidth: 200, fieldName: 'output' },
        // { key: 'resourceId', name: 'ResourceId', minWidth: 200, fieldName: 'resourceId' },
        // { key: 'resourceState', name: 'ResourceState', minWidth: 100, fieldName: 'resourceState' },
        // { key: 'exitCode', name: 'ExitCode', minWidth: 100, fieldName: 'exitCode' },
        // { key: 'id', name: 'Id', minWidth: 200, fieldName: 'id' },
    ];

    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (rowProps?: IDetailsRowProps, defaultRender?: (rowProps?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (rowProps) rowProps.styles = {
            // root: { borderBottom: (props.noHeader ?? false) && items.length === 1 ? 0 : undefined },
            fields: { alignItems: 'center' }, check: { minHeight: '62px' }, cell: { fontSize: '14px' }
        }
        return defaultRender ? defaultRender(rowProps) : null;
    };

    const _onItemInvoked = (item: ComponentDeployment): void => {
        console.log(item);
        setDeployment(item);
        // onComponentSelected(item.component);
        // history.push(`/orgs/${orgId}/projects/${project?.slug ?? projectId}/components/${item.component.slug}`);
    };

    const _getDeploymentName = (d?: ComponentDeployment) => d ? `Deployment: ${d.id}` : undefined;

    const _getDeploymentStatus = (d?: ComponentDeployment) => {
        if (d?.resourceState) {
            if (d.resourceState.toLowerCase() === 'succeeded' || d.resourceState.toLowerCase() === 'failed') {
                return d.finished ? `${d.resourceState} ${d.finished.toLocaleString()}` : d.resourceState;
            } else {
                return `${d.resourceState}${dots}`;
            }
        } else if (d?.started) {
            return `Started ${d.started.toLocaleString()}`;
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
                    items={deployments ?? []}
                    columns={columns}
                    isHeaderVisible={false}
                    onRenderRow={_onRenderRow}
                    layoutMode={DetailsListLayoutMode.fixedColumns}
                    checkboxVisibility={CheckboxVisibility.hidden}
                    selectionMode={SelectionMode.none}
                    onItemInvoked={_onItemInvoked}
                    styles={{ focusZone: { minWidth: '1px' }, root: { minWidth: '460px', boxShadow: theme.effects.elevation8 } }}
                />
            </Stack.Item>
            <Stack.Item grow={2}>
                <DeploymentConsole deployment={deployment} isPolling={isPolling} />
            </Stack.Item>
        </Stack >

    );
}
