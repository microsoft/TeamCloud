// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { ComboBox, DefaultButton, Dropdown, IComboBox, IComboBoxOption, IDropdownOption, Label, PrimaryButton, Stack, Text, TextField } from '@fluentui/react';
import { DeploymentScopeDefinition } from 'teamcloud';
import { useHistory, useParams } from 'react-router-dom';
import { getManagementGroups, getSubscriptions } from '../Azure';

export interface IDeploymentScopeFormProps {
    showProgress: (show: boolean) => void;
    onCreateDeploymentScope: (scope: DeploymentScopeDefinition) => Promise<void>;
}

export const DeploymentScopeForm: React.FC<IDeploymentScopeFormProps> = (props) => {

    let history = useHistory();
    let { orgId } = useParams() as { orgId: string };

    const [scopeName, setScopeName] = useState<string>();
    const [scopeManagementGroup, setManagementScopeGroup] = useState<string>();
    const [scopeManagementGroupOptions, setScopeManagementGroupOptions] = useState<IDropdownOption[]>();
    const [scopeSubscriptions, setScopeSubscriptions] = useState<string[]>();
    const [scopeSubscriptionOptions, setScopeSubscriptionOptions] = useState<IComboBoxOption[]>();

    const [orgSubscriptionOptions, setOrgSubscriptionOptions] = useState<IComboBoxOption[]>();

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [errorText, setErrorText] = useState<string>();

    const _scopeComplete = () => scopeName && (scopeManagementGroup || scopeSubscriptions);

    useEffect(() => {
        const _resolveScopeGroup = async () => {
            try {
                const groups = await getManagementGroups();

                if (!groups) {
                    setScopeManagementGroupOptions([]);
                    return;
                }

                setScopeManagementGroupOptions(groups.map(g => ({ key: g.id, text: g.properties.displayName })));

                if (groups.length === 1 && groups[0].id === '/providers/Microsoft.Management/managementGroups/default') {

                    // setScopeName(groups[0].properties.displayName);
                    // setManagementScopeGroup(groups[0].id);
                }
            } catch (error) {
                console.error(error);
                setScopeManagementGroupOptions([]);
            }
        };
        _resolveScopeGroup();
    }, []);


    useEffect(() => {
        const _resolveSubscriptions = async () => {
            try {
                const subscriptions = await getSubscriptions();

                if (!subscriptions) {
                    setOrgSubscriptionOptions([]);
                    setScopeSubscriptionOptions([])
                    return;
                }

                const options = subscriptions.map(s => ({ key: s.subscriptionId, text: s.displayName }));
                setOrgSubscriptionOptions(options);
                setScopeSubscriptionOptions(options);

            } catch (error) {
                console.error(error)
                setOrgSubscriptionOptions([]);
                setScopeSubscriptionOptions([])
            }
        };
        _resolveSubscriptions();
    }, []);

    useEffect(() => {
        updateProgress(scopeManagementGroupOptions === undefined || orgSubscriptionOptions === undefined)
    }, [scopeManagementGroupOptions, orgSubscriptionOptions]);


    const updateProgress = (show: boolean) => props.showProgress(show);

    const _submitForm = () => {
        if (orgId && _scopeComplete()) {

            setFormEnabled(false);

            const scopeDef = {
                displayName: scopeName,
                managementGroupId: scopeManagementGroup,
                subscriptionIds: scopeSubscriptions,
                isDefault: true
            } as DeploymentScopeDefinition;

            props.onCreateDeploymentScope(scopeDef);
        }
    };

    const _resetAndCloseForm = () => {
        // setComponentTemplate(undefined);
        setFormEnabled(true);
        history.replace(`/orgs/${orgId}/settings/scopes`);
        // props.onFormClose();
    };


    const _onScopeSubscriptionsChange = (event: React.FormEvent<IComboBox>, option?: IComboBoxOption, index?: number, value?: string) => {
        if (value) {
            const values = value.split(',');
            if (values.length > 0) {
                const newOptions = values.map(v => ({ key: v.trim(), text: v.trim() }));
                setScopeSubscriptionOptions(orgSubscriptionOptions?.concat(newOptions) ?? newOptions);
                const validSubs = scopeSubscriptions?.filter(s => orgSubscriptionOptions?.findIndex(o => o ? o.key === s : false) ?? -1 >= 0) ?? [];
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
        <Stack
            tokens={{ childrenGap: '20px' }}>
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
                <Dropdown
                    required={!scopeSubscriptions || scopeSubscriptions.length === 0}
                    label='Management Group'
                    disabled={!formEnabled || !scopeManagementGroupOptions || (scopeSubscriptions && scopeSubscriptions.length > 0)}
                    selectedKey={scopeManagementGroup}
                    options={scopeManagementGroupOptions ?? []}
                    onChange={(_ev, val) => setManagementScopeGroup(val ? val.key as string : undefined)} />
            </Stack.Item>
            <Stack.Item>
                <Label disabled={!(scopeManagementGroup === undefined || scopeManagementGroup === '') || (scopeSubscriptions && scopeSubscriptions.length > 0)}>OR</Label>
            </Stack.Item>
            <Stack.Item>
                <ComboBox
                    required={!scopeManagementGroup}
                    label='Subscriptions'
                    disabled={!formEnabled || !(scopeManagementGroup === undefined || scopeManagementGroup === '')}
                    multiSelect
                    allowFreeform
                    selectedKey={scopeSubscriptions}
                    options={scopeSubscriptionOptions ?? []}
                    onChange={_onScopeSubscriptionsChange} />
            </Stack.Item>
            <Stack.Item styles={{ root: { paddingTop: '24px' } }}>
                <PrimaryButton text='Create scope' disabled={!formEnabled || !_scopeComplete()} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
                <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            </Stack.Item>
            <Text>{errorText}</Text>
        </Stack>
    );
}
