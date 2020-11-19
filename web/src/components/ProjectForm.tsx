// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Stack, TextField, Dropdown, IDropdownOption, Spinner, Panel, Text, PrimaryButton, DefaultButton, Label } from '@fluentui/react';
import { ProjectTemplate, User, ProjectDefinition, ProjectMembershipRole, UserDefinition, Organization } from 'teamcloud';
import { ProjectMemberPicker } from '.';
import { GraphUser } from '../model'
import { api } from '../API';


export interface IProjectFormProps {
    org?: Organization;
    user?: User;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const ProjectForm: React.FunctionComponent<IProjectFormProps> = (props) => {

    const [projectName, setProjectName] = useState<string>();
    const [projectTemplate, setProjectTemplate] = useState<ProjectTemplate>();
    const [projectTemplates, setProjectTemplates] = useState<ProjectTemplate[]>();
    const [projectTemplateOptions, setProjectTemplateOptions] = useState<IDropdownOption[]>();
    const [userIdentifiers, setUserIdentifiers] = useState<string[]>();
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [errorText, setErrorText] = useState<string>();

    useEffect(() => {
        if (projectTemplates === undefined) {
            const _setProjectTemplates = async () => {
                if (props.org) {
                    const result = await api.getProjectTemplates(props.org.id);
                    setProjectTemplates(result.data ?? undefined);
                    setProjectTemplateOptions(_projectTemplateOptions(result.data ?? []));
                }
            };
            _setProjectTemplates();
        }
    }, [props.org, projectTemplates]);

    const _submitForm = async () => {
        setFormEnabled(false);
        if (props.user && props.org && projectName && projectTemplate) {
            let userDefinitions: UserDefinition[] = [{ identifier: props.user.id, role: 'Owner' as ProjectMembershipRole }];
            if (userIdentifiers?.length && userIdentifiers.length > 0) {
                userDefinitions = userDefinitions.concat(userIdentifiers.map(i => ({
                    identifier: i,
                    role: 'Member' as ProjectMembershipRole
                })));
            }
            const projectDefinition: ProjectDefinition = {
                displayName: projectName,
                template: projectTemplate.id,
                templateInput: '',
                users: userDefinitions
            };
            const result = await api.createProject(props.org.id, { body: projectDefinition });
            if (result.code === 202)
                _resetAndCloseForm();
            else {
                // console.log(JSON.stringify(result));
                setErrorText(result.status ?? undefined);
            }
        }
    };

    const _resetAndCloseForm = () => {
        setProjectName(undefined);
        setProjectTemplate(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

    const _projectTemplateOptions = (data?: ProjectTemplate[]): IDropdownOption[] => {
        if (!data) return [];
        return data.map(pt => ({ key: pt.id, text: pt.id } as IDropdownOption));
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setProjectTemplate((projectTemplates && option) ? projectTemplates.find(pt => pt.id === option.key) : undefined);
    };

    const _onMembersChanged = (users?: GraphUser[]) => {
        setUserIdentifiers(users?.map(u => u.id))
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton text='Create project' disabled={!formEnabled || !(projectName && projectTemplate)} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
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
                        options={projectTemplateOptions || []}
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
