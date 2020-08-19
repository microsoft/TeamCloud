// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Project } from '../model';
import { Link, useHistory } from 'react-router-dom';
import { ShimmeredDetailsList, DetailsListLayoutMode, IColumn } from '@fluentui/react';

export interface IProjectListProps {
    projects: Project[] | undefined,
    projectFilter?: string
    onProjectSelected?: (project: Project) => void;
}

export const ProjectList: React.FunctionComponent<IProjectListProps> = (props) => {

    const history = useHistory();

    const columns: IColumn[] = [
        { key: 'name', name: 'Project Name', onRender: (p: Project) => (<Link onClick={() => _onLinkClicked(p)} to={'/projects/' + p.id} style={{ textDecoration: 'none' }}>{p.name}</Link>), minWidth: 100, isResizable: true },
        { key: 'id', name: 'ID', fieldName: 'id', minWidth: 260, isResizable: true },
        { key: 'type', name: 'Type', onRender: (p: Project) => (<Link to={'/projectTypes/' + p.type.id} style={{ textDecoration: 'none' }}>{p.type.id}</Link>), minWidth: 100, isResizable: true },
        { key: 'group', name: 'ResourceGroup', onRender: (p: Project) => p.resourceGroup?.name, minWidth: 220, isResizable: true },
        { key: 'location', name: 'Location', onRender: (p: Project) => p.resourceGroup?.region, minWidth: 100, isResizable: true },
        { key: 'userCount', name: 'Users', onRender: (p: Project) => p.users.length, minWidth: 100, isResizable: true }
    ];

    const _applyProjectFilter = (project: Project): boolean => {
        return props.projectFilter ? project.name.toUpperCase().includes(props.projectFilter.toUpperCase()) : true;
    }

    const _onLinkClicked = (project: Project): void => {
        if (props.onProjectSelected)
            props.onProjectSelected(project);
    }

    const _onItemInvoked = (project: Project): void => {
        _onLinkClicked(project);
        history.push('/projects/' + project.id)
    };

    // const _onColumnHeaderClicked = (ev?: React.MouseEvent<HTMLElement>, column?: IColumn) => {
    //     console.log(column?.key);
    // }

    const items = props.projects ? props.projects.filter(_applyProjectFilter) : new Array<Project>();

    return (
        <ShimmeredDetailsList
            items={items}
            columns={columns}
            layoutMode={DetailsListLayoutMode.justified}
            enableShimmer={items.length === 0}
            // onColumnHeaderClick={_onColumnHeaderClicked}
            selectionPreservedOnEmptyClick={true}
            ariaLabelForSelectionColumn="Toggle selection"
            ariaLabelForSelectAllCheckbox="Toggle selection for all items"
            checkButtonAriaLabel="Row checkbox"
            onItemInvoked={_onItemInvoked} />
    );
}
