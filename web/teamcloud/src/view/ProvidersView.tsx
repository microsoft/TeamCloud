// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from "react";
import { ICommandBarItemProps, SearchBox, Stack, IBreadcrumbItem } from '@fluentui/react';
import { getProviders } from '../API'
import { Project, DataResult, Provider, User, TeamCloudUserRole } from '../model'
import { SubheaderBar, ProviderList } from "../components";

export interface IProvidersViewProps {
    user?: User;
    onProjectSelected?: (project: Project) => void;
}

export const ProvidersView: React.FunctionComponent<IProvidersViewProps> = (props) => {

    const [providers, setProviders] = useState<Provider[]>();
    const [providerFilter, setProviderFilter] = useState<string>();

    useEffect(() => {
        if (providers === undefined) {
            const _setProviders = async () => {
                const result = await getProviders();
                const data = (result as DataResult<Provider[]>).data;
                setProviders(data);
            };
            _setProviders();
        }
    }, [providers]);

    const _refresh = async () => {
        let result = await getProviders();
        let data = (result as DataResult<Provider[]>).data;
        setProviders(data);
    }


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

    // const _onRenderNewProjectTypeFormFooterContent = () => (
    //     <div>
    //         <PrimaryButton disabled={!newProjectFormEnabled || !(newProjectName && newProjectType)} onClick={() => _onCreateNewProjectType()} styles={{ root: { marginRight: 8 } }}>
    //             Create project type
    //         </PrimaryButton>
    //         <DefaultButton disabled={!newProjectFormEnabled} onClick={() => _onNewProjectTypeFormReset()}>Cancel</DefaultButton>
    //         <Spinner styles={{ root: { visibility: newProjectFormEnabled ? 'hidden' : 'visible' } }} />
    //     </div>
    // );

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
                // onProjectTypeSelected={props.onProjectSelected}
                />
            </Stack>
            {/* <Panel
                headerText='New project type'
                isOpen={newProjectTypePanelOpen}
                onDismiss={() => _onNewProjectTypeFormReset()}
                onRenderFooterContent={_onRenderNewProjectTypeFormFooterContent}>
                <ProjectTypeForm
                    fieldsEnabled={!newProjectTypeFormEnabled}
                    onNameChange={_onProjectTypeFormNameChange}
                    // onProjectTypeChange={_onProjectFormTypeChange}
                    onFormSubmit={() => _onCreateNewProjectType()} />
                <Text>{newProjectErrorText}</Text>
            </Panel> */}
        </>
    );
}
