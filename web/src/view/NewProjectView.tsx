// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { ISubmitEvent } from '@rjsf/core';
import { FuiForm } from '@rjsf/fluent-ui';
import { Stack, TextField, Dropdown, IDropdownOption, Text, PrimaryButton, DefaultButton, IconButton } from '@fluentui/react';
import { ProjectTemplate, ProjectDefinition } from 'teamcloud';
import { ContentContainer, ContentHeader, ContentProgress } from '../components';
import { api } from '../API';


export interface INewProjectViewProps { }

export const NewProjectView: React.FC<INewProjectViewProps> = (props) => {

    let history = useHistory();
    let { orgId } = useParams() as { orgId: string };

    const [projectName, setProjectName] = useState<string>();
    const [projectTemplate, setProjectTemplate] = useState<ProjectTemplate>();
    const [projectTemplates, setProjectTemplates] = useState<ProjectTemplate[]>();
    const [projectTemplateOptions, setProjectTemplateOptions] = useState<IDropdownOption[]>();
    const [formEnabled, setFormEnabled] = useState<boolean>(false);
    const [errorText, setErrorText] = useState<string>();


    useEffect(() => {
        if (orgId && projectTemplates === undefined) {
            const _setProjectTemplates = async () => {
                const result = await api.getProjectTemplates(orgId);
                setProjectTemplates(result.data ?? undefined);
                setProjectTemplateOptions(_projectTemplateOptions(result.data ?? []));
                if (result.data && !projectTemplate) {
                    setProjectTemplate(result.data.find(t => t.isDefault));
                }
                setFormEnabled(true);
            };
            _setProjectTemplates();
        }
    }, [orgId, projectTemplates, projectTemplate]);

    const _submitForm = async (e: ISubmitEvent<any>) => {
        setFormEnabled(false);


        if (orgId && projectName && projectTemplate && e.formData) {
            const projectDefinition: ProjectDefinition = {
                displayName: projectName,
                template: projectTemplate.id,
                templateInput: JSON.stringify(e.formData),
            };
            const projectResult = await api.createProject(orgId, { body: projectDefinition });
            const project = projectResult.data;

            if (project)
                history.push(`/orgs/${orgId}/projects/${project.slug}`);
            else {
                console.error(projectResult)
                setErrorText(projectResult.status ?? 'failed to create project');
            }
        }
    };

    const _resetAndCloseForm = () => {
        setProjectName(undefined);
        setProjectTemplate(undefined);
        setFormEnabled(true);
    };

    const _projectTemplateOptions = (data?: ProjectTemplate[]): IDropdownOption[] => {
        if (!data) return [];
        return data.map(pt => ({ key: pt.id, text: pt.isDefault ? `${pt.displayName} (default)` : pt.displayName } as IDropdownOption));
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setProjectTemplate((projectTemplates && option) ? projectTemplates.find(pt => pt.id === option.key) : undefined);
    };

    const _onRenderPanelFooterContent = () => (
        <div style={{ paddingTop: '24px' }}>
            <PrimaryButton type='submit' text='Create project' disabled={!formEnabled || !(projectName && projectTemplate)} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
        </div>
    );

    return (
        <Stack styles={{ root: { height: '100%' } }}>
            <ContentProgress progressHidden={formEnabled} />
            <ContentHeader title='New Project'>
                <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.replace(`/orgs/${orgId}`)} />
            </ContentHeader>
            <ContentContainer wide full>
                <Stack
                    tokens={{ childrenGap: '20px' }}>
                    <Stack.Item>
                        <TextField
                            required
                            label='Name'
                            disabled={!formEnabled}
                            onChange={(ev, val) => setProjectName(val)} />
                    </Stack.Item>
                    <Stack.Item>
                        <Dropdown
                            required
                            label='Project Template'
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
                </Stack>
            </ContentContainer>
            <Text>{errorText}</Text>
        </Stack>
    );
}
