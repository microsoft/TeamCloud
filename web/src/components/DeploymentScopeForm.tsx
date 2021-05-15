// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { ComboBox, DefaultButton, IComboBox, IComboBoxOption, Label, PrimaryButton, Stack, TextField } from '@fluentui/react';
import { DeploymentScopeDefinition } from 'teamcloud';
import { FuiForm } from '@rjsf/fluent-ui'
import { useHistory, useParams } from 'react-router-dom';
import { useAzureManagementGroups, useAzureSubscriptions } from '../hooks';
import { useDeploymentScopeTypeInformation } from '../hooks/useDeploymentScopeTypeInformation';
import { ContentSeparator } from '.';
import { TeamCloudFieldTemplate } from './form/TeamCloudFieldTemplate';
import { TeamCloudForm } from './form/TeamCloudForm';

export interface IDeploymentScopeFormProps {
    embedded?: boolean,
    onScopeChange?: (scope?: DeploymentScopeDefinition) => void;
    createDeploymentScope?: (scope: DeploymentScopeDefinition) => Promise<void>;
}

export const DeploymentScopeForm: React.FC<IDeploymentScopeFormProps> = (props) => {

    const history = useHistory();
    const { orgId } = useParams() as { orgId: string };

    const { data: subscriptions } = useAzureSubscriptions();
    const { data: managementGroups } = useAzureManagementGroups();
    const { data: scopeTypeInformation } = useDeploymentScopeTypeInformation();

    const [scopeName, setScopeName] = useState<string>();

    const [scopeType, setScopeType] = useState<string>();
    const [scopeTypeOptions, setScopeTypeOptions] = useState<IComboBoxOption[]>();

    const [scopeTypeSchema, setScopeTypeSchema] = useState<string>();
    const [scopeTypeForm, setScopeTypeForm] = useState<string>();

    const [scopeManagementGroup, setScopeManagementGroup] = useState<string>();
    const [scopeManagementGroupOptions, setScopeManagementGroupOptions] = useState<IComboBoxOption[]>();
    const [scopeSubscriptions, setScopeSubscriptions] = useState<string[]>();
    const [scopeSubscriptionOptions, setScopeSubscriptionOptions] = useState<IComboBoxOption[]>();

    const [formEnabled, setFormEnabled] = useState<boolean>(true);

    const { onScopeChange } = props;

    const _scopeComplete = () => scopeName && scopeType && (scopeManagementGroup || scopeSubscriptions);

    // const _scopeTypeOptions = () => scopeTypeInformation?.map(info => ({ key: info.type?.toString(), text: info.displayName })) as IComboBoxOption[];

    // const _scopeTypeOptionsDefault = () => _scopeTypeOptions();

    useEffect(() => {
        if (subscriptions && scopeSubscriptionOptions === undefined) {
            // console.log('+ setScopeSubscriptionOptions')
            setScopeSubscriptionOptions(subscriptions?.map(s => ({ key: s.subscriptionId, text: s.displayName })));
        }
    }, [subscriptions, scopeSubscriptionOptions]);

    useEffect(() => {
        if (managementGroups && scopeManagementGroupOptions === undefined) {
            // console.log('+ scopeManagementGroupOptions')
            setScopeManagementGroupOptions(managementGroups?.map(s => ({ key: s.id, text: s.properties.displayName })));
        }
    }, [managementGroups, scopeManagementGroupOptions]);

    useEffect(() => {
        if (scopeTypeOptions === undefined) {
            // console.log('+ scopeTypeOptions')
            var options =scopeTypeInformation?.map(info => ({ key: info.type?.toString(), text: info.displayName })) as IComboBoxOption[];
            setScopeType(options?.find(option => option !== undefined)?.key as string);
            setScopeTypeOptions(options);
        }
    }, [scopeType, scopeTypeOptions, scopeTypeInformation])

    useEffect(() => {
        if (scopeType && scopeTypeInformation) {
            var scopeTypeInfo = scopeTypeInformation?.find(info => info && info.type === scopeType)
            console.log("ScopeTypeInfo = " + JSON.stringify(scopeTypeInfo));
            setScopeTypeSchema(scopeTypeInfo?.inputDataSchema || undefined);
            setScopeTypeForm(scopeTypeInfo?.inputDataForm || undefined);
        } else {
            setScopeTypeSchema(undefined);
            setScopeTypeForm(undefined);
        }
    }, [scopeType, scopeTypeInformation]);

    useEffect(() => {
        if (onScopeChange !== undefined) {
            const scopeDef = {
                displayName: scopeName,
                type: scopeType,
                managementGroupId: scopeManagementGroup,
                subscriptionIds: scopeSubscriptions,
                // isDefault: true
            } as DeploymentScopeDefinition;
            // console.log(`onScopeChange ${scopeDef}`);
            onScopeChange(scopeDef);
        }
        // }, [onScopeChange, scopeName, scopeSubscriptions]);
    }, [onScopeChange, scopeName, scopeType, scopeTypeSchema, scopeManagementGroup, scopeSubscriptions]);


    const _submitForm = () => {
        if (orgId && props.createDeploymentScope !== undefined && _scopeComplete()) {

            setFormEnabled(false);

            const scopeDef = {
                displayName: scopeName,
                type: scopeType,
                managementGroupId: scopeManagementGroup,
                subscriptionIds: scopeSubscriptions,
                // isDefault: true
            } as DeploymentScopeDefinition;

            props.createDeploymentScope(scopeDef);
        }
    };


    const _resetAndCloseForm = () => {
        setFormEnabled(true);
        history.push(`/orgs/${orgId}/settings/scopes`);
    };


    const _onScopeSubscriptionsChange = (event: React.FormEvent<IComboBox>, option?: IComboBoxOption, index?: number, value?: string) => {
        if (value) {
            const values = value.split(',');
            if (values.length > 0) {
                const subscriptionOptions = subscriptions?.map(s => ({ key: s.subscriptionId, text: s.displayName })) ?? [];
                const newOptions = values.map(v => ({ key: v.trim(), text: v.trim() }));
                setScopeSubscriptionOptions(subscriptionOptions.concat(newOptions) ?? newOptions);
                const validSubs = scopeSubscriptions?.filter(s => subscriptionOptions.findIndex(o => o ? o.key === s : false) ?? -1 >= 0) ?? [];
                setScopeSubscriptions(validSubs.concat(newOptions.map(no => no.key.trim())));
            }
        } else if (option?.key && option.selected !== undefined) {
            const sub = option.key as string;
            if (scopeSubscriptions) {
                if (!option.selected && scopeSubscriptions.indexOf(sub) >= 0) {
                    setScopeSubscriptions(scopeSubscriptions.filter(s => s !== sub));
                } else if (option.selected) {
                    setScopeSubscriptions(scopeSubscriptions.concat([sub]));
                }
            } else if (option.selected) {
                setScopeSubscriptions([sub]);
            }
        }
    };


    return (
        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: props.embedded ? { padding: '24px 8px' } : undefined }}>
            <Stack.Item>
                <TextField
                    required
                    label='Name'
                    description='Deployment scope display name'
                    disabled={!formEnabled}
                    value={scopeName}
                    onChange={(_ev, val) => setScopeName(val)} />
            </Stack.Item>
            <Stack.Item>
                <ComboBox
                    required
                    label='Type'
                    disabled={!formEnabled}
                    selectedKey={scopeType}
                    options={scopeTypeOptions}
                    onChange={(_ev, val) => setScopeType(val ? val.key as string : undefined)} />
            </Stack.Item>
            <Stack.Item>
                <ComboBox
                    required={!scopeSubscriptions || scopeSubscriptions.length === 0}
                    label='Management Group'
                    disabled={!formEnabled || !scopeManagementGroupOptions || (scopeSubscriptions && scopeSubscriptions.length > 0)}
                    selectedKey={scopeManagementGroup}
                    options={scopeManagementGroupOptions ?? []}
                    onChange={(_ev, val) => setScopeManagementGroup(val ? val.key as string : undefined)} />
            </Stack.Item>
            <Stack.Item>
                <Label disabled={!(scopeManagementGroup === undefined || scopeManagementGroup === '') || (scopeSubscriptions && scopeSubscriptions.length > 0)}>OR</Label>
            </Stack.Item>
            <Stack.Item>
                <ComboBox
                    required={!scopeManagementGroup}
                    // required
                    label='Subscriptions'
                    disabled={!formEnabled || !(scopeManagementGroup === undefined || scopeManagementGroup === '')}
                    // disabled={!formEnabled}
                    multiSelect
                    // allowFreeform
                    selectedKey={scopeSubscriptions}
                    options={scopeSubscriptionOptions}
                    onChange={_onScopeSubscriptionsChange} />
            </Stack.Item>
            <Stack.Item>
                <ContentSeparator />
                <FuiForm
                    disabled={!formEnabled}
                    onSubmit={_submitForm}
                    FieldTemplate={TeamCloudFieldTemplate}
                    widgets={TeamCloudForm.Widgets}
                    fields={TeamCloudForm.Fields}
                    schema={scopeTypeSchema ? JSON.parse(scopeTypeSchema) : {}}
                    uiSchema={scopeTypeForm ? JSON.parse(scopeTypeForm) : {}}>
                    <ContentSeparator />
                    <div style={{ paddingTop: '24px' }}>
                        <PrimaryButton text='Create scope' disabled={!formEnabled || !_scopeComplete()} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
                        <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
                    </div>
                </FuiForm>
            </Stack.Item>   

        </Stack>
    );
}
