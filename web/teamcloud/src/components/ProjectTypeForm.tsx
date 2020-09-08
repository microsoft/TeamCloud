// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Stack, TextField, Spinner, DefaultButton, IButtonStyles, getTheme, Image, ButtonType, Text, Label, Checkbox, SpinButton, Panel, PrimaryButton } from '@fluentui/react';
import { Position } from 'office-ui-fabric-react/lib/utilities/positioning';
import { ProjectType, Provider, DataResult, ProviderReference, StatusResult, ErrorResult } from "../model";
import { getProviders, createProjectType } from "../API";
import AppInsights from '../img/appinsights.svg';
import DevOps from '../img/devops.svg';
import DevTestLabs from '../img/devtestlabs.svg';
import GitHub from '../img/github.svg';

export interface IProjectTypeFormProps {
    // fieldsEnabled: boolean;
    // onFormSubmit: () => void;
    // onNameChange: (val?: string) => void;
    // onProjectTypeChange: (val?: ProjectType) => void;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const ProjectTypeForm: React.FunctionComponent<IProjectTypeFormProps> = (props) => {

    const [providers, setProviders] = useState<Provider[]>();
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [projectTypeName, setProjectTypeName] = useState<string>();
    const [projectTypeIsDefault, setProjectTypeIsDefault] = useState<boolean>();
    const [projectTypeRegion, setProjectTypeRegion] = useState('eastus');
    const [projectTypeSubscriptions, setProjectTypeSubscriptions] = useState(new Array<string>());
    const [projectTypeSubscriptionsCapacity, setProjectTypeSubscriptionsCapacity] = useState(10);
    const [projectTypeRgNamePrefix, setProjectTypeRgNamePrefix] = useState<string>();
    const [projectTypeProviders, setProjectTypeProviders] = useState(new Array<ProviderReference>());
    const [errorText, setErrorText] = useState<string>();

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

    const _toggleProviderSelection = (provider: Provider) => {
        let index = projectTypeProviders.findIndex(r => r.id === provider.id)
        if (index > -1) {
            projectTypeProviders.splice(index, 1)
        } else {
            const providerReference: ProviderReference = {
                id: provider.id
            }
            projectTypeProviders.push(providerReference)
        }
        console.log(projectTypeProviders.map(p => p.id).join(', '))
        console.log(projectTypeRegion)
    }

    const _getProviderButtons = (data?: Provider[]) => {
        let buttons = data?.map(p => (
            <Stack.Item styles={{ root: { width: '46%' } }}>
                <DefaultButton
                    toggle={true}
                    buttonType={ButtonType.icon}
                    styles={_providerButtonStyles}
                    onClick={() => _toggleProviderSelection(p)} >
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

    const _submitForm = async () => {
        setFormEnabled(false);
        if (projectTypeName && projectTypeProviders.length > 0 && projectTypeSubscriptions.length > 0) {
            const projectType: ProjectType = {
                id: projectTypeName,
                isDefault: projectTypeIsDefault,
                region: projectTypeRegion,
                subscriptions: projectTypeSubscriptions,
                subscriptionCapacity: projectTypeSubscriptionsCapacity,
                resourceGroupNamePrefix: projectTypeRgNamePrefix,
                providers: projectTypeProviders
            };
            const result = await createProjectType(projectType);
            if ((result as StatusResult).code === 202)
                _resetAndCloseForm();
            else if ((result as ErrorResult).errors) {
                // console.log(JSON.stringify(result));
                setErrorText((result as ErrorResult).status);
            }
        }
    };

    const _resetAndCloseForm = () => {
        setProjectTypeName(undefined);
        setProjectTypeIsDefault(undefined)
        setProjectTypeRegion('eastus')
        setProjectTypeSubscriptions(new Array<string>())
        setProjectTypeSubscriptionsCapacity(10)
        setProjectTypeRgNamePrefix(undefined)
        setProjectTypeProviders(new Array<ProviderReference>())
        setErrorText("")
        setFormEnabled(true);
        props.onFormClose();
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton disabled={!formEnabled || !(projectTypeName && projectTypeProviders.length > 0 && projectTypeSubscriptions.length > 0)} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }}>
                Create project
            </PrimaryButton>
            <DefaultButton disabled={!formEnabled} onClick={() => _resetAndCloseForm()}>Cancel</DefaultButton>
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    return (
        <Panel
            headerText='New project'
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}
            onRenderFooterContent={_onRenderPanelFooterContent}>

            <Stack tokens={{ childrenGap: '12px' }}>
                <Stack.Item>
                    <Label required>Providers</Label>
                    {_getProviderButtons(providers)}
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        label='Name'
                        required
                        // errorMessage='Name is required.'
                        disabled={!formEnabled}
                        onChange={(ev, val) => setProjectTypeName(val)} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        label='Region'
                        required
                        // errorMessage='Name is required.'
                        defaultValue={projectTypeRegion}
                        disabled={!formEnabled}
                        onChange={(ev, val) => setProjectTypeRegion(val ?? 'eastus')} />
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        label='Subscriptions'
                        required
                        // errorMessage='Name is required.'
                        disabled={!formEnabled}
                        onChange={(ev, val) => setProjectTypeSubscriptions(val?.split(',') ?? [])} />
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
                    {/* onChange={(ev, val) => setProjectTypeSubscriptionsCapacity(val)} /> */}
                </Stack.Item>
                <Stack.Item>
                    <TextField
                        label='Resource Group name prefix'
                        // errorMessage='Name is required.'
                        disabled={!formEnabled}
                        onChange={(ev, val) => setProjectTypeRgNamePrefix(val)} />
                </Stack.Item>
                <Stack.Item>
                    <Checkbox
                        label='Set as default'
                        styles={{ root: { paddingTop: '10px' }, label: { fontSize: '14px', fontWeight: '600' } }}
                        onChange={(ev, val) => setProjectTypeIsDefault(val)} />
                </Stack.Item>
            </Stack>
            <Text>{errorText}</Text>
        </Panel>
    );
}
