// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { DefaultButton, Dropdown, IDropdownOption, PrimaryButton, Spinner, Stack } from '@fluentui/react';
import { ComponentRequest, ComponentTemplate } from 'teamcloud';
import { useHistory } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import { FuiForm } from '@rjsf/fluent-ui'
import { ISubmitEvent } from '@rjsf/core';
import { api } from '../API';
import { ProjectContext } from '../Context';

export const ComponentForm: React.FC = () => {

    const history = useHistory();

    const [template, setTemplate] = useState<ComponentTemplate>();
    const [templateOptions, setTemplateOptions] = useState<IDropdownOption[]>();
    const [formEnabled, setFormEnabled] = useState<boolean>(true);

    const { org, project, templates } = useContext(ProjectContext);

    useEffect(() => {
        if (project && templates) {
            console.log(`setProjectComponentTemplateOptions (${project.slug})`);
            setTemplateOptions(_templateOptions(templates));
        }
    }, [project, templates]);

    const _submitForm = async (e: ISubmitEvent<any>) => {
        setFormEnabled(false);

        if (org && project && template && e.formData) {
            const request: ComponentRequest = {
                templateId: template.id,
                inputJson: JSON.stringify(e.formData)
            };
            const result = await api.createProjectComponent(project.organization, project.id, { body: request });
            const component = result.data;

            if (component)
                history.push(`/orgs/${org.slug}/projects/${project.slug}/components`);
            else {
                console.error(result);
                // console.log(JSON.stringify(result));
                // setErrorText(result.status ?? 'unknown');
            }
        }
    };

    // const _resetAndCloseForm = () => {
    //     setComponentTemplate(undefined);
    //     setFormEnabled(true);
    //     // onFormClose();
    // };

    const _templateOptions = (data?: ComponentTemplate[]): IDropdownOption[] => {
        if (!data) return [];
        return data.map(t => ({ key: t.id, text: t.displayName ?? t.id } as IDropdownOption));
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setTemplate((templates && option) ? templates.find(t => t.id === option.key) : undefined);
    };

    const _onRenderPanelFooterContent = () => (
        <div style={{ paddingTop: '24px' }}>
            <PrimaryButton type='submit' text='Create component' disabled={!formEnabled || !(template)} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => history.push(`/orgs/${org?.slug}/projects/${project?.slug}`)} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );


    return (
        <Stack
            tokens={{ childrenGap: '20px' }}>
            <Stack.Item>
                <Dropdown
                    required
                    label='Template'
                    disabled={!formEnabled}
                    options={templateOptions || []}
                    onChange={_onDropdownChange} />
            </Stack.Item>
            <Stack.Item>
                <ReactMarkdown>{template?.description ?? undefined as any}</ReactMarkdown>
            </Stack.Item>
            <Stack.Item>
                <FuiForm
                    disabled={!formEnabled}
                    onSubmit={_submitForm}
                    schema={template?.inputJsonSchema ? JSON.parse(template.inputJsonSchema) : {}}>
                    {_onRenderPanelFooterContent()}
                </FuiForm>
            </Stack.Item>
        </Stack>
    );
}
