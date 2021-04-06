// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { ComboBox, DefaultButton, IComboBox, IComboBoxOption, Label, PrimaryButton, Stack, TextField } from '@fluentui/react';
import { DeploymentScopeDefinition } from 'teamcloud';
import { useHistory, useParams } from 'react-router-dom';
import { useAzureManagementGroups, useAzureSubscriptions } from '../hooks';

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

    const [scopeName, setScopeName] = useState<string>();
    const [scopeManagementGroup, setScopeManagementGroup] = useState<string>();
    const [scopeManagementGroupOptions, setScopeManagementGroupOptions] = useState<IComboBoxOption[]>();
    const [scopeSubscriptions, setScopeSubscriptions] = useState<string[]>();
    const [scopeSubscriptionOptions, setScopeSubscriptionOptions] = useState<IComboBoxOption[]>();

    const [formEnabled, setFormEnabled] = useState<boolean>(true);

    const { onScopeChange } = props;

    const _scopeComplete = () => scopeName && (scopeManagementGroup || scopeSubscriptions);
    // const _scopeComplete = () => scopeName && scopeSubscriptions && scopeSubscriptions.length > 0;


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


    // useEffect(() => {
    //     if (scopeSubscriptionOptions && scopeSubscriptionOptions.length === 1 && scopeSubscriptions === undefined) {
    //         console.log('+ setScopeSubscriptions')
    //         setScopeSubscriptions([scopeSubscriptionOptions[0].key as string]);
    //     }
    // }, [scopeSubscriptions, scopeSubscriptionOptions]);


    useEffect(() => {
        if (onScopeChange !== undefined) {
            const scopeDef = {
                displayName: scopeName,
                managementGroupId: scopeManagementGroup,
                subscriptionIds: scopeSubscriptions,
                // isDefault: true
            } as DeploymentScopeDefinition;
            // console.log(`onScopeChange ${templateDef}`);
            onScopeChange(scopeDef);
        }
        // }, [onScopeChange, scopeName, scopeSubscriptions]);
    }, [onScopeChange, scopeName, scopeManagementGroup, scopeSubscriptions]);


    const _submitForm = () => {
        if (orgId && props.createDeploymentScope !== undefined && _scopeComplete()) {

            setFormEnabled(false);

            const scopeDef = {
                displayName: scopeName,
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
            {!(props.embedded ?? false) && (
                <Stack.Item styles={{ root: { paddingTop: '24px' } }}>
                    <PrimaryButton text='Create scope' disabled={!formEnabled || !_scopeComplete()} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
                    <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
                </Stack.Item>
            )}
        </Stack>
    );
}
