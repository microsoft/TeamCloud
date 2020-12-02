// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { DefaultButton, Dropdown, IconButton, IDropdownOption, PrimaryButton, Spinner, Stack, Text } from '@fluentui/react';
import { ComponentRequest, ComponentTemplate, Organization, Project } from 'teamcloud';
import { useHistory } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import { FuiForm } from '@rjsf/fluent-ui'
import { ISubmitEvent } from '@rjsf/core';
import { api } from '../API';
import { ContentContainer, ContentHeader, ContentProgress } from '../components';

export interface INewComponentViewProps {
    org?: Organization;
    project?: Project;
}

export const NewComponentView: React.FC<INewComponentViewProps> = (props) => {

    const history = useHistory();

    const [componentTemplate, setComponentTemplate] = useState<ComponentTemplate>();
    const [componentTemplates, setComponentTemplates] = useState<ComponentTemplate[]>();
    const [componentTemplateOptions, setComponentTemplateOptions] = useState<IDropdownOption[]>();
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [errorText, setErrorText] = useState<string>();

    const { org, project } = props;

    useEffect(() => {
        if (project && componentTemplates === undefined) {
            const _setComponentTemplates = async () => {
                const result = await api.getProjectComponentTemplates(project.organization, project.id);
                setComponentTemplates(result.data ?? undefined);
                setComponentTemplateOptions(_componentTemplateOptions(result.data ?? undefined));
            };
            _setComponentTemplates();
        }
    }, [project, componentTemplates]);

    const _submitForm = async (e: ISubmitEvent<any>) => {
        setFormEnabled(false);

        if (org && project && componentTemplate && e.formData) {
            const request: ComponentRequest = {
                templateId: componentTemplate.id,
                inputJson: JSON.stringify(e.formData)
            };
            const result = await api.createProjectComponent(project.organization, project.id, { body: request });
            const component = result.data;

            if (component)
                history.push(`/orgs/${org.slug}/projects/${project.slug}/components`);
            else {
                // console.log(JSON.stringify(result));
                setErrorText(result.status ?? 'unknown');
            }
        }
    };

    // const _resetAndCloseForm = () => {
    //     setComponentTemplate(undefined);
    //     setFormEnabled(true);
    //     // props.onFormClose();
    // };

    const _componentTemplateOptions = (data?: ComponentTemplate[]): IDropdownOption[] => {
        if (!data) return [];
        return data.map(ct => ({ key: ct.id, text: ct.displayName ?? ct.id } as IDropdownOption));
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setComponentTemplate((componentTemplates && option) ? componentTemplates.find(ct => ct.id === option.key) : undefined);
    };

    const _onRenderPanelFooterContent = () => (
        <div style={{ paddingTop: '24px' }}>
            <PrimaryButton type='submit' text='Create component' disabled={!formEnabled || !(componentTemplate)} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => history.push(`/orgs/${props.org?.slug}/projects/${props.project?.slug}`)} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );


    return (
        <Stack styles={{ root: { height: '100%' } }}>
            <ContentProgress progressHidden={formEnabled} />
            <ContentHeader title='New Component'>
                <IconButton
                    iconProps={{ iconName: 'ChromeClose' }}
                    onClick={() => history.push(`/orgs/${props.org?.slug}/projects/${props.project?.slug}`)} />
            </ContentHeader>
            <ContentContainer wide full>
                <Stack
                    tokens={{ childrenGap: '20px' }}>
                    <Stack.Item>
                        <Dropdown
                            required
                            label='Template'
                            disabled={!formEnabled}
                            options={componentTemplateOptions || []}
                            onChange={_onDropdownChange} />
                    </Stack.Item>
                    <Stack.Item>
                        <ReactMarkdown>{componentTemplate?.description ?? undefined as any}</ReactMarkdown>
                    </Stack.Item>
                    <Stack.Item>
                        <FuiForm
                            disabled={!formEnabled}
                            onSubmit={_submitForm}
                            schema={componentTemplate?.inputJsonSchema ? JSON.parse(componentTemplate.inputJsonSchema) : {}}>
                            {_onRenderPanelFooterContent()}
                        </FuiForm>
                    </Stack.Item>
                </Stack>
            </ContentContainer>
            <Text>{errorText}</Text>
        </Stack>
    );
}
