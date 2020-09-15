// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { ProjectUserRole, ProjectMember, Project, UserType, Properties } from '../model';
import { PrimaryButton, DefaultButton, Panel, Stack, TextField, Dropdown, Label, Spinner, Persona, PersonaSize } from '@fluentui/react';

export interface IProjectMemberFormProps {
    // user: User;
    member?: ProjectMember;
    project: Project;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const ProjectMemberForm: React.FunctionComponent<IProjectMemberFormProps> = (props) => {

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [projectRole, setProjectRole] = useState<ProjectUserRole>();
    const [projectProperties, setProjectProperties] = useState<Properties>();

    useEffect(() => {
        setProjectRole(props.member?.projectMembership.role)
        const newProps: Properties = {}
        if (props.member?.projectMembership.properties)
            for (const k in props.member.projectMembership.properties)
                newProps[k] = props.member.projectMembership.properties[k]
        newProps[''] = ''
        setProjectProperties(newProps)
    }, [props.member]);


    const _submitForm = async () => {
        if (props.member) {
            setFormEnabled(false);
            if ((projectRole && projectRole !== props.member.projectMembership.role)
                || (projectProperties && projectProperties !== props.member.projectMembership.properties)) {
                // const result = await updateProjectUser(projectDefinition);
                // if ((result as StatusResult).code === 202)
                //     _resetAndCloseForm();
                // else if ((result as ErrorResult).errors) {
                //     // console.log(JSON.stringify(result));
                //     setErrorText((result as ErrorResult).status);
                // }

            }
        }
    };

    const _resetAndCloseForm = () => {
        setProjectRole(undefined);
        setProjectProperties(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton text='Edit member' disabled={!formEnabled} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    const _onPropertyKeyChange = (key: string, value: string, newKey?: string) => {
        const newProps: Properties = {}
        for (const k in projectProperties) newProps[(k === key) ? newKey ?? '' : k] = value
        if (!newProps['']) newProps[''] = ''
        setProjectProperties(newProps)
    }

    const _onPropertyValueChange = (key: string, newValue?: string) => {
        const newProps: Properties = {}
        for (const k in projectProperties) newProps[k] = (k === key) ? newValue ?? '' : projectProperties[k]
        setProjectProperties(newProps)
    }

    const _getPropertiesTextFields = () => {
        let propertyStacks = [];
        if (projectProperties) {
            let counter = 0
            for (const key in projectProperties) {
                propertyStacks.push(
                    <Stack key={counter} horizontal tokens={{ childrenGap: '8px' }}>
                        <TextField
                            description='Name'
                            value={key}
                            onChange={(_ev, val) => _onPropertyKeyChange(key, projectProperties[key], val)} />
                        <TextField
                            description='Value'
                            value={projectProperties[key]}
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

    const _roleDropdownDisabled = () => {
        return !formEnabled || (props.member?.projectMembership.role === ProjectUserRole.Owner
            && props.project.users.filter(u => u.userType === UserType.User
                && u.projectMemberships
                && u.projectMemberships!.find(pm => pm.projectId === props.project.id && pm.role === ProjectUserRole.Owner)).length === 1)
    };

    return (
        <Panel
            headerText='Edit Member'
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}
            onRenderFooterContent={_onRenderPanelFooterContent}>
            <Stack tokens={{ childrenGap: '12px' }}>
                <Stack.Item>
                    <Persona
                        text={props.member?.graphUser?.displayName ?? props.member?.user.id}
                        secondaryText={props.member?.graphUser?.jobTitle ?? props.member?.user.userType}
                        tertiaryText={props.member?.graphUser?.department}
                        imageUrl={props.member?.graphUser?.imageUrl}
                        size={PersonaSize.size72} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        readOnly
                        label='Id'
                        value={props.member?.user.id} />
                </Stack.Item>
                <Stack.Item>
                    <Dropdown
                        required
                        label='Role'
                        // errorMessage='Project Type is required.'
                        // placeholder='Select a Project Type'
                        disabled={_roleDropdownDisabled()}
                        selectedKey={projectRole}
                        // defaultSelectedKey={projectRole as string}
                        options={[ProjectUserRole.Owner, ProjectUserRole.Member].map(r => ({ key: r, text: r, data: r }))}
                        onChange={(_ev, val) => setProjectRole(val?.key ? ProjectUserRole[val.key as keyof typeof ProjectUserRole] : undefined)} />
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
