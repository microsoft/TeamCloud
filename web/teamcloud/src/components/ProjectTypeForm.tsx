// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from "react";
import { Stack, TextField, Spinner, DefaultButton, IButtonStyles, getTheme, Image, ButtonType, Text, Label, Checkbox, SpinButton } from "@fluentui/react";
import { Position } from 'office-ui-fabric-react/lib/utilities/positioning';
import { ProjectType, Provider, DataResult } from "../model";
import { getProviders } from "../API";
import AppInsights from '../img/appinsights.svg';
import DevOps from '../img/devops.svg';
import DevTestLabs from '../img/devtestlabs.svg';
import GitHub from '../img/github.svg';

export interface IProjectTypeFormProps {
    fieldsEnabled: boolean;
    onFormSubmit: () => void;
    onNameChange: (val: string | undefined) => void;
    // onProjectTypeChange: (val: ProjectType | undefined) => void;
}

export const ProjectTypeForm: React.FunctionComponent<IProjectTypeFormProps> = (props) => {

    const [providers, setProviders] = useState<Provider[]>();

    useEffect(() => {
        if (providers === undefined) {
            const _setProviders = async () => {
                const result = await getProviders()
                const data = (result as DataResult<Provider[]>).data;
                setProviders(data);
            };
            _setProviders();
        }
    }, [providers]);

    const _findKnownProviderName = (provider: Provider) => {
        if (provider.id) {
            if (provider.id.startsWith('azure.appinsights')) return 'AppInsights';
            if (provider.id.startsWith('azure.devops')) return 'DevOps';
            if (provider.id.startsWith('azure.devtestlabs')) return 'DevTestLabs';
            if (provider.id.startsWith('github')) return 'GitHub';
        }
        return undefined;
    }

    const _findKnownProviderImage = (provider: Provider) => {
        if (provider.id) {
            if (provider.id.startsWith('azure.appinsights')) return AppInsights;
            if (provider.id.startsWith('azure.devops')) return DevOps;
            if (provider.id.startsWith('azure.devtestlabs')) return DevTestLabs;
            if (provider.id.startsWith('github')) return GitHub;
        }
        return provider.id;
    }

    const theme = getTheme();

    const _providerButtonStyles: IButtonStyles = {
        root: {
            border: 'none',
            height: '100%',
            width: '100%',
            borderWidth: '1px',
            borderStyle: 'solid',
            borderColor: theme.palette.neutralLighter,
            padding: '8px 20px'
        }
    }
    const _getProviderButtons = () => {
        let buttons = providers?.map(p => (
            <Stack.Item styles={{ root: { width: '46%' } }}>
                <DefaultButton
                    buttonType={ButtonType.icon}
                    styles={_providerButtonStyles}>
                    <Stack horizontalAlign='center' tokens={{ padding: '10px', childrenGap: '6px' }}>
                        <Image
                            src={_findKnownProviderImage(p)}
                            height={48} width={48} />
                        <Text>{_findKnownProviderName(p)}</Text>
                    </Stack>
                </DefaultButton>
            </Stack.Item >
        ));
        return (
            <Stack horizontal wrap tokens={{ childrenGap: '12px' }}>
                {buttons}
            </Stack>
        );
    }

    if (providers) {
        return (
            <Stack tokens={{ childrenGap: '12px' }}>
                <Stack.Item>
                    <Label required>Providers</Label>
                    {_getProviderButtons()}
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        label='Name'
                        required
                        // errorMessage='Name is required.'
                        disabled={props.fieldsEnabled}
                        onChange={(ev, val) => props.onNameChange(val)} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        label='Subscriptions'
                        required
                        // errorMessage='Name is required.'
                        disabled={props.fieldsEnabled}
                        onChange={(ev, val) => props.onNameChange(val)} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        label='Subscriptions'
                        required
                        // errorMessage='Name is required.'
                        disabled={props.fieldsEnabled}
                        onChange={(ev, val) => props.onNameChange(val)} />
                </Stack.Item>
                <Stack.Item>
                    <SpinButton
                        label='Subscription capacity'
                        labelPosition={Position.top}
                        defaultValue='10'
                        min={1}
                        max={50}
                        step={1}
                        incrementButtonAriaLabel={'Increase value by 1'}
                        decrementButtonAriaLabel={'Decrease value by 1'} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        label='Resource Group name prefix'
                        // errorMessage='Name is required.'
                        disabled={props.fieldsEnabled}
                        onChange={(ev, val) => props.onNameChange(val)} />
                </Stack.Item>
                <Stack.Item>
                    <Checkbox
                        label='Set as default'
                        styles={{ root: { paddingTop: '10px' }, label: { fontSize: '14px', fontWeight: '600' } }} />
                </Stack.Item>
            </Stack>
        );
    } else {
        return (<Stack verticalFill verticalAlign='center' horizontalAlign='center'><Spinner /></Stack>)
    }
}
