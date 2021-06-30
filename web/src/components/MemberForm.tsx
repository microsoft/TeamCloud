// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { PrimaryButton, DefaultButton, Panel, Stack, TextField, Dropdown, Label, Spinner, Text } from '@fluentui/react';
import { ProjectMembershipRole, Project, ProjectMembership, User } from 'teamcloud';
import { GraphUser, Properties } from '../model'
import { api } from '../API';
import { UserPersona } from '.';

export interface IMemberFormProps {
    user?: User;
    project?: Project;
    graphUser?: GraphUser;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const MemberForm: React.FC<IMemberFormProps> = (props) => {

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [projectMembership, setProjectMembership] = useState<ProjectMembership>();
    const [newProjectRole, setNewProjectRole] = useState<ProjectMembershipRole>();
    const [newProjectProperties, setNewProjectProperties] = useState<Properties>();
    const [errorText, setErrorText] = useState<string>();

    useEffect(() => {
        if (props.project)
            setProjectMembership(props.user?.projectMemberships?.find(pm => pm.projectId === props.project!.id))
    }, [props.user, props.project]);

    useEffect(() => {
        if (projectMembership) {
            setNewProjectRole(projectMembership.role)
            const newProps: Properties = {}
            if (projectMembership.properties)
                for (const k in projectMembership.properties)
                    newProps[k] = projectMembership.properties[k]
            newProps[''] = ''
            setNewProjectProperties(newProps)
        }
    }, [projectMembership]);


    const _submitForm = async () => {
        if (props.user && props.project && projectMembership) {
            setFormEnabled(false);
            if ((newProjectRole && newProjectRole !== projectMembership.role)
                || (newProjectProperties && newProjectProperties !== projectMembership.properties)) {
                const newProps: Properties = {}
                for (const k in newProjectProperties)
                    if (k !== '' && newProjectProperties[k] !== '')
                        newProps[k] = newProjectProperties[k]

                const newProjectMembership: ProjectMembership = {
                    projectId: projectMembership.projectId,
                    role: newProjectRole ?? projectMembership.role,
                    properties: newProps
                }

                const index = props.user.projectMemberships?.findIndex(m => m.projectId === projectMembership.projectId)
                if (index !== undefined && index >= 0) {
                    props.user.projectMemberships![index] = newProjectMembership;
                    const result = await api.updateProjectUser(props.user.id, props.project.organization, props.project.id, { body: props.user });
                    if (result.code === 202)
                        _resetAndCloseForm();
                    else {
                        // console.log(JSON.stringify(result));
                        setErrorText(result.status ?? undefined);
                    }
                } else {
                    setErrorText('index not found')
                }
            } else {
                setErrorText('nothing changed')
            }
        } else {
            setErrorText('no props.member')
        }
    };

    const _resetAndCloseForm = () => {
        setNewProjectRole(undefined);
        setNewProjectProperties(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton text='Edit member' disabled={!formEnabled || !projectMembership} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    const _onPropertyKeyChange = (key: string, value: string, newKey?: string) => {
        const newProps: Properties = {}
        for (const k in newProjectProperties) newProps[(k === key) ? newKey ?? '' : k] = value
        if (!newProps['']) newProps[''] = ''
        setNewProjectProperties(newProps)
    }

    const _onPropertyValueChange = (key: string, newValue?: string) => {
        const newProps: Properties = {}
        for (const k in newProjectProperties) newProps[k] = (k === key) ? newValue ?? '' : newProjectProperties[k]
        setNewProjectProperties(newProps)
    }

    const _getPropertiesTextFields = () => {
        let propertyStacks = [];
        if (newProjectProperties) {
            let counter = 0
            for (const key in newProjectProperties) {
                propertyStacks.push(
                    <Stack key={counter} horizontal tokens={{ childrenGap: '8px' }}>
                        <TextField
                            description='Name'
                            value={key}
                            onChange={(_ev, val) => _onPropertyKeyChange(key, newProjectProperties[key], val)} />
                        <TextField
                            description='Value'
                            value={newProjectProperties[key]}
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

    const _roleDropdownDisabled = () => false;
    // {
    //     return !formEnabled || !projectMembership || (projectMembership.role.toLowerCase === 'owner'
    //         && props.project.users.filter(u => u.userType === 'User'
    //             && u.projectMemberships
    //             && u.projectMemberships!.find(pm => pm.projectId === props.project.id && pm.toLowerCase === 'owner')).length === 1)
    // };

    return (
        <Panel
            headerText='Edit Member'
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}
            onRenderFooterContent={_onRenderPanelFooterContent}>
            <Stack tokens={{ childrenGap: '12px' }}>
                <Stack.Item>
                    <UserPersona principal={props.graphUser} large />
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
                        disabled={_roleDropdownDisabled()}
                        selectedKey={newProjectRole}
                        options={['Member', 'Admin'].map(r => ({ key: r, text: r, data: r }))}
                        onChange={(_ev, val) => setNewProjectRole(val?.key ? val.key as ProjectMembershipRole : undefined)} />
                </Stack.Item>
                <Stack.Item>
                    <Label>Properties</Label>
                    {_getPropertiesTextFields()}
                </Stack.Item>
            </Stack>
            <Text>{errorText}</Text>
        </Panel>
    );
}
