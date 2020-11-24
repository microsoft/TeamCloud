// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Project } from 'teamcloud';
import { useHistory, useParams } from 'react-router-dom';
import { DetailsListLayoutMode, IColumn, IRenderFunction, IDetailsRowProps, CheckboxVisibility, SelectionMode, Persona, PersonaSize, getTheme, DetailsList, Stack, Text } from '@fluentui/react';

export interface IProjectListProps {
    projects?: Project[];
    onProjectSelected?: (project: Project) => void;
}

export const ProjectList: React.FunctionComponent<IProjectListProps> = (props) => {

    const history = useHistory();

    let { orgId } = useParams() as { orgId: string };

    const [projectFilter, setProjectFilter] = useState<string>();

    const theme = getTheme();

    const columns: IColumn[] = [
        {
            key: 'project', name: 'Project', minWidth: 100, isResizable: false, onRender: (p: Project) => (
                <Stack tokens={{ padding: '8px' }}>
                    <Persona
                        text={p.displayName}
                        size={PersonaSize.size48}
                        coinProps={{
                            styles: {
                                initials: {
                                    borderRadius: '4px',
                                    fontSize: '20px',
                                    fontWeight: '400'
                                }
                            }
                        }}
                        styles={{
                            primaryText: {
                                fontSize: theme.fonts.xLarge.fontSize,
                                fontWeight: theme.fonts.xLarge.fontWeight
                            }
                        }} />
                </Stack>
            )
        }
    ];


    const _applyProjectFilter = (project: Project): boolean => {
        return projectFilter ? project.displayName.toUpperCase().includes(projectFilter.toUpperCase()) : true;
    }

    const _onLinkClicked = (project: Project): void => {
        if (props.onProjectSelected)
            props.onProjectSelected(project);
    }

    const _onItemInvoked = (project: Project): void => {
        if (project) {
            _onLinkClicked(project);
            history.push(`${orgId}/projects/${project.slug}`);
        } else {
            console.error('nope');
        }
    };

    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (props?: IDetailsRowProps, defaultRender?: (props?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (props) props.styles = { fields: { alignItems: 'center' }, check: { minHeight: '62px' } }
        return defaultRender ? defaultRender(props) : null;
    };


    const items = props.projects ? props.projects.filter(_applyProjectFilter) : new Array<Project>();

    if (props.projects === undefined)
        return (<></>);

    if (props.projects.length === 0)
        return (<Text styles={{ root: { width: '100%', paddingLeft: '8px' } }}>No projects</Text>)

    return (
        <DetailsList
            items={items}
            columns={columns}
            isHeaderVisible={false}
            onRenderRow={_onRenderRow}
            selectionMode={SelectionMode.none}
            layoutMode={DetailsListLayoutMode.justified}
            checkboxVisibility={CheckboxVisibility.hidden}
            selectionPreservedOnEmptyClick={true}
            onItemInvoked={_onItemInvoked}
            styles={{
                root: {
                    borderRadius: theme.effects.roundedCorner4,
                    boxShadow: theme.effects.elevation4,
                    backgroundColor: theme.palette.white
                }
            }} />
    );
}
