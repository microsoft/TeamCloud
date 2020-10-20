// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Stack, TextField, Dropdown, IDropdownOption, Spinner, Panel, Text, PrimaryButton, DefaultButton, Label } from '@fluentui/react';
import { ProjectType, User, ProjectDefinition, ProjectMembershipRole, UserDefinition } from 'teamcloud';
import { ProjectMemberPicker } from '.';
import { GraphUser } from '../model'
import { api } from '../API';


export interface IProjectFormProps {
    user?: User;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const ProjectForm: React.FunctionComponent<IProjectFormProps> = (props) => {

    const [projectName, setProjectName] = useState<string>();
    const [projectType, setProjectType] = useState<ProjectType>();
    const [projectTypes, setProjectTypes] = useState<ProjectType[]>();
    const [projectTypeOptions, setProjectTypeOptions] = useState<IDropdownOption[]>();
    const [userIdentifiers, setUserIdentifiers] = useState<string[]>();
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [errorText, setErrorText] = useState<string>();

    useEffect(() => {
        if (projectTypes === undefined) {
            const _setProjectTypes = async () => {
                const result = await api.getProjectTypes();
                setProjectTypes(result.data);
                setProjectTypeOptions(_projectTypeOptions(result.data));
            };
            _setProjectTypes();
        }
    }, [projectTypes]);

    const _submitForm = async () => {
        setFormEnabled(false);
        if (props.user && projectName && projectType) {
            let userDefinitions: UserDefinition[] = [{ identifier: props.user.id, role: 'Owner' as ProjectMembershipRole }];
            if (userIdentifiers?.length && userIdentifiers.length > 0) {
                userDefinitions = userDefinitions.concat(userIdentifiers.map(i => ({
                    identifier: i,
                    role: 'Member' as ProjectMembershipRole
                })));
            }
            const projectDefinition: ProjectDefinition = {
                name: projectName,
                projectType: projectType.id,
                users: userDefinitions
            };
            const result = await api.createProject({ body: projectDefinition });
            if (result.code === 202)
                _resetAndCloseForm();
            else {
                // console.log(JSON.stringify(result));
                setErrorText(result.status);
            }
        }
    };

    const _resetAndCloseForm = () => {
        setProjectName(undefined);
        setProjectType(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

    const _projectTypeOptions = (data?: ProjectType[]): IDropdownOption[] => {
        if (!data) return [];
        return data.map(pt => ({ key: pt.id, text: pt.id } as IDropdownOption));
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setProjectType((projectTypes && option) ? projectTypes.find(pt => pt.id === option.key) : undefined);
    };

    const _onMembersChanged = (users?: GraphUser[]) => {
        setUserIdentifiers(users?.map(u => u.id))
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton text='Create project' disabled={!formEnabled || !(projectName && projectType)} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    return (
        <Panel
            headerText='New Project'
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}
            onRenderFooterContent={_onRenderPanelFooterContent}>
            <Stack tokens={{ childrenGap: '12px' }}>
                <Stack.Item>
                    <TextField
                        required
                        label='Name'
                        // errorMessage='Name is required.'
                        disabled={!formEnabled}
                        onChange={(ev, val) => setProjectName(val)} />
                </Stack.Item>
                <Stack.Item>
                    <Dropdown
                        required
                        label='Project Type'
                        // errorMessage='Project Type is required.'
                        // placeholder='Select a Project Type'
                        disabled={!formEnabled}
                        options={projectTypeOptions || []}
                        onChange={_onDropdownChange} />
                </Stack.Item>
                <Stack.Item>
                    <Label>Members</Label>
                    <ProjectMemberPicker
                        formEnabled={formEnabled}
                        onChange={_onMembersChanged} />
                </Stack.Item>
            </Stack>
            <Text>{errorText}</Text>
        </Panel>
    );
}
