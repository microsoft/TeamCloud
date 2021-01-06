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
    const { component, componentDeployments, onComponentSelected } = useContext(ProjectContext);

    const [deployment, setDeployment] = useState<ComponentDeployment>();
    const [deployments, setDeployments] = useState<ComponentDeployment[]>();
    const [isPolling, setIsPolling] = useState(true);

    useEffect(() => {
        if (componentDeployments && deployment === undefined) {
            console.log('+ setDeployment');
            setDeployment(componentDeployments.splice(-1)[0]);
        }
    }, [deployment, componentDeployments])


    useEffect(() => {
        if (componentDeployments) {
            console.log('+ setDeployments');
            setDeployments(deployment ? [deployment, ...componentDeployments.filter(d => d.id !== deployment.id)] : componentDeployments);
        }
    }, [deployment, componentDeployments]);


    useEffect(() => {
        const poll = (deployments ?? []).some((d) => d.resourceState?.toLowerCase() !== 'succeeded' && d.resourceState?.toLowerCase() !== 'failed');
        if (isPolling !== poll) {
            console.log(`+ setPollDeployment (${poll})`);
            setIsPolling(poll);
        }
    }, [deployments, isPolling])

    useInterval(async () => {

        if (org && component && component.resourceState?.toLowerCase() !== 'succeeded' && component.resourceState?.toLowerCase() !== 'failed') {
            console.log('- refreshComponent');
            const result = await api.getProjectComponent(component.id, component.organization, component.projectId);
            if (result.data) {
                onComponentSelected(result.data);
            } else {
                console.error(result);
            }
            console.log('+ refreshComponent');
        }

        if (org && deployments) {

            let _deployments = await Promise.all(deployments
                .map(async d => {
                    if (d.finished === undefined && d.exitCode === undefined) {
                        console.log(`- refreshDeployment (${d.id})`);
                        const result = await api.getProjectDeployment(d.id, org.id, d.projectId, d.componentId);
                        if (result.data) {
                            d = result.data;
                        } else {
                            console.error(result);
                        }
                        console.log(`+ refreshDeployment (${d.id})`);
                    }
                    return d;
                }));

            setDeployments(_deployments);

            if (deployment && _deployments && _deployments.some(d => d.id === deployment.id)) {
                setDeployment(_deployments.find(d => d.id === deployment.id));
            }
        }

    }, isPolling ? 5000 : undefined);

    const [dots, setDots] = useState('');

    useInterval(() => {
        const d = dots.length < 3 ? `${dots}.` : '';
        setDots(d);
    }, isPolling ? 1000 : undefined);

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
        }
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
    };

    const _getDeploymentName = (d?: ComponentDeployment) => d ? `${d.typeName || d.type}: ${d.id}` : undefined;

    const _getDeploymentStatus = (d?: ComponentDeployment) => {
        if (d?.resourceState) {
            if (d.resourceState.toLowerCase() === 'succeeded' || d.resourceState.toLowerCase() === 'failed') {
                return d.finished ? `${d.resourceState} ${d.finished.toLocaleString()}` : d.resourceState;
            } else {
                return `${d.resourceState} ${dots}`;
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
                    items={(deployments ?? []).sort((a, b) => (b.created?.valueOf() ?? 0) - (a.created?.valueOf() ?? 0))}
                    columns={columns}
                    isHeaderVisible={false}
                    onRenderRow={_onRenderRow}
                    layoutMode={DetailsListLayoutMode.fixedColumns}
                    checkboxVisibility={CheckboxVisibility.hidden}
                    selectionMode={SelectionMode.single}
                    onActiveItemChanged={_onItemInvoked}
                    styles={{ focusZone: { minWidth: '1px' }, root: { minWidth: '460px', boxShadow: theme.effects.elevation8 } }}
                />
            </Stack.Item>
            <Stack.Item grow={2}>
                <DeploymentConsole deployment={deployment} isPolling={isPolling} />
            </Stack.Item>
        </Stack >

    );
}
