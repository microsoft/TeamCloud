// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Provider } from '../model';
import { Link, useHistory } from 'react-router-dom';
import { ShimmeredDetailsList, DetailsListLayoutMode, IColumn } from '@fluentui/react';

export interface IProviderListProps {
    providers: Provider[] | undefined,
    providerFilter?: string
    onProviderSelected?: (provider: Provider) => void;
}

export const ProviderList: React.FunctionComponent<IProviderListProps> = (props) => {

    const history = useHistory();

    const columns: IColumn[] = [
        { key: 'id', name: 'ID', onRender: (p: Provider) => (<Link onClick={() => _onLinkClicked(p)} to={'/providers/' + p.id} style={{ textDecoration: 'none' }}>{p.id}</Link>), minWidth: 200, isResizable: true },
        { key: 'url', name: 'Url', fieldName: 'url', minWidth: 340, isResizable: true },
        { key: 'group', name: 'ResourceGroup', onRender: (p: Provider) => p.resourceGroup?.name, minWidth: 200, isResizable: true },
        { key: 'registered', name: 'Registered', fieldName: 'registered', minWidth: 200, isResizable: true },
        { key: 'mode', name: 'Mode', fieldName: 'commandMode', minWidth: 100, isResizable: true }
    ];

    const _applyProviderFilter = (provider: Provider): boolean => {
        return props.providerFilter ? provider.id.toUpperCase().includes(props.providerFilter.toUpperCase()) : true;
    }

    const _onLinkClicked = (provider: Provider): void => {
        if (props.onProviderSelected)
            props.onProviderSelected(provider);
    }

    const _onItemInvoked = (provider: Provider): void => {
        _onLinkClicked(provider);
        history.push('/providers/' + provider.id)
    };

    // const _onColumnHeaderClicked = (ev?: React.MouseEvent<HTMLElement>, column?: IColumn) => {
    //     console.log(column?.key);
    // }

    const items = props.providers ? props.providers.filter(_applyProviderFilter) : new Array<Provider>();

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
