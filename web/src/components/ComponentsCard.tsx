// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, useEffect, useState } from 'react';
import { Stack, Shimmer, DefaultButton, IButtonStyles, getTheme, Image, Text, ICommandBarItemProps, Dialog, DialogType, DialogFooter, PrimaryButton, IContextualMenuProps, IContextualMenuItem, FontIcon, IColumn, Persona, PersonaSize, DetailsList, DetailsListLayoutMode, CheckboxVisibility, IDetailsRowProps, IRenderFunction, SelectionMode } from '@fluentui/react';
import { Component, ComponentTemplate, ErrorResult } from 'teamcloud';
import { DetailCard } from '.';
import { api } from '../API';
import { useHistory, useParams } from 'react-router-dom';
import { OrgContext, ProjectContext } from '../Context';
import DevOps from '../img/devops.svg';
import GitHub from '../img/github.svg';
import Resource from '../img/resource.svg';


export const ComponentsCard: React.FC = () => {

    const history = useHistory();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [component, setComponent] = useState<Component>();
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

    const [items, setItems] = useState<{ component: Component, template: ComponentTemplate }[]>()

    const { scopes } = useContext(OrgContext);
    const { project, components, templates, members, onComponentSelected } = useContext(ProjectContext);

    useEffect(() => {
        if (components && templates && (items === undefined || items.length !== components.length)) {
            setItems(components.map(c => ({ component: c, template: templates.find(t => t.id === c.templateId)! })));
        }
    }, [components, templates, items]);

    // const { project, components } = useContext(ProjectContext);


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
                    // target='_blank'
                    styles={_componentButtonStyles}
                    // menuProps={_itemMenuProps(c)}
                    onClick={() => history.push(`/orgs/${orgId}/projects/${project?.slug ?? projectId}/components/${c.id}`)}>
                    {/* <Image
                        src={_findKnownProviderImage(c)}
                        height={24} width={24} /> */}
                </DefaultButton>
            </Stack.Item>
        </Stack>
    ));




    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (rowProps?: IDetailsRowProps, defaultRender?: (rowProps?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (rowProps) rowProps.styles = {
            // root: { borderBottom: (props.noHeader ?? false) && items.length === 1 ? 0 : undefined },
            fields: { alignItems: 'center' }, check: { minHeight: '62px' }, cell: { fontSize: '14px' }
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

    const _getRepoImage = (template: ComponentTemplate) => {
        switch (template.repository.provider) {
            // case 'Unknown': return;
            case 'DevOps': return DevOps;
            case 'GitHub': return GitHub;
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


    const onRenderNameColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
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


    const onRenderTypeColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return (
            <Stack horizontal >
                <FontIcon iconName={_getTypeIcon(item.template)} className='component-type-icon' />
                <Text styles={{ root: { paddingLeft: '4px' } }}>{item.template.type}</Text>
            </Stack>
        )
    };

    const onRenderRepoColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        let name = item.template.repository.repository?.replaceAll('-', ' ') ?? item.template.repository.url;
        // if (name && item.template.repository.version)
        //     name = `${name} (${item.template.repository.version})`;
        return (
            <Stack horizontal >
                <Image src={_getRepoImage(item.template)} styles={{ image: { width: '18px', height: '18px' } }} />
                <Text styles={{ root: { paddingLeft: '4px' } }}>{name}</Text>
            </Stack>
        )
    };


    const onRenderCreatorColumn = (item?: { component: Component, template: ComponentTemplate }, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        const creator = members?.find(m => m.user.id === item.component.requestedBy);
        return (
            <Persona
                text={creator?.graphUser?.displayName ?? creator?.user.id}
                hidePersonaDetails
                // showSecondaryText
                // secondaryText={creator?.graphUser?.mail ?? (creator?.graphUser?.otherMails && creator.graphUser.otherMails.length > 0 ? creator.graphUser.otherMails[0] : undefined)}
                imageUrl={creator?.graphUser?.imageUrl}
                // styles={{ root: { paddingTop: '24px' } }}
                size={PersonaSize.size24} />

            // <Stack horizontal >
            //     <Image src={_getRepoImage(item.template)} styles={{ image: { width: '18px', height: '18px' } }} />
            //     <Text styles={{ root: { paddingLeft: '4px' } }}>{name}</Text>
            // </Stack>
        )
    };

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 200, onRender: onRenderNameColumn },
        { key: 'scope', name: 'Scope', minWidth: 140, onRender: (i: { component: Component, template: ComponentTemplate }) => scopes?.find(s => s.id === i.component.deploymentScopeId)?.displayName },
        { key: 'state', name: 'State', minWidth: 140, onRender: (i: { component: Component, template: ComponentTemplate }) => i.component.resourceState },
        { key: 'type', name: 'Type', minWidth: 140, onRender: onRenderTypeColumn },
        // { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
        // { key: 'blank', name: '', minWidth: 40, maxWidth: 40, onRender: (_: ComponentTemplate) => undefined },
        // { key: 'repository', name: 'Repository', minWidth: 240, maxWidth: 240, onRender: onRenderRepoColumn },
        // { key: 'version', name: 'Version', minWidth: 90, maxWidth: 90, onRender: (i: { component: Component, template: ComponentTemplate }) => i.template.repository.version },
        // { key: 'requestedBy', name: 'Creator', minWidth: 50, onRender: onRenderCreatorColumn },
    ];


    const _onItemInvoked = (item: { component: Component, template: ComponentTemplate }): void => {
        // console.log(item);
        onComponentSelected(item.component);
        history.push(`/orgs/${orgId}/projects/${project?.slug ?? projectId}/components/${item.component.id}`);
    };

    return (
        <>
            <DetailCard
                title='Components'
                callout={components?.length.toString()}
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
