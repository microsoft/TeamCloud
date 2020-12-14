// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, useEffect, useState } from 'react';
import { Stack, DefaultButton, Text, ICommandBarItemProps, Dialog, DialogType, DialogFooter, PrimaryButton, FontIcon, IColumn, Persona, PersonaSize, DetailsList, DetailsListLayoutMode, CheckboxVisibility, IDetailsRowProps, IRenderFunction, SelectionMode } from '@fluentui/react';
import { Component, ComponentTemplate, ErrorResult } from 'teamcloud';
import { DetailCard } from '.';
import { api } from '../API';
import { useHistory, useParams } from 'react-router-dom';
import { OrgContext, ProjectContext } from '../Context';
import DevOps from '../img/devops.svg';
import GitHub from '../img/github.svg';
import Resource from '../img/resource.svg';
import { ComponentLink } from './ComponentLink';


export const ComponentsCard: React.FC = () => {

    const history = useHistory();

    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };
    const { org, scopes } = useContext(OrgContext);
    const { project, components, templates, onComponentSelected } = useContext(ProjectContext);

    const [component, setComponent] = useState<Component>();
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

    const [items, setItems] = useState<{ component: Component, template: ComponentTemplate }[]>()


    useEffect(() => {
        if (components && templates && (items === undefined || items.length !== components.length)) {
            setItems(components.map(c => ({ component: c, template: templates.find(t => t.id === c.templateId)! })));
        }
    }, [components, templates, items]);

    // const { project, components } = useContext(ProjectContext);




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







    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (rowProps?: IDetailsRowProps, defaultRender?: (rowProps?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (rowProps) rowProps.styles = {
            // root: { borderBottom: (props.noHeader ?? false) && items.length === 1 ? 0 : undefined },

            root: { border: 'none' },
            fields: { alignItems: 'center' },
            // check: { minHeight: '62px' },
            cell: { fontSize: '14px', paddingLeft: '0px' },
        }
        return defaultRender ? defaultRender(rowProps) : null;
    };



    const _getTypeImage = (template: ComponentTemplate) => {
        const provider = template.repository.provider.toLowerCase();
        switch (template.type) {
            // case 'Custom': return 'Link';
            // case 'Readme': return 'PageList';
            case 'Environment': return Resource;
            case 'AzureResource': return Resource;
            case 'GitRepository': return provider === 'github' ? GitHub : provider === 'devops' ? DevOps : undefined;
        }
        return undefined;
    };


    const _getTypeIcon = (template: ComponentTemplate) => {
        if (template.type)
            switch (template.type) { // VisualStudioIDELogo32
                case 'Custom': return 'Link'; // Link12, FileSymlink, OpenInNewWindow, VSTSLogo
                case 'Readme': return 'PageList'; // Preview, Copy, FileHTML, FileCode, MarkDownLanguage, Document
                case 'Environment': return 'AzureLogo'; // Processing, Settings, Globe, Repair
                case 'AzureResource': return 'AzureLogo'; // AzureServiceEndpoint
                case 'GitRepository': return 'OpenSource';
                default: return undefined;
            }
    };


    const onRenderNameColumn = (item?: { component: Component, template: ComponentTemplate }) => {
        if (!item) return undefined;
        // const name = item.displayName?.replaceAll('-', ' ');
        return (
            // <Stack tokens={{ padding: '5px' }}>
            <Persona
                text={item.component.displayName ?? undefined}
                size={PersonaSize.size24}
                imageUrl={_getTypeImage(item.template)}
                coinProps={{ styles: { initials: { borderRadius: '4px' } } }}
                styles={{
                    root: { color: 'inherit' },
                    primaryText: { color: 'inherit', textTransform: 'capitalize' }
                }} />
            // </Stack>
        );
    };


    const onRenderTypeColumn = (item?: { component: Component, template: ComponentTemplate }) => {
        if (!item) return undefined;
        return (
            <Stack horizontal >
                <FontIcon iconName={_getTypeIcon(item.template)} className='component-type-icon' />
                <Text styles={{ root: { paddingLeft: '4px' } }}>{item.template.type}</Text>
            </Stack>
        )
    };


    const onRenderLinkColumn = (item?: { component: Component, template: ComponentTemplate }) => {
        if (!item || !org) return undefined;
        return <ComponentLink component={item.component} />
    };



    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 200, maxWidth: 200, onRender: onRenderNameColumn },
        { key: 'type', name: 'Type', minWidth: 160, maxWidth: 160, onRender: onRenderTypeColumn },
        { key: 'scope', name: 'Scope', minWidth: 140, maxWidth: 140, onRender: (i: { component: Component, template: ComponentTemplate }) => scopes?.find(s => s.id === i.component.deploymentScopeId)?.displayName },
        // { key: 'state', name: 'State', minWidth: 140, onRender: (i: { component: Component, template: ComponentTemplate }) => i.component.resourceState },
        { key: 'link', name: 'Link', minWidth: 140, maxWidth: 140, onRender: onRenderLinkColumn },
        // { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
        // { key: 'blank', name: '', minWidth: 40, maxWidth: 40, onRender: (_: ComponentTemplate) => undefined },
        // { key: 'repository', name: 'Repository', minWidth: 240, maxWidth: 240, onRender: onRenderRepoColumn },
        // { key: 'version', name: 'Version', minWidth: 90, maxWidth: 90, onRender: (i: { component: Component, template: ComponentTemplate }) => i.template.repository.version },
        // { key: 'requestedBy', name: 'Creator', minWidth: 50, onRender: onRenderCreatorColumn },
    ];


    const _onItemInvoked = (item: { component: Component, template: ComponentTemplate }): void => {
        // console.log(item);
        onComponentSelected(item.component);
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
                    isHeaderVisible={false}
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
