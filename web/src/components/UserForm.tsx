// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { PrimaryButton, DefaultButton, Stack, TextField, Dropdown, Spinner, IconButton, Pivot, PivotItem, IColumn, DetailsList, DetailsListLayoutMode, CheckboxVisibility, Text, SelectionMode } from '@fluentui/react';
import { AlternateIdentity, User, UserRole } from 'teamcloud';
import { GraphUser } from '../model'
import { api } from '../API'
import { UserPersona, Lightbox } from '.';
import { prettyPrintCamlCaseString } from '../Utils';
import { useProjects } from '../hooks';
import { useQueryClient } from 'react-query';

export interface IUserFormProps {
    me: boolean;
    user?: User;
    graphUser?: GraphUser;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const UserForm: React.FC<IUserFormProps> = (props) => {

    interface AlternateIdentityItem {
        key: string
        title: string
        identity: AlternateIdentity
    }

    interface PropertyItem {
        key: string
        value: string
    }

    interface MembershipItem {
        project: string
        role: string
    }

    const detailsListTextStyle: React.CSSProperties = {
        verticalAlign: 'middle',
        lineHeight: '30px',
        fontSize: '14px'
    }

    const queryClient = useQueryClient();

    const { data: projects } = useProjects();

    const [newPropertyKey, setNewPropertyKey] = useState<string>();
    const [newPropertyAddEnabled, setNewPropertyAddEnabled] = useState<boolean>(false);

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [formUser, setFormUser] = useState<User>();
    const [pivotKey, setPivotKey] = useState<string>('Details');
    const [errorMessage, setErrorMessage] = useState<string>();

    useEffect(() => {
        if (props.user) {
            // console.log("Updating form user: " + JSON.stringify(props.user));
            setFormUser({ ...props.user });
        } else {
            setFormUser(undefined);
        }
    }, [props.user]);

    useEffect(() => {
        if (formUser && formUser.properties) {

            var sanitized: string = (newPropertyKey ?? '').trim();
            var enabled: boolean = (sanitized.length > 0 && (!Object.keys(formUser.properties).includes(sanitized) ?? false));

            setNewPropertyAddEnabled(enabled);

        } else {
            setNewPropertyAddEnabled(false);
        }

    }, [formUser, newPropertyKey])

    const _submitForm = async () => {
        if (formUser) {

            console.log(`Submitting: ${JSON.stringify(formUser)}`);

            try {

                setFormEnabled(false);

                await api.updateOrganizationUserMe(formUser!.organization, {
                    body: formUser,
                    onResponse: (raw, flat) => {
                        if (raw.status >= 400)
                            throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
                    }
                });

                queryClient.invalidateQueries(['org', formUser!.organization, 'user', 'me']);

                _resetAndCloseForm();

            } catch (error) {

                setErrorMessage(`${error}`);

            } finally {

                setFormEnabled(true);
            }

        } else {

            _resetAndCloseForm();
        }
    };

    const _resetAndCloseForm = () => {
        setFormEnabled(true);
        setFormUser(undefined);
        props.onFormClose();
    };

    const _onRoleChange = (newRole?: UserRole) => {
        if (formUser && newRole)
            formUser.role = newRole ?? "None";
    }

    const _onPropertyAdd = (newValue?: string) => {
        if (formUser?.properties && newValue) {
            let sanitized = newValue.trim();
            if (!Object.keys(formUser.properties).includes(sanitized)) {
                formUser.properties[sanitized] = '';
                setNewPropertyKey('');
            }
        }
    }

    const _onPropertyUpdate = (key: string, newValue?: string) => {
        if (formUser?.properties) {
            let sanitized = key.trim();
            if (Object.keys(formUser.properties).includes(sanitized)) {
                formUser.properties[sanitized] = newValue || '';
                setFormUser({ ...formUser });
            }
        }
    }

    const _onPropertyDelete = (key: string) => {
        if (formUser?.properties) {
            let sanitized = key.trim();
            if (Object.keys(formUser.properties).includes(sanitized)) {
                delete formUser.properties[sanitized];
                setFormUser({ ...formUser });
            }
        }
    }
    const _renderPropertyKeyColumn = (item?: PropertyItem, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return (<Stack><Text style={detailsListTextStyle}>{item.key}</Text></Stack>);
    }

    const _renderPropertyValueColumn = (item?: PropertyItem, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return (
            <Stack horizontal>
                <TextField
                    value={item.value}
                    styles={{ root: { width: '100%' } }}
                    onChange={(_ev, val) => _onPropertyUpdate(item.key, val)} />
                <IconButton
                    iconProps={{ iconName: 'Delete' }}
                    style={{ backgroundColor: 'transparent' }}
                    onClick={() => _onPropertyDelete(item.key)} />
            </Stack>);
    }

    const propertiesColums: IColumn[] = [
        {
            key: 'key', name: 'Key',
            minWidth: 100, maxWidth: 200, isResizable: false,
            onRender: _renderPropertyKeyColumn
        },
        {
            key: 'value', name: 'Value',
            minWidth: 400,
            onRender: _renderPropertyValueColumn
        }
    ];

    const _renderPropertiesPivot = () => {
        let items: PropertyItem[] = [];

        if (formUser?.properties) {
            for (const key in formUser.properties) {
                items.push({
                    key: key,
                    value: formUser.properties[key] ?? ''
                } as PropertyItem);
            }
        }

        return (
            <Stack>
                <DetailsList
                    columns={propertiesColums}
                    items={items}
                    layoutMode={DetailsListLayoutMode.justified}
                    checkboxVisibility={CheckboxVisibility.hidden}
                    selectionMode={SelectionMode.none}
                    selectionPreservedOnEmptyClick
                />
                <Stack horizontal style={{ marginTop: '10px' }}>
                    <TextField
                        value={newPropertyKey}
                        placeholder='Create a new property'
                        onKeyDown={(_ev) => { if (_ev.keyCode === 13) _onPropertyAdd(newPropertyKey) }}
                        onChange={(_ev, val) => setNewPropertyKey(val)} />
                    <IconButton
                        iconProps={{ iconName: 'AddTo' }}
                        disabled={!newPropertyAddEnabled}
                        style={{ backgroundColor: 'transparent' }}
                        onClick={() => _onPropertyAdd(newPropertyKey)} />
                </Stack>
            </Stack>)
    };

    const _renderDetailsPivot = () => {

        const roleControl = props.me
            ? (<TextField
                readOnly
                label='Role'
                value={formUser?.role} />)
            : (<Dropdown
                required
                label='Role'
                selectedKey={formUser?.role ?? undefined}
                options={['Owner', 'Admin', 'Member', 'None'].map(r => ({ key: r, text: r, data: r }))}
                onChange={(_ev, val) => _onRoleChange(val?.key ? val.key as UserRole : undefined)} />);

        return formUser ? (
            <Stack tokens={{ childrenGap: '12px' }}>
                <Stack.Item>
                    <TextField
                        readOnly
                        label='Id'
                        value={props.user?.id} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        readOnly
                        label='Login'
                        value={props.user?.loginName ?? undefined} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        readOnly
                        label='E-Mail'
                        value={props.user?.mailAddress ?? undefined} />
                </Stack.Item>
                <Stack.Item>
                    {roleControl}
                </Stack.Item>
            </Stack>
        ) : (<></>);
    };

    const _renderMembershipValueColumn = (item?: MembershipItem, index?: number, column?: IColumn) => {
        if (item && column) {
            let property: string = column.fieldName ?? column.key ?? ''
            return <Text style={detailsListTextStyle}>{(item as any)[property]}</Text>
        }
        return <></>
    };

    const membershipColums: IColumn[] = [
        {
            key: 'project', name: 'Project', fieldName: 'project',
            minWidth: 100, maxWidth: 200, isResizable: false,
            onRender: _renderMembershipValueColumn
        },
        {
            key: 'role', name: 'Role', fieldName: 'role',
            minWidth: 400,
            onRender: _renderMembershipValueColumn
        }
    ];

    const _renderMembershipPivot = () => {

        let items: MembershipItem[] = [];

        if (projects && formUser?.projectMemberships) {
            items = formUser.projectMemberships.map((membership) => ({
                project: projects.find(p => p.id === membership.projectId)?.displayName ?? membership.projectId,
                role: membership.role
            } as MembershipItem));
        }

        return (
            <DetailsList
                columns={membershipColums}
                items={items}
                layoutMode={DetailsListLayoutMode.justified}
                checkboxVisibility={CheckboxVisibility.hidden}
                selectionPreservedOnEmptyClick />
        )
    };

    const _renderAlternateIdentityServiceColumn = (item?: AlternateIdentityItem, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return <Stack verticalAlign='center'>
            <Text style={detailsListTextStyle}>{item.title}</Text>
        </Stack>
    };

    const _onAlternateIdentityLoginChange = (key: string, newValue?: string) => {
        if (formUser?.alternateIdentities && Object.keys(formUser.alternateIdentities).includes(key)) {
            let identity = (formUser.alternateIdentities as any)[key] as AlternateIdentity;
            if (identity) {
                identity.login = newValue || '';
                setFormUser({ ...formUser });
            }
        }
    }

    const _renderAlternateIdentityLoginColumn = (item?: AlternateIdentityItem, index?: number, column?: IColumn) => {
        if (!item || !item.identity) return undefined
        return <TextField
            value={item.identity.login ?? undefined}
            onChange={(_ev, val) => _onAlternateIdentityLoginChange(item.key, val)} />;
    };

    const alternateIdentitiesColums: IColumn[] = [
        {
            key: 'title', name: 'Service',
            minWidth: 100, maxWidth: 200, isResizable: false,
            onRender: _renderAlternateIdentityServiceColumn
        },
        {
            key: 'login', name: 'Login',
            minWidth: 400,
            onRender: _renderAlternateIdentityLoginColumn
        }
    ];

    const _renderAlternateIdentitiesPivot = () => {

        let items: AlternateIdentityItem[] = [];

        if (formUser?.alternateIdentities) {
            items = Object.getOwnPropertyNames(formUser.alternateIdentities).map((name) => ({
                key: name,
                title: prettyPrintCamlCaseString(name),
                identity: (formUser.alternateIdentities as any)[name]
            }));
        }

        return (
            <DetailsList
                columns={alternateIdentitiesColums}
                items={items}
                layoutMode={DetailsListLayoutMode.justified}
                checkboxVisibility={CheckboxVisibility.hidden}
                selectionPreservedOnEmptyClick />
        )
    };

    const _renderLightboxHeader = (): JSX.Element => {
        return (<UserPersona principal={props.graphUser} large />);
    };

    const _renderLightboxFooter = (): JSX.Element => {
        return (
            <>
                <Stack.Item><Text style={detailsListTextStyle}>{errorMessage}</Text></Stack.Item>
                <Stack.Item><Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} /></Stack.Item>
                <PrimaryButton text='Ok' disabled={!formEnabled} onClick={() => _submitForm()} />
                <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            </>
        );
    }

    return (
        <Lightbox
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}
            onRenderHeader={_renderLightboxHeader}
            onRenderFooter={_renderLightboxFooter}>
            <Pivot selectedKey={pivotKey} onLinkClick={(i, e) => setPivotKey(i?.props.itemKey ?? 'Details')} styles={{ root: { height: '100%', marginBottom: '12px' } }}>
                <PivotItem headerText='Details' itemKey='Details'>
                    {_renderDetailsPivot()}
                </PivotItem>
                <PivotItem headerText='Memberships' itemKey='Memberships'>
                    {_renderMembershipPivot()}
                </PivotItem>
                <PivotItem headerText='Properties' itemKey='Properties'>
                    {_renderPropertiesPivot()}
                </PivotItem>
                <PivotItem headerText='Alternate Identities' itemKey='AlternateIdentities'>
                    {_renderAlternateIdentitiesPivot()}
                </PivotItem>
            </Pivot>
        </Lightbox>
    );
}
