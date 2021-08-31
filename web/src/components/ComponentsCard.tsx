// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Stack, DefaultButton, Text, ICommandBarItemProps, Dialog, DialogType, DialogFooter, PrimaryButton, IColumn, DetailsList, DetailsListLayoutMode, CheckboxVisibility, IDetailsRowProps, IRenderFunction, SelectionMode } from '@fluentui/react';
import { Component, ComponentTemplate } from 'teamcloud';
import { DetailCard, ComponentLink } from '.';
import { useOrg, useDeploymentScopes, useProject, useProjectComponents, useProjectComponentTemplates } from '../hooks';

import { useDeleteProjectComponent } from '../hooks/useDeleteProjectComponent';
import { ComponentIcon } from './ComponentIcon';

export const ComponentsCard: React.FC = () => {

    const history = useHistory();

    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const { data: org } = useOrg();
    const { data: scopes } = useDeploymentScopes();

    const { data: project } = useProject();
    const { data: components } = useProjectComponents();
    const { data: templates } = useProjectComponentTemplates();
    const deleteComponent = useDeleteProjectComponent();

    const [component, setComponent] = useState<Component>();
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

    const [items, setItems] = useState<{ component: Component, template: ComponentTemplate }[]>()


    useEffect(() => {
        if (components && templates && (items === undefined || items.length !== components.length)) {
            console.log(JSON.stringify(components));
            setItems(components.map(c => ({ component: c, template: templates.find(t => t.id === c.templateId)! })));
        }
    }, [components, templates, items]);


    const _getCommandBarItems = (): ICommandBarItemProps[] => [
        { key: 'newComponent', text: 'New', iconProps: { iconName: 'WebAppBuilderFragmentCreate' }, onClick: () => history.push(`/orgs/${orgId}/projects/${projectId}/components/new`) },
    ];


    const _onComponentDelete = async () => {
        if (component && project) {
            await deleteComponent(component);
            setComponent(undefined);
            setDeleteConfirmOpen(false);
        }
    }

    const _confirmDialogSubtext = (): string => `This will permanently delete '${component?.displayName ? component.displayName : 'this component'}'. This action connot be undone.`;


    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (rowProps?: IDetailsRowProps, defaultRender?: (rowProps?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (rowProps) rowProps.styles = {
            root: { border: 'none' },
            fields: { alignItems: 'center' },
            cell: { fontSize: '14px' },
        }
        return defaultRender ? defaultRender(rowProps) : null;
    };

    const onRenderNameColumn = (item?: { component: Component, template: ComponentTemplate }) => {
        if (!item) return undefined;
        return (
            <Stack horizontal tokens={{ childrenGap: '10px' }}>
                <ComponentIcon component={item.component} />
                <Text>{item.component?.displayName}</Text>
            </Stack>
        );
    };

    const onRenderTypeColumn = (item?: { component: Component, template: ComponentTemplate }) => {
        if (!item) return undefined;
        return <ComponentLink component={item.component} />
    };


    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 500, maxWidth: 500, onRender: onRenderNameColumn },
        { key: 'type', name: 'Resource', minWidth: 160, maxWidth: 160, onRender: onRenderTypeColumn },
        { key: 'scope', name: 'Scope', minWidth: 140, maxWidth: 140, onRender: (i: { component: Component, template: ComponentTemplate }) => scopes?.find(s => s.id === i.component.deploymentScopeId)?.displayName },
        { key: 'state', name: 'State', minWidth: 120, maxWidth: 120, onRender: (i: { component: Component, template: ComponentTemplate }) => i.component.resourceState }
    ];

    const _onItemInvoked = (item: { component: Component, template: ComponentTemplate }): void => {
        history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${item.component.slug}`);
    };

    return (
        <>
            <DetailCard
                title='Components'
                callout={components?.length}
                commandBarItems={_getCommandBarItems()}>
                <DetailsList
                    items={items ?? []}
                    columns={columns}
                    // styles={{ root: { maxWidth: '400px' } }}
                    // isHeaderVisible
                    onRenderRow={_onRenderRow}
                    layoutMode={DetailsListLayoutMode.fixedColumns}
                    checkboxVisibility={CheckboxVisibility.hidden}
                    selectionMode={SelectionMode.none}
                    onItemInvoked={_onItemInvoked}
                />
                {/* <Shimmer
                    // customElementsGroup={_getShimmerElements()}
                    isDataLoaded={components !== undefined}
                    width={152} >
                    <Stack tokens={{ childrenGap: '0' }} >
                        {_getComponentStacks()}
                    </Stack>
                </Shimmer> */}
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
