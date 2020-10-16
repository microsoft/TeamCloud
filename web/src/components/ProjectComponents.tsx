// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { DataResult, Project, Component, User, StatusResult, ErrorResult } from '../model'
// import { Project, Component, User, StatusResult, ErrorResult } from 'teamcloud';
import { Stack, Shimmer, DefaultButton, IButtonStyles, getTheme, Image, ICommandBarItemProps, Dialog, DialogType, DialogFooter, PrimaryButton, IContextualMenuProps, IContextualMenuItem } from '@fluentui/react';
import { ProjectDetailCard, ProjectComponentForm } from '.';
import AppInsights from '../img/appinsights.svg';
import DevOps from '../img/devops.svg';
import DevTestLabs from '../img/devtestlabs.svg';
import GitHub from '../img/github.svg';
import { getProjectComponents, deleteProjectComponent } from '../API';

export interface IProjectComponentsProps {
    user?: User;
    project: Project;
}

export const ProjectComponents: React.FunctionComponent<IProjectComponentsProps> = (props) => {

    const [component, setComponent] = useState<Component>();
    const [components, setComponents] = useState<Component[]>();
    const [addComponentPanelOpen, setAddComponentPanelOpen] = useState(false);
    const [showContextualMenu, setShowContextualMenu] = useState(false);
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

    useEffect(() => {
        if (props.project) {
            const _setComponents = async () => {
                const result = await getProjectComponents(props.project.id!);
                const data = (result as DataResult<Component[]>).data;
                setComponents(data);
            };
            _setComponents();
        }
    }, [props.project]);

    const _findKnownProviderImage = (component: Component) => {
        if (component.offerId) {
            if (component.offerId.includes('azure.appinsights')) return AppInsights;
            if (component.offerId.includes('azure.devops')) return DevOps;
            if (component.offerId.includes('azure.devtestlabs')) return DevTestLabs;
            if (component.offerId.includes('github')) return GitHub;
        }
        return undefined;
    }

    const _itemMenuProps = (component: Component): IContextualMenuProps => ({
        items: [
            {
                key: 'delete',
                text: 'Delete component',
                iconProps: { iconName: 'Delete' },
                data: component,
                onClick: _onItemButtonClicked
            }
        ]
    });

    const _onItemButtonClicked = (ev?: React.MouseEvent<HTMLElement> | React.KeyboardEvent<HTMLElement>, item?: IContextualMenuItem): boolean | void => {
        let component = item?.data as Component;
        if (component) {
            setComponent(component);
            setDeleteConfirmOpen(true);
        }
    };

    const _getCommandBarItems = (): ICommandBarItemProps[] => [
        { key: 'newComponent', text: 'New', iconProps: { iconName: 'WebAppBuilderFragmentCreate' }, onClick: () => { setAddComponentPanelOpen(true) } },
    ];


    const _onComponentDelete = async () => {
        if (component) {
            const result = await deleteProjectComponent(props.project.id, component.id);
            if ((result as StatusResult).code !== 202 && (result as ErrorResult).errors) {
                console.log(result as ErrorResult);
            }
            setComponent(undefined);
            setDeleteConfirmOpen(false);
        }
    }

    const _confirmDialogSubtext = (): string => `This will permanently delete '${component?.displayName ? component.displayName : 'this component'}'. This action connot be undone.`;

    const theme = getTheme();

    const _componentButtonStyles: IButtonStyles = {
        root: {
            // border: 'none',
            width: '100%',
            textAlign: 'start',
            borderBottom: '1px',
            borderStyle: 'none none solid none',
            borderRadius: '0',
            borderColor: theme.palette.neutralLighter,
            padding: '24px 6px'
        },
        menuIcon: {
            display: 'none'
        }
    }

    const _getComponentStacks = () => components?.sort((a, b) => a.offerId === b.offerId ? 0 : a.offerId > b.offerId ? 1 : -1).map(c => (
        <Stack key={c.id} horizontal tokens={{ childrenGap: '12px' }}>
            <Stack.Item styles={{ root: { width: '100%' } }}>
                <DefaultButton
                    // iconProps={{ iconName: _getLinkTypeIcon(l) }}
                    text={c.displayName ?? c.id}
                    secondaryText={c.description ?? c.offerId}
                    // href={l.href}
                    target='_blank'
                    styles={_componentButtonStyles}
                    menuProps={_itemMenuProps(c)}>
                    <Image
                        src={_findKnownProviderImage(c)}
                        height={24} width={24} />
                </DefaultButton>
            </Stack.Item>
        </Stack>
    ));

    return (
        <>
            <ProjectDetailCard
                title='Components'
                callout={components?.length.toString()}
                commandBarItems={_getCommandBarItems()} >
                <Shimmer
                    // customElementsGroup={_getShimmerElements()}
                    isDataLoaded={components !== undefined}
                    width={152} >
                    <Stack tokens={{ childrenGap: '0' }} >
                        {_getComponentStacks()}
                    </Stack>
                </Shimmer>
            </ProjectDetailCard>
            <ProjectComponentForm
                user={props.user}
                project={props.project}
                panelIsOpen={addComponentPanelOpen}
                onFormClose={() => setAddComponentPanelOpen(false)} />
            <Dialog
                hidden={!deleteConfirmOpen}
                dialogContentProps={{ type: DialogType.normal, title: 'Confirm Delete', subText: _confirmDialogSubtext() }}>
                <DialogFooter>
                    <PrimaryButton text='Delete' onClick={() => _onComponentDelete()} />
                    <DefaultButton text='Cancel' onClick={() => setDeleteConfirmOpen(false)} />
                </DialogFooter>
            </Dialog>
        </>
    );
}
