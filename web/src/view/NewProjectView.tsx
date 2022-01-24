// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ISubmitEvent } from '@rjsf/core';
import { FuiForm } from '@rjsf/fluent-ui';
import { Stack, TextField, Dropdown, IDropdownOption, PrimaryButton, DefaultButton, IconButton } from '@fluentui/react';
import { ProjectTemplate, ProjectDefinition } from 'teamcloud';
import { ContentContainer, ContentHeader, ContentProgress } from '../components';
import { useCreateProject, useOrg, useProjectTemplates } from '../hooks';

export const NewProjectView: React.FC = () => {

    const navigate = useNavigate();

    const [projectName, setProjectName] = useState<string>();
    const [projectTemplate, setProjectTemplate] = useState<ProjectTemplate>();
    const [projectTemplateOptions, setProjectTemplateOptions] = useState<IDropdownOption[]>();
    const [formEnabled, setFormEnabled] = useState<boolean>(false);

    const { data: org } = useOrg();
    const { data: templates } = useProjectTemplates();

    const createProject = useCreateProject();

    useEffect(() => {
        if (org && templates) {
            // console.log(`+ setProjectTemplateOptions (${org.slug})`);
            const options = templates.map(t => ({ key: t.id, text: t.isDefault ? `${t.displayName} (default)` : t.displayName } as IDropdownOption));
            setProjectTemplateOptions(options);
            if (templates)
                setProjectTemplate(templates.find(t => t.isDefault));
        }
    }, [org, templates]);


    useEffect(() => {
        if (org && templates) {
            // console.log(`+ setFormEnabled (${org.slug})`);
            setFormEnabled(true);
        }
    }, [org, templates]);


    const _submitForm = async (e: ISubmitEvent<any>) => {
        setFormEnabled(false);

        if (org && projectName && projectTemplate && e.formData) {
            const projectDefinition: ProjectDefinition = {
                displayName: projectName,
                template: projectTemplate.id,
                templateInput: JSON.stringify(e.formData),
            };

            await createProject(projectDefinition)
        }
    };


    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setProjectTemplate((templates && option) ? templates.find(t => t.id === option.key) : undefined);
    };


    return (
        <Stack styles={{ root: { height: '100%' } }}>
            <ContentProgress progressHidden={formEnabled} />
            <ContentHeader title='New Project'>
                <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => navigate(`/orgs/${org?.slug}`)} />
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
                            <div style={{ paddingTop: '24px' }}>
                                <PrimaryButton type='submit' text='Create project' disabled={!formEnabled || !(projectName && projectTemplate)} styles={{ root: { marginRight: 8 } }} />
                                <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => navigate(`/orgs/${org?.slug}`)} />
                            </div>
                        </FuiForm>
                    </Stack.Item>
                </Stack>
            </ContentContainer>
        </Stack>
    );
}
