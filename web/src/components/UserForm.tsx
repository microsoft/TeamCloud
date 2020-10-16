// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Properties, User, GraphUser, TeamCloudUserRole } from '../model';
import { PrimaryButton, DefaultButton, Panel, Stack, TextField, Dropdown, Label, Spinner, Persona, PersonaSize } from '@fluentui/react';

export interface IUserFormProps {
    // user: User;
    me: boolean;
    user?: User;
    graphUser?: GraphUser;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const UserForm: React.FunctionComponent<IUserFormProps> = (props) => {

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [userRole, setUserRole] = useState<TeamCloudUserRole>();
    const [userProperties, setUserProperties] = useState<Properties>();

    useEffect(() => {
        setUserRole(props.user?.role)
        const newProps: Properties = {}
        if (props.user?.properties)
            for (const k in props.user?.properties)
                newProps[k] = props.user?.properties[k]
        newProps[''] = ''
        setUserProperties(newProps)
    }, [props.user]);


    const _submitForm = async () => {
        setFormEnabled(false);
        if ((userRole && userRole !== props.user?.role)
            || (userProperties && userProperties !== props.user?.properties)) {
            // const result = await updateProjectUser(projectDefinition);
            // if ((result as StatusResult).code === 202)
            //     _resetAndCloseForm();
            // else if ((result as ErrorResult).errors) {
            //     // console.log(JSON.stringify(result));
            //     setErrorText((result as ErrorResult).status);
            // }
        }
    };

    const _resetAndCloseForm = () => {
        setUserRole(undefined);
        setUserProperties(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton text='Edit' disabled={!formEnabled} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    const _onPropertyKeyChange = (key: string, value: string, newKey?: string) => {
        const newProps: Properties = {}
        for (const k in userProperties) newProps[(k === key) ? newKey ?? '' : k] = value
        if (!newProps['']) newProps[''] = ''
        setUserProperties(newProps)
    }

    const _onPropertyValueChange = (key: string, newValue?: string) => {
        const newProps: Properties = {}
        for (const k in userProperties) newProps[k] = (k === key) ? newValue ?? '' : userProperties[k]
        setUserProperties(newProps)
    }

    const _getPropertiesTextFields = () => {
        let propertyStacks = [];
        if (userProperties) {
            let counter = 0
            for (const key in userProperties) {
                propertyStacks.push(
                    <Stack key={counter} horizontal tokens={{ childrenGap: '8px' }}>
                        <TextField
                            description='Name'
                            value={key}
                            onChange={(_ev, val) => _onPropertyKeyChange(key, userProperties[key], val)} />
                        <TextField
                            description='Value'
                            value={userProperties[key]}
                            onChange={(_ev, val) => _onPropertyValueChange(key, val)} />
                    </Stack>)
                counter++
            }
        }
        return (
            <Stack.Item>
                {propertyStacks}
            </Stack.Item>
        )
    };

    // const _roleDropdownDisabled = () => {
    //     return !formEnabled || (props.member?.projectMembership.role === ProjectUserRole.Owner
    //         && props.project.users.filter(u => u.userType === UserType.User
    //             && u.projectMemberships
    //             && u.projectMemberships!.find(pm => pm.projectId === props.project.id && pm.role === ProjectUserRole.Owner)).length === 1)
    // };

    return (
        <Panel
            headerText='Edit'
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}
            onRenderFooterContent={_onRenderPanelFooterContent}>
            <Stack tokens={{ childrenGap: '12px' }}>
                <Stack.Item>
                    <Persona
                        text={props.graphUser?.displayName ?? props.user?.id}
                        secondaryText={props.graphUser?.jobTitle ?? props.user?.userType}
                        tertiaryText={props.graphUser?.department}
                        imageUrl={props.graphUser?.imageUrl}
                        size={PersonaSize.size72} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        readOnly
                        label='Id'
                        value={props.user?.id} />
                </Stack.Item>
                <Stack.Item>
                    <Dropdown
                        required
                        label='Role'
                        // errorMessage='Project Type is required.'
                        // placeholder='Select a Project Type'
                        // disabled={_roleDropdownDisabled()}
                        disabled
                        selectedKey={userRole}
                        // defaultSelectedKey={projectRole as string}
                        options={[TeamCloudUserRole.Admin, TeamCloudUserRole.Creator].map(r => ({ key: r, text: r, data: r }))}
                        onChange={(_ev, val) => setUserRole(val?.key ? TeamCloudUserRole[val.key as keyof typeof TeamCloudUserRole] : undefined)} />
                </Stack.Item>
                <Stack.Item>
                    <Label>Properties</Label>
                    {_getPropertiesTextFields()}
                </Stack.Item>
            </Stack>
            {/* <Text>{errorText}</Text> */}
        </Panel>
    );
}
