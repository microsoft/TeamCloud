// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, useState } from 'react';
import { Stack, Shimmer, DefaultButton, IButtonStyles, getTheme, ICommandBarItemProps, Dialog, DialogType, DialogFooter, PrimaryButton, IContextualMenuProps, IContextualMenuItem } from '@fluentui/react';
import { Component, ErrorResult } from 'teamcloud';
import { DetailCard } from '.';
import { api } from '../API';
import { useHistory, useParams } from 'react-router-dom';
import { ProjectContext } from '../Context';

export const ComponentsCard: React.FC = () => {

    const history = useHistory();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [component, setComponent] = useState<Component>();
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

    const { project, components } = useContext(ProjectContext);

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
        { key: 'newComponent', text: 'New', iconProps: { iconName: 'WebAppBuilderFragmentCreate' }, onClick: () => history.push(`/orgs/${orgId}/projects/${projectId}/components/new`) },
    ];


    const _onComponentDelete = async () => {
        if (component && project) {
            const result = await api.deleteProjectComponent(component.id, project.organization, project.id);
            if (result.code !== 202 && (result as ErrorResult).errors) {
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

    const _getComponentStacks = () => components?.sort((a, b) => a.templateId === b.templateId ? 0 : (a.templateId ?? '') > (b.templateId ?? '') ? 1 : -1).map(c => (
        <Stack key={c.id} horizontal tokens={{ childrenGap: '12px' }}>
            <Stack.Item styles={{ root: { width: '100%' } }}>
                <DefaultButton
                    // iconProps={{ iconName: _getLinkTypeIcon(l) }}
                    text={c.displayName ?? c.id}
                    secondaryText={c.description ?? c.templateId}
                    // href={l.href}
                    target='_blank'
                    styles={_componentButtonStyles}
                    menuProps={_itemMenuProps(c)}>
                    {/* <Image
                        src={_findKnownProviderImage(c)}
                        height={24} width={24} /> */}
                </DefaultButton>
            </Stack.Item>
        </Stack>
    ));

    return (
        <>
            <DetailCard
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
            </DetailCard>
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
