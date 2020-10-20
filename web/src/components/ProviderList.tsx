// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Provider } from 'teamcloud';
import { Link, ShimmeredDetailsList, DetailsListLayoutMode, IColumn, IRenderFunction, IDetailsRowProps, SelectionMode, CheckboxVisibility } from '@fluentui/react';

export interface IProviderListProps {
    providers: Provider[] | undefined,
    providerFilter?: string
    onProviderSelected?: (provider: Provider) => void;
}

export const ProviderList: React.FunctionComponent<IProviderListProps> = (props) => {

    const columns: IColumn[] = props.providers ? [
        { key: 'id', name: 'ID', onRender: (p: Provider) => (<Link onClick={() => _onLinkClicked(p)} style={{ textDecoration: 'none' }}>{p.id}</Link>), minWidth: 200, isResizable: true },
        { key: 'url', name: 'Url', fieldName: 'url', minWidth: 340, isResizable: true },
        { key: 'group', name: 'ResourceGroup', onRender: (p: Provider) => p.resourceGroup?.name, minWidth: 200, isResizable: true },
        { key: 'registered', name: 'Registered', onRender: (p: Provider) => p.registered?.toDateString(), minWidth: 200, isResizable: true },
        { key: 'mode', name: 'Mode', fieldName: 'commandMode', minWidth: 100, isResizable: true }
    ] : [];

    const _applyProviderFilter = (provider: Provider): boolean => {
        return props.providerFilter ? provider.id.toUpperCase().includes(props.providerFilter.toUpperCase()) : true;
    }

    const _onLinkClicked = (provider: Provider): void => {
        if (props.onProviderSelected)
            props.onProviderSelected(provider);
    }

    const _onItemInvoked = (provider: Provider): void => {
        _onLinkClicked(provider);
    };

    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (props?: IDetailsRowProps, defaultRender?: (props?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (props) props.styles = { fields: { alignItems: 'center' }, check: { minHeight: '62px' } }
        return defaultRender ? defaultRender(props) : null;
    };

    // const _onColumnHeaderClicked = (ev?: React.MouseEvent<HTMLElement>, column?: IColumn) => {
    //     console.log(column?.key);
    // }

    const items = props.providers ? props.providers.filter(_applyProviderFilter) : new Array<Provider>();

    return (
        <ShimmeredDetailsList
            items={items}
            columns={columns}
            onRenderRow={_onRenderRow}
            enableShimmer={props.providers === undefined}
            selectionMode={SelectionMode.none}
            layoutMode={DetailsListLayoutMode.justified}
            checkboxVisibility={CheckboxVisibility.hidden}
            cellStyleProps={{ cellLeftPadding: 46, cellRightPadding: 20, cellExtraRightPadding: 0 }}
            // onColumnHeaderClick={_onColumnHeaderClicked}
            selectionPreservedOnEmptyClick={true}
            onItemInvoked={_onItemInvoked} />
    );
}
