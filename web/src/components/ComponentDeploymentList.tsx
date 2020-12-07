// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { CheckboxVisibility, DetailsList, DetailsListLayoutMode, FontIcon, getTheme, IColumn, IDetailsRowProps, IRenderFunction, SelectionMode, Stack, Text, TextField } from '@fluentui/react';
import { ProjectContext } from '../Context';
import { ComponentDeployment } from 'teamcloud';

export interface IComponentDeploymentListProps {

}

export const ComponentDeploymentList: React.FunctionComponent<IComponentDeploymentListProps> = (props) => {

    const theme = getTheme();

    const { component, componentDeployments } = useContext(ProjectContext);

    const [deployment, setDeployment] = useState<ComponentDeployment>();

    useEffect(() => {
        if (componentDeployments && deployment === undefined) {
            console.log('setDeployment');
            setDeployment(componentDeployments[0]);
        }
    }, [deployment, componentDeployments])


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
            key: 'componentId', name: 'ComponentId', minWidth: 400, maxWidth: 400, onRender: (d: ComponentDeployment) => (
                <Stack
                    horizontal
                    verticalAlign='center'
                    horizontalAlign='space-between'
                    tokens={{ childrenGap: '20px' }}
                    styles={{ root: { padding: '5px' } }}>
                    <Text>{d.id}</Text>
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
                    items={componentDeployments ?? []}
                    columns={columns}
                    isHeaderVisible={false}
                    onRenderRow={_onRenderRow}
                    layoutMode={DetailsListLayoutMode.fixedColumns}
                    checkboxVisibility={CheckboxVisibility.hidden}
                    selectionMode={SelectionMode.none}
                    onItemInvoked={_onItemInvoked}
                    styles={{ focusZone: { minWidth: '1px' } }}
                />
            </Stack.Item>
            <Stack.Item grow={2} styles={{
                root: {
                    height: '100%',
                    // minWidth: '60%',
                    padding: '10px 40px',
                    borderRadius: theme.effects.roundedCorner4,
                    // boxShadow: theme.effects.elevation4,
                    backgroundColor: theme.palette.black
                }
            }}>
                {deployment?.output && (
                    <TextField
                        readOnly
                        multiline
                        borderless
                        resizable={false}
                        defaultValue={deployment?.output ?? undefined}
                        styles={{
                            root: {
                                color: theme.palette.white,
                                minHeight: '50%'
                            },
                            fieldGroup: {
                                backgroundColor: theme.palette.black,
                                border: 'none',
                                color: theme.palette.white,
                                height: '600px'
                            },
                            field: {
                                // backgroundColor: theme.palette.black,
                                // border: 'none',
                                color: theme.palette.white,
                            },
                            wrapper: {
                                color: theme.palette.white,
                            }

                        }} />
                )}
                {/* <Text block styles={{ root: { color: theme.palette.white } }}>
                        {deployment?.output}
                    </Text> */}

                {/* <ReactMarkdown>{template?.description ?? undefined as any}</ReactMarkdown> */}
            </Stack.Item>
        </Stack >

    );
}
