// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Organization, Project } from 'teamcloud';
import { Link, useHistory } from 'react-router-dom';
import { ShimmeredDetailsList, DetailsListLayoutMode, IColumn, CommandButton, IContextualMenuProps, IContextualMenuItem, IRenderFunction, IDetailsRowProps, CheckboxVisibility, SelectionMode, Dialog, DialogType, DialogFooter, PrimaryButton, DefaultButton, Persona, PersonaSize, getTheme, DetailsList, Stack } from '@fluentui/react';

export interface IProjectListProps {
    org?: Organization;
    projects?: Project[];
    projectFilter?: string;
    onProjectSelected?: (project: Project) => void;
    onProjectDeleted?: (project?: Project) => void;
}

export const ProjectList: React.FunctionComponent<IProjectListProps> = (props) => {

    const [project, setProject] = useState<Project>();
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

    const history = useHistory();

    const _itemMenuProps = (project: Project): IContextualMenuProps => ({
        items: [
            {
                key: 'delete',
                text: 'Delete project',
                iconProps: { iconName: 'Delete' },
                data: project,
                onClick: _onItemButtonClicked
            }
        ]
    });

    const _onItemButtonClicked = (ev?: React.MouseEvent<HTMLElement> | React.KeyboardEvent<HTMLElement>, item?: IContextualMenuItem): boolean | void => {
        let project = item?.data as Project;
        if (project) {
            setProject(project);
            setDeleteConfirmOpen(true);
        }
    };

    // const columns: IColumn[] = [
    // { key: 'name', name: 'Project Name', onRender: (p: Project) => (<Link onClick={() => _onLinkClicked(p)} to={'/projects/' + p.id} style={{ textDecoration: 'none' }}>{p.displayName}</Link>), minWidth: 100, isResizable: true },
    // { key: 'button', name: 'Commands Button', onRender: (p: Project) => (<CommandButton menuIconProps={{ iconName: 'More' }} menuProps={_itemMenuProps(p)} />), minWidth: 30, isResizable: false, isIconOnly: true },
    // { key: 'id', name: 'ID', fieldName: 'id', minWidth: 300, isResizable: true },
    // { key: 'type', name: 'Type', onRender: (p: Project) => (<Link to={'/projectTypes/' + p.type.id} style={{ textDecoration: 'none' }}>{p.type.id}</Link>), minWidth: 100, isResizable: true },
    // { key: 'group', name: 'ResourceGroup', onRender: (p: Project) => p.resourceGroup?.name, minWidth: 320, isResizable: true },
    // { key: 'location', name: 'Location', onRender: (p: Project) => p.resourceGroup?.region, minWidth: 100, isResizable: true },
    // { key: 'userCount', name: 'Users', onRender: (p: Project) => p.users.length, minWidth: 100, isResizable: true },
    // ];

    const theme = getTheme();

    const columns: IColumn[] = [
        {
            key: 'project', name: 'Project', minWidth: 100, isResizable: false, onRender: (p: Project) => (
                <Stack tokens={{ padding: '8px' }}>
                    <Persona
                        text={p.displayName}
                        size={PersonaSize.size48}
                        coinProps={{
                            styles: {
                                initials: {
                                    borderRadius: '4px',
                                    fontSize: '20px',
                                    fontWeight: '400'
                                }
                            }
                        }}
                        styles={{
                            primaryText: {
                                fontSize: theme.fonts.xLarge.fontSize,
                                fontWeight: theme.fonts.xLarge.fontWeight
                            }
                        }}
                        imageInitials={p.displayName.split(' ').map(s => s[0].toUpperCase()).join('')} />
                </Stack>
            )
        }
    ];


    const _applyProjectFilter = (project: Project): boolean => {
        return props.projectFilter ? project.displayName.toUpperCase().includes(props.projectFilter.toUpperCase()) : true;
    }

    const _onLinkClicked = (project: Project): void => {
        if (props.onProjectSelected)
            props.onProjectSelected(project);
    }

    const _onItemInvoked = (project: Project): void => {
        if (props.org) {
            _onLinkClicked(project);
            history.push(`/orgs/${props.org.slug}/projects/${project.slug}`)
        } else {
            console.warn('nope');
        }
    };

    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (props?: IDetailsRowProps, defaultRender?: (props?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (props) props.styles = { fields: { alignItems: 'center' }, check: { minHeight: '62px' } }
        return defaultRender ? defaultRender(props) : null;
    };

    const _onProjectDelete = () => {
        if (project && props.onProjectDeleted) {
            props.onProjectDeleted(project)
            setProject(undefined);
        }
        setDeleteConfirmOpen(false);
    };

    const _confirmDialogSubtext = (): string => `This will permanently delete '${project ? project.displayName : 'this project'}'. This action connot be undone.`;

    // const _onColumnHeaderClicked = (ev?: React.MouseEvent<HTMLElement>, column?: IColumn) => {
    //     console.log(column?.key);
    // }

    const items = props.projects ? props.projects.filter(_applyProjectFilter) : new Array<Project>();

    return (
        <>
            <DetailsList
                items={items}
                columns={columns}
                isHeaderVisible={false}
                onRenderRow={_onRenderRow}
                selectionMode={SelectionMode.none}
                layoutMode={DetailsListLayoutMode.justified}
                checkboxVisibility={CheckboxVisibility.hidden}
                selectionPreservedOnEmptyClick={true}
                onItemInvoked={_onItemInvoked}
                styles={{
                    root: {
                        borderRadius: theme.effects.roundedCorner4,
                        boxShadow: theme.effects.elevation4,
                        backgroundColor: theme.palette.white
                    }
                }} />

            <Dialog
                hidden={!deleteConfirmOpen}
                dialogContentProps={{ type: DialogType.normal, title: 'Confirm Delete', subText: _confirmDialogSubtext() }}>
                <DialogFooter>
                    <PrimaryButton text='Delete' onClick={() => _onProjectDelete()} />
                    <DefaultButton text='Cancel' onClick={() => setDeleteConfirmOpen(false)} />
                </DialogFooter>
            </Dialog>
        </>
    );
}
