// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { ProjectType } from '../model';
import { Link, useHistory } from 'react-router-dom';
import { ShimmeredDetailsList, DetailsListLayoutMode, IColumn } from '@fluentui/react';

export interface IProjectTypeListProps {
    projectTypes: ProjectType[] | undefined,
    projectTypeFilter?: string
    onProjectTypeSelected?: (projectType: ProjectType) => void;
}

export const ProjectTypeList: React.FunctionComponent<IProjectTypeListProps> = (props) => {

    const history = useHistory();

    const columns: IColumn[] = [
        // { key: 'projectName', name: 'Project T', onRender: (p: Project) => (<Link onClick={() => _onLinkClicked(p)} to={'/projects/' + p.id} style={{ textDecoration: 'none' }}>{p.name}</Link>), minWidth: 100, isResizable: true },
        { key: 'id', name: 'ID', fieldName: 'id', minWidth: 100, isResizable: true },
        { key: 'default', name: 'Default', onRender: (t: ProjectType) => t.isDefault ? 'Yes' : 'No', minWidth: 100, isResizable: true },
        { key: 'location', name: 'Location', fieldName: 'region', minWidth: 100, isResizable: true },
        { key: 'providers', name: 'Providers', onRender: (t: ProjectType) => t.providers.map(p => p.id).join(', '), minWidth: 300, isResizable: true },
        { key: 'subscriptions', name: 'Subscriptions', onRender: (t: ProjectType) => t.subscriptions.join(', '), minWidth: 300, isResizable: true }
    ];

    const _applyProjectTypeFilter = (projectType: ProjectType): boolean => {
        return props.projectTypeFilter ? projectType.id.toUpperCase().includes(props.projectTypeFilter.toUpperCase()) : true;
    }

    const _onLinkClicked = (projectType: ProjectType): void => {
        if (props.onProjectTypeSelected)
            props.onProjectTypeSelected(projectType);
    }

    const _onItemInvoked = (projectType: ProjectType): void => {
        _onLinkClicked(projectType);
        history.push('/projectTypes/' + projectType.id)
    };

    // const _onColumnHeaderClicked = (ev?: React.MouseEvent<HTMLElement>, column?: IColumn) => {
    //     console.log(column?.key);
    // }

    const items = props.projectTypes ? props.projectTypes.filter(_applyProjectTypeFilter) : new Array<ProjectType>();

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
