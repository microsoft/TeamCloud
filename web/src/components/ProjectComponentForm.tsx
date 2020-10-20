// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Project, ComponentOffer, ComponentRequest, User } from 'teamcloud';
import { DefaultButton, Dropdown, IDropdownOption, Panel, PrimaryButton, Spinner, Stack, Text } from '@fluentui/react';
import { FuiForm } from '@rjsf/fluent-ui'
// import { JSONSchema7 } from 'json-schema'
import { ISubmitEvent } from '@rjsf/core';
import { api } from '../API';

export interface IProjectComponentFormProps {
    user?: User;
    project: Project;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const ProjectComponentForm: React.FunctionComponent<IProjectComponentFormProps> = (props) => {

    const [offer, setOffer] = useState<ComponentOffer>();
    const [offers, setOffers] = useState<ComponentOffer[]>();
    const [offerOptions, setOfferOptions] = useState<IDropdownOption[]>();
    // const [offerInputSchema, setOfferInputSchema] = useState<JSONSchema7>();
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [errorText, setErrorText] = useState<string>();
    // const [components, setComponents] = useState<Component[]>();
    // const [addComponentPanelOpen, setAddComponentPanelOpen] = useState(false);

    useEffect(() => {
        if (props.project) {
            const _setOffers = async () => {
                const result = await api.getProjectOffers(props.project.id!);
                setOffers(result.data);
                setOfferOptions(_offerOptions(result.data));
            };
            _setOffers();
        }
    }, [props.project]);

    const _submitForm = async (e: ISubmitEvent<any>) => {
        setFormEnabled(false);

        if (offer && e.formData) {
            const request: ComponentRequest = {
                offerId: offer.id,
                inputJson: JSON.stringify(e.formData)
            };
            const result = await api.createProjectComponent(props.project.id, { body: request });
            if (result.code === 202)
                _resetAndCloseForm();
            else {
                // console.log(JSON.stringify(result));
                setErrorText(result.status);
            }
        }
    };

    const _resetAndCloseForm = () => {
        setOffer(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

    const _offerOptions = (data?: ComponentOffer[]): IDropdownOption[] => {
        if (!data) return [];
        return data.map(pt => ({ key: pt.id, text: pt.id } as IDropdownOption));
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setOffer((offers && option) ? offers.find(pt => pt.id === option.key) : undefined);
    };

    const _onRenderPanelFooterContent = () => (
        <div style={{ paddingTop: '24px' }}>
            <PrimaryButton type='submit' text='Create component' disabled={!formEnabled || !(offer)} styles={{ root: { marginRight: 8 } }} />
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
                        options={offerOptions || []}
                        onChange={_onDropdownChange} />
                </Stack.Item>
                <Stack.Item>
                    <FuiForm
                        disabled={!formEnabled}
                        onSubmit={_submitForm}
                        schema={offer?.inputJsonSchema ? JSON.parse(offer.inputJsonSchema) : {}}>
                        {_onRenderPanelFooterContent()}
                    </FuiForm>
                </Stack.Item>
            </Stack>
            <Text>{errorText}</Text>
        </Panel>
    );
}
