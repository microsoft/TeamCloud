// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { ICommandBarItemProps, SearchBox, Stack, IBreadcrumbItem } from '@fluentui/react';
import { SubheaderBar, ProviderList, ProviderPanel } from '../components';
import { Provider, User } from 'teamcloud'
import { api } from '../API';

export interface IProvidersViewProps {
    user?: User;
}

export const ProvidersView: React.FunctionComponent<IProvidersViewProps> = (props) => {

    const [providers, setProviders] = useState<Provider[]>();
    const [providerFilter, setProviderFilter] = useState<string>();
    const [selectedProvider, setSelectedProvider] = useState<Provider>();
    const [detailsPanelOpen, setDetailsPanelOpen] = useState(false);

    useEffect(() => {
        if (providers === undefined) {
            const _setProviders = async () => {
                const result = await api.getProviders();
                setProviders(result.data);
            };
            _setProviders();
        }
    }, [providers]);

    const _refresh = async () => {
        let result = await api.getProviders();
        setProviders(result.data);
    };


    const _commandBarItems = (): ICommandBarItemProps[] => [
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'refresh' }, onClick: () => { _refresh() } },
        // { key: 'newProjectType', text: 'New project type', iconProps: { iconName: 'NewTeamProject' }, onClick: () => { setNewProjectTypePanelOpen(true) }, disabled: !_userCanCreateProjectTypes() },
    ];

    const _centerCommandBarItems: ICommandBarItemProps[] = [
        { key: 'search', onRender: () => <SearchBox className="searchBox" iconProps={{ iconName: 'Filter' }} placeholder="Filter" onChange={(_, filter) => setProviderFilter(filter)} /> }
    ];

    const _breadcrumbs: IBreadcrumbItem[] = [
        { text: '', key: 'root', href: '/', isCurrentItem: true }
    ];

    const _onProviderSelected = (provider: Provider) => {
        setSelectedProvider(provider)
        setDetailsPanelOpen(true)
    };

    const _onDetailsPanelClose = () => {
        setSelectedProvider(undefined)
        setDetailsPanelOpen(false)
    };

    return (
        <>
            <Stack>
                <SubheaderBar
                    breadcrumbs={_breadcrumbs}
                    commandBarItems={_commandBarItems()}
                    centerCommandBarItems={_centerCommandBarItems}
                    commandBarWidth='90px' />
                <ProviderList
                    providers={providers}
                    providerFilter={providerFilter}
                    onProviderSelected={_onProviderSelected} />
            </Stack>
            <ProviderPanel
                provider={selectedProvider}
                panelIsOpen={detailsPanelOpen}
                onPanelClose={_onDetailsPanelClose} />
        </>
    );
}
