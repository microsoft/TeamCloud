// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Stack, TextField, Dropdown, IDropdownOption, Spinner, Text, PrimaryButton, DefaultButton, getTheme, IconButton, Pivot, PivotItem, ComboBox, ChoiceGroup } from '@fluentui/react';
import { ProjectTemplate, ProjectDefinition } from 'teamcloud';
import { ISubmitEvent } from '@rjsf/core';
import { FuiForm } from '@rjsf/fluent-ui';
// import { ProjectMemberPicker } from '.';
// import { GraphUser } from '../model'
import { api } from '../API';
import { AzureRegions } from '../model';


export interface INewOrganizationViewProps {
    // org?: Organization;
    // user?: User;
    // panelIsOpen: boolean;
    // onFormClose: () => void;
}

export const NewOrganizationView: React.FunctionComponent<INewOrganizationViewProps> = (props) => {

    let history = useHistory();

    let { orgId } = useParams() as { orgId: string };

    // const [org, setOrg] = useState(props.org);
    const [projectName, setProjectName] = useState<string>();
    const [region, setRegion] = useState<string>();
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

    // useEffect(() => {
    //     if (orgId && projectTemplates === undefined) {
    //         const _setProjectTemplates = async () => {
    //             const result = await api.getProjectTemplates(orgId);
    //             setProjectTemplates(result.data ?? undefined);
    //             setProjectTemplateOptions(_projectTemplateOptions(result.data ?? []));
    //         };
    //         _setProjectTemplates();
    //     }
    // }, [orgId, projectTemplates]);

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
            const result = await api.createProject(orgId, { body: projectDefinition });
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
        // props.onFormClose();
    };

    const _projectTemplateOptions = (data?: ProjectTemplate[]): IDropdownOption[] => {
        if (!data) return [];
        return data.map(pt => ({ key: pt.id, text: pt.displayName } as IDropdownOption));
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setProjectTemplate((projectTemplates && option) ? projectTemplates.find(pt => pt.id === option.key) : undefined);
    };

    // const _onMembersChanged = (users?: GraphUser[]) => {
    //     setUserIdentifiers(users?.map(u => u.id))
    // };

    const _onRenderPanelFooterContent = () => (
        <div style={{ paddingTop: '24px' }}>
            <PrimaryButton text='Create project' disabled={!formEnabled || !(projectName && projectTemplate)} styles={{ root: { marginRight: 8 } }} />
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
                        >New Organization</Text>
                    </Stack.Item>
                    <Stack.Item styles={{ root: { paddingRight: '12px' } }}>
                        <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.replace('/')} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item styles={{ root: { padding: '24px 44px' } }}>
                <Pivot>
                    <PivotItem headerText='Basic Settings'>
                        <Stack
                            tokens={{ childrenGap: '20px' }}
                            styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Subscription'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Resource group'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Name'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>

                            <Stack.Item>
                                <ComboBox
                                    required
                                    label='Location'
                                    allowFreeform
                                    autoComplete={'on'}
                                    selectedKey={region}
                                    options={AzureRegions.map(r => ({ key: r, text: r }))}
                                    onChange={(_ev, val) => setRegion(val?.text ?? undefined)} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Configuration'>
                        <Stack
                            tokens={{ childrenGap: '20px' }}
                            styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <ChoiceGroup
                                    required
                                    label='Web portal'
                                    options={[{ key: 'enabled', text: 'Enabled' }, { key: 'disabled', text: 'Disabled' }]} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Deployment Scope'>
                        <Stack
                            tokens={{ childrenGap: '20px' }}
                            styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Name'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Subscription'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Account'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Managaement Group'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Project Template'>
                        <Stack
                            tokens={{ childrenGap: '20px' }}
                            styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Name'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Url'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    label='Version'
                                    description='Branch/Tag/SHA'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    label='Token'
                                    description='Personal access token (required for private repos)'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Tags'>
                        <Stack
                            tokens={{ childrenGap: '20px' }}
                            styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Subscription'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Review + Create'>
                        <Stack
                            tokens={{ childrenGap: '20px' }}
                            styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Subscription'
                                    // errorMessage='Name is required.'
                                    disabled={!formEnabled}
                                    onChange={(ev, val) => setProjectName(val)} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                </Pivot>
            </Stack.Item>
            <Text>{errorText}</Text>
        </Stack>
    );
}
