import React from 'react';
import { Project } from '../model';
import { Link, useHistory } from 'react-router-dom';
import { ShimmeredDetailsList, DetailsListLayoutMode } from '@fluentui/react';

export interface IProjectListProps {
    projects: Project[] | undefined,
    projectFilter?: string
    onProjectSelected?: (project: Project) => void;
}

export const ProjectList: React.FunctionComponent<IProjectListProps> = (props) => {

    const history = useHistory();

    const columns = [
        { key: 'projectName', name: 'Project Name', data: 'string', onRender: (p: Project) => (<Link onClick={() => _onLinkClicked(p)} to={'/projects/' + p.id} style={{ textDecoration: 'none' }}>{p.name}</Link>), minWidth: 100, isResizable: true },
        { key: 'projectId', name: 'ID', data: 'string', fieldName: 'id', minWidth: 240, isResizable: true },
        { key: 'projectType', name: 'Type', data: 'string', onRender: (p: Project) => p.type.id, minWidth: 160, isResizable: true },
        { key: 'projectGroup', name: 'ResourceGroup', data: 'string', onRender: (p: Project) => p.resourceGroup.name, minWidth: 220, isResizable: true },
        { key: 'projectLocation', name: 'Location', data: 'string', onRender: (p: Project) => p.resourceGroup.region, minWidth: 100, isResizable: true },
        { key: 'projectUserCount', name: 'Users', data: 'number', onRender: (p: Project) => p.users.length, minWidth: 160, isResizable: true }
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

    const items = props.projects ? props.projects.filter(_applyProjectFilter) : new Array<Project>();

    return (
        <ShimmeredDetailsList
            items={items}
            columns={columns}
            layoutMode={DetailsListLayoutMode.justified}
            enableShimmer={items.length === 0}
            selectionPreservedOnEmptyClick={true}
            ariaLabelForSelectionColumn="Toggle selection"
            ariaLabelForSelectAllCheckbox="Toggle selection for all items"
            checkButtonAriaLabel="Row checkbox"
            onItemInvoked={_onItemInvoked} />
    );
}
