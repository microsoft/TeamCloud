// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { ProjectType } from '../model';
import { Link, ShimmeredDetailsList, DetailsListLayoutMode, IColumn, IRenderFunction, IDetailsRowProps, SelectionMode, CheckboxVisibility } from '@fluentui/react';

export interface IProjectTypeListProps {
    projectTypes?: ProjectType[];
    projectTypeFilter?: string;
    onProjectTypeSelected?: (projectType: ProjectType) => void;
}

export const ProjectTypeList: React.FunctionComponent<IProjectTypeListProps> = (props) => {

    const columns: IColumn[] = [
        { key: 'id', name: 'ID', onRender: (p: ProjectType) => (<Link onClick={() => _onLinkClicked(p)} style={{ textDecoration: 'none' }}>{p.id}</Link>), minWidth: 200, isResizable: true },
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
    };

    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (props?: IDetailsRowProps, defaultRender?: (props?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (props) props.styles = { fields: { alignItems: 'center' }, check: { minHeight: '62px' } }
        return defaultRender ? defaultRender(props) : null;
    };

    // const _onColumnHeaderClicked = (ev?: React.MouseEvent<HTMLElement>, column?: IColumn) => {
    //     console.log(column?.key);
    // }

    const items = props.projectTypes ? props.projectTypes.filter(_applyProjectTypeFilter) : new Array<ProjectType>();

    return (
        <ShimmeredDetailsList
            items={items}
            columns={columns}
            onRenderRow={_onRenderRow}
            enableShimmer={items.length === 0}
            selectionMode={SelectionMode.none}
            layoutMode={DetailsListLayoutMode.justified}
            checkboxVisibility={CheckboxVisibility.hidden}
            cellStyleProps={{ cellLeftPadding: 46, cellRightPadding: 20, cellExtraRightPadding: 0 }}

            // onColumnHeaderClick={_onColumnHeaderClicked}
            selectionPreservedOnEmptyClick={true}
            onItemInvoked={_onItemInvoked} />
    );
}
