// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Stack, TextField, Dropdown, IDropdownOption, Spinner, Text, PrimaryButton, DefaultButton, getTheme, IconButton } from '@fluentui/react';
import { ProjectTemplate, ProjectDefinition } from 'teamcloud';
import { ISubmitEvent } from '@rjsf/core';
import { FuiForm } from '@rjsf/fluent-ui';
// import { ProjectMemberPicker } from '.';
// import { GraphUser } from '../model'
import { api } from '../API';


export interface INewProjectViewProps {
    // org?: Organization;
    // user?: User;
    // panelIsOpen: boolean;
    // onFormClose: () => void;
}

export const NewProjectView: React.FunctionComponent<INewProjectViewProps> = (props) => {

    let history = useHistory();
    let { orgId } = useParams() as { orgId: string };

    // const [org, setOrg] = useState(props.org);
    const [projectName, setProjectName] = useState<string>();
    const [projectTemplate, setProjectTemplate] = useState<ProjectTemplate>();
    const [projectTemplates, setProjectTemplates] = useState<ProjectTemplate[]>();
    const [projectTemplateOptions, setProjectTemplateOptions] = useState<IDropdownOption[]>();
    // const [userIdentifiers, setUserIdentifiers] = useState<string[]>();
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [errorText, setErrorText] = useState<string>();


    // useEffect(() => {
    //     if (project === undefined) {
    //         const _setProject = async () => {
    //             const result = await api.getProject(projectId, orgId);
    //             setProject(result.data);
    //         };
    //         _setProject();
    //     }
    // }, [project, projectId, orgId]);

    useEffect(() => {
        if (orgId && projectTemplates === undefined) {
            const _setProjectTemplates = async () => {
                const result = await api.getProjectTemplates(orgId);
                setProjectTemplates(result.data ?? undefined);
                setProjectTemplateOptions(_projectTemplateOptions(result.data ?? []));
                if (result.data && !projectTemplate) {
                    setProjectTemplate(result.data.find(t => t.isDefault));
                }
            };
            _setProjectTemplates();
        }
    }, [orgId, projectTemplates]);

    const _submitForm = async (e: ISubmitEvent<any>) => {
        setFormEnabled(false);


        if (orgId && projectName && projectTemplate && e.formData) {
            // let userDefinitions: UserDefinition[] = [{ identifier: props.user.id, role: 'Owner' as ProjectMembershipRole }];
            // if (userIdentifiers?.length && userIdentifiers.length > 0) {
            //     userDefinitions = userDefinitions.concat(userIdentifiers.map(i => ({
            //         identifier: i,
            //         role: 'Member' as ProjectMembershipRole
            //     })));
            // }
            const projectDefinition: ProjectDefinition = {
                displayName: projectName,
                template: projectTemplate.id,
                templateInput: JSON.stringify(e.formData),
                // users: userDefinitions
            };
            const projectResult = await api.createProject(orgId, { body: projectDefinition });
            const project = projectResult.data;

            if (project)
                history.push(`/orgs/${orgId}/projects/${project.slug}`);
            else {
                // console.log(JSON.stringify(result));
                console.error(projectResult)
                setErrorText(projectResult.status ?? 'failed to create project');
            }
        }
    };

    const _resetAndCloseForm = () => {
        setProjectName(undefined);
        setProjectTemplate(undefined);
        setFormEnabled(true);
        // props.onFormClose();
    };

    const _projectTemplateOptions = (data?: ProjectTemplate[]): IDropdownOption[] => {
        if (!data) return [];
        return data.map(pt => ({ key: pt.id, text: pt.isDefault ? `${pt.displayName} (default)` : pt.displayName } as IDropdownOption));
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setProjectTemplate((projectTemplates && option) ? projectTemplates.find(pt => pt.id === option.key) : undefined);
    };

    // const _onMembersChanged = (users?: GraphUser[]) => {
    //     setUserIdentifiers(users?.map(u => u.id))
    // };

    const _onRenderPanelFooterContent = () => (
        <div style={{ paddingTop: '24px' }}>
            <PrimaryButton type='submit' text='Create project' disabled={!formEnabled || !(projectName && projectTemplate)} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    const theme = getTheme();

    return (
        <Stack>
            {/* <Stack.Item styles={{ root: { padding: '24px 20px 0px 32px' } }}> */}
            <Stack.Item styles={{ root: { margin: '0px', padding: '24px 30px 20px 32px', backgroundColor: theme.palette.white, borderBottom: `${theme.palette.neutralLight} solid 1px` } }}>
                <Stack horizontal
                    verticalFill
                    horizontalAlign='space-between'
                    verticalAlign='baseline'>
                    <Stack.Item>
                        <Text
                            styles={{ root: { fontSize: theme.fonts.xxLarge.fontSize, fontWeight: '700', letterSpacing: '-1.12px', marginLeft: '12px' } }}
                        >New Project</Text>
                    </Stack.Item>
                    <Stack.Item styles={{ root: { paddingRight: '8px' } }}>
                        <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.replace(`/orgs/${orgId}`)} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item styles={{ root: { padding: '24px 44px' } }}>
                <Stack
                    tokens={{ childrenGap: '20px' }}>
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
                            label='Project Template'
                            // errorMessage='Project Type is required.'
                            // placeholder='Select a Project Type'
                            selectedKey={projectTemplate?.id}
                            disabled={!formEnabled}
                            options={projectTemplateOptions || []}
                            onChange={_onDropdownChange} />
                    </Stack.Item>
                    <Stack.Item>
                        <FuiForm
                            disabled={!formEnabled}
                            onSubmit={_submitForm}
                            schema={projectTemplate?.inputJsonSchema ? JSON.parse(projectTemplate.inputJsonSchema) : {}}>
                            {_onRenderPanelFooterContent()}
                        </FuiForm>
                    </Stack.Item>
                    {/* <Stack.Item>
                    <Label>Members</Label>
                    <ProjectMemberPicker
                        formEnabled={formEnabled}
                        onChange={_onMembersChanged} />
                </Stack.Item> */}

                </Stack>
            </Stack.Item>
            <Text>{errorText}</Text>
        </Stack>
    );
}
