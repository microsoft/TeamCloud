// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Project, ComponentRequest, ComponentTemplate } from 'teamcloud';
import { DefaultButton, Dropdown, IDropdownOption, Panel, PrimaryButton, Spinner, Stack, Text } from '@fluentui/react';
import { FuiForm } from '@rjsf/fluent-ui'
// import { JSONSchema7 } from 'json-schema'
import { ISubmitEvent } from '@rjsf/core';
import { api } from '../API';
import ReactMarkdown from 'react-markdown';

export interface IProjectComponentFormProps {
    // user?: User;
    project: Project;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const ProjectComponentForm: React.FunctionComponent<IProjectComponentFormProps> = (props) => {
    // return (<></>);
    const [componentTemplate, setComponentTemplate] = useState<ComponentTemplate>();
    const [componentTemplates, setComponentTemplates] = useState<ComponentTemplate[]>();
    const [componentTemplateOptions, setComponentTemplateOptions] = useState<IDropdownOption[]>();
    // const [offerInputSchema, setOfferInputSchema] = useState<JSONSchema7>();
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [errorText, setErrorText] = useState<string>();
    // const [components, setComponents] = useState<Component[]>();
    // const [addComponentPanelOpen, setAddComponentPanelOpen] = useState(false);

    useEffect(() => {
        if (props.project) {
            const _setComponentTemplates = async () => {
                const result = await api.getProjectComponentTemplates(props.project.organization, props.project.id);
                setComponentTemplates(result.data ?? undefined);
                setComponentTemplateOptions(_componentTemplateOptions(result.data ?? undefined));
            };
            _setComponentTemplates();
        }
    }, [props.project]);

    const _submitForm = async (e: ISubmitEvent<any>) => {
        setFormEnabled(false);

        if (componentTemplate && e.formData) {
            const request: ComponentRequest = {
                templateId: componentTemplate.id,
                inputJson: JSON.stringify(e.formData)
            };
            const result = await api.createProjectComponent(props.project.organization, props.project.id, { body: request });
            if (result.code === 202)
                _resetAndCloseForm();
            else {
                // console.log(JSON.stringify(result));
                setErrorText(result.status ?? 'unknown');
            }
        }
    };

    const _resetAndCloseForm = () => {
        setComponentTemplate(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

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
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );


    return (
        <Panel
            headerText='New Component'
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}>
            <Stack tokens={{ childrenGap: '12px' }}>
                <Stack.Item>
                    <Dropdown
                        required
                        label='Offer'
                        disabled={!formEnabled}
                        options={componentTemplateOptions || []}
                        onChange={_onDropdownChange} />
                </Stack.Item>
                <Stack.Item>
                    <FuiForm
                        disabled={!formEnabled}
                        onSubmit={_submitForm}
                        schema={componentTemplate?.inputJsonSchema ? JSON.parse(componentTemplate.inputJsonSchema) : {}}>
                        {_onRenderPanelFooterContent()}
                    </FuiForm>
                </Stack.Item>
                <Stack.Item>
                    <ReactMarkdown>{componentTemplate?.description ?? undefined as any}</ReactMarkdown>
                </Stack.Item>
            </Stack>
            <Text>{errorText}</Text>
        </Panel>
    );
}
