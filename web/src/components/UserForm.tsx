// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { PrimaryButton, DefaultButton, Stack, TextField, Dropdown, Spinner, getTheme, Modal, IconButton, Pivot, PivotItem, IColumn, DetailsList, DetailsListLayoutMode, CheckboxVisibility, Text, SelectionMode } from '@fluentui/react';
import { AlternateIdentity, User, UserRole } from 'teamcloud';
import { GraphUser } from '../model'
import { api } from '../API'
import { UserPersona } from '.';
import { prettyPrintCamlCaseString } from '../Utils';

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

    const theme = getTheme();

    const [newPropertyKey, setNewPropertyKey] = useState<string>();
    const [newPropertyAddEnabled, setNewPropertyAddEnabled] = useState<boolean>(false);

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [formUser, setFormUser] = useState<User>();
    const [pivotKey, setPivotKey] = useState<string>('Details');
    const [errorMessage, setErrorMessage] = useState<string>();

    useEffect(() => {
        if (props.user) {
            setFormUser({ ...props.user });
        } else {
            setFormUser(undefined);
        }
    }, [props.user]);

    useEffect(() => {    
        if (formUser && formUser.properties)
        {   
            var sanitized: string = (newPropertyKey ?? '').trim();
            var enabled: boolean = (sanitized.length > 0 && (!Object.keys(formUser.properties).includes(sanitized) ?? false));

            setNewPropertyAddEnabled(enabled);
        } else {
            setNewPropertyAddEnabled(false);
        }

    }, [formUser, newPropertyKey])

    const _submitForm = async () => {
        if (props.user) {
            setFormEnabled(false);
            const result = await api.updateOrganizationUser(props.user!.id, props.user!.organization, { body: formUser });
            console.log(JSON.stringify(result));
            if (result.code === 200) {
                _resetAndCloseForm();
            } else {
                setErrorMessage(result.status ?? undefined);
            }
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
            if (!Object.keys(formUser.properties).includes(sanitized))
            {
                formUser.properties[sanitized] = '';
                setNewPropertyKey('');
            }
        }
    }

    const _onPropertyUpdate = (key: string, newValue?: string) => {
        if (formUser?.properties) {
            let sanitized = key.trim();
            if (Object.keys(formUser.properties).includes(sanitized))
            {
                formUser.properties[sanitized] = newValue || '';
                setFormUser({ ...formUser });
            }
        }
    }

    const _onPropertyDelete = (key: string) => {
        if (formUser?.properties) {
            let sanitized = key.trim();
            if (Object.keys(formUser.properties).includes(sanitized))
            {
                delete formUser.properties[sanitized];
                setFormUser({ ...formUser });
            }
        }
    }
    const _renderPropertyKeyColumn = (item?: PropertyItem, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return (
            <Stack>
                <Text style={{ verticalAlign: 'middle', lineHeight: '30px' }}>{ item.key }</Text>
            </Stack>);
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
                    style={{ backgroundColor: 'transparent'}}
                    onClick={() => _onPropertyDelete(item.key)} />
            </Stack>);
    }

    const propertiesColums: IColumn[] = [
        { key: 'key', name: 'Key', minWidth: 100, maxWidth: 200, isResizable: false, onRender: _renderPropertyKeyColumn },
        { key: 'value', name: 'Value', minWidth: 400, onRender: _renderPropertyValueColumn },
    ];

    const _renderPropertiesPivot = () => {
        let items:PropertyItem[] = [];

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
                    selectionPreservedOnEmptyClick={true}
                    />
                <Stack horizontal style={{marginTop: '10px'}}>
                    <TextField 
                        value={newPropertyKey} 
                        placeholder='Create a new property'
                        onKeyDown={(_ev) => { if (_ev.keyCode === 13) _onPropertyAdd(newPropertyKey) }}
                        onChange={(_ev, val) => setNewPropertyKey(val)} />
                    <IconButton 
                        iconProps={{ iconName: 'AddTo' }} 
                        disabled={!newPropertyAddEnabled}
                        style={{ backgroundColor: 'transparent'}}
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

    const _renderMembershipPivot = () => {
        return (<></>);
    };

    const _renderAlternateIdentityServiceColumn = (item?: AlternateIdentityItem, index?: number, column?: IColumn) => {
        if (!item) return undefined;
        return <Stack verticalAlign='center'>
            <Text>{ item.title }</Text>
        </Stack>
    };

    const _onAlternateIdentityLoginChange = (key: string, newValue?: string) => {
        if (formUser)
        {
            let alternateIdentity:AlternateIdentity = (formUser.alternateIdentities as any)[key];
            if (alternateIdentity) alternateIdentity.login = newValue;
        }
    }

    const _renderAlternateIdentityLoginColumn =(item?: AlternateIdentityItem, index?: number, column?: IColumn) => {
        if (!item || !item.identity) return undefined
        return <TextField 
            value={item.identity.login ?? undefined } 
            onChange={(_ev, val) => _onAlternateIdentityLoginChange(item.key, val)} />;
    };

    const alternateIdentitiesColums: IColumn[] = [
        { key: 'title', name: 'Service', minWidth: 100, maxWidth: 200, isResizable: false, onRender: _renderAlternateIdentityServiceColumn },
        { key: 'login', name: 'Login', minWidth: 400, onRender: _renderAlternateIdentityLoginColumn },
    ];

    const _renderAlternateIdentitiesPivot = () => {
        
        let items:AlternateIdentityItem[] = [];

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
                selectionPreservedOnEmptyClick={true} />
        )
    };

    return (

        <Modal
            theme={theme}
            styles={{ main: { margin: 'auto 100px', minHeight:'calc(100% - 32px)', minWidth:'calc(100% - 32px)' }, scrollableContent: { padding: '50px' } }}
            isBlocking={false}
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}>
                
            <Stack tokens={{ childrenGap: '12px' }} style={{height: 'calc(100vh - 132px)' }}>
                <Stack.Item>
                    <Stack horizontal horizontalAlign='space-between' 
                        tokens={{ childrenGap: '50px' }} 
                        style={{ paddingBottom: '32px', borderBottom: '1px lightgray solid' }}>
                        <Stack.Item>
                            <UserPersona principal={props.graphUser} large />
                        </Stack.Item>
                        <Stack.Item>
                            <IconButton 
                                iconProps={{ iconName: 'ChromeClose' }}
                                onClick={() => _resetAndCloseForm()} />
                        </Stack.Item>
                    </Stack>
                </Stack.Item>
                <Stack.Item>
                    <Pivot selectedKey={pivotKey} onLinkClick={(i, e) => setPivotKey(i?.props.itemKey ?? 'Details')} styles={{ root: { height: '100%', marginBottom: '12px' } }}>
                        <PivotItem headerText='Details' itemKey='Details'>
                            {_renderDetailsPivot()}
                        </PivotItem>
                        <PivotItem headerText='Properties' itemKey='Properties'>
                            {_renderPropertiesPivot()}
                        </PivotItem>
                        <PivotItem headerText='Memberships' itemKey='Memberships'>
                            {_renderMembershipPivot()}
                        </PivotItem>
                        <PivotItem headerText='Alternate Identities' itemKey='AlternateIdentities'>
                            {_renderAlternateIdentitiesPivot()}
                        </PivotItem>
                    </Pivot>
                </Stack.Item>
                <Stack.Item style={{}}>
                    <Stack horizontal horizontalAlign='end'
                        tokens={{ childrenGap: '50px' }} 
                        style={{ paddingTop: '32px', borderTop: '1px lightgray solid', position: 'absolute', left: '50px', bottom: '50px', width: 'calc(100% - 100px)' }}>
                        <Stack.Item>
                            <Text>{errorMessage}</Text>
                            <PrimaryButton text='Ok' disabled={!formEnabled} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
                            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
                            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
                        </Stack.Item>
                    </Stack>
                </Stack.Item>
            </Stack>

        </Modal>
    );
}
