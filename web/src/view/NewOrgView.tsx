// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory } from 'react-router-dom';
import { Stack, TextField, Dropdown, IDropdownOption, Text, PrimaryButton, DefaultButton, IconButton, Pivot, PivotItem, ComboBox, ChoiceGroup, Label, IComboBoxOption, IComboBox } from '@fluentui/react';
import { OrganizationDefinition, DeploymentScopeDefinition, ProjectTemplateDefinition, Organization } from 'teamcloud'
import { getManagementGroups, getSubscriptions } from '../Azure'
import { AzureRegions, Tags } from '../model';
import { ContentContainer, ContentHeader, ContentProgress, OrgSettingsDetail } from '../components';
import { api } from '../API';

export interface INewOrgViewProps {
    onOrgSelected: (org?: Organization) => void;
}

export const NewOrgView: React.FC<INewOrgViewProps> = (props) => {

    const history = useHistory();

    // Basic Settings
    const [orgName, setOrgName] = useState<string>();
    const [orgSubscription, setOrgSubscription] = useState<string>();
    const [orgSubscriptionOptions, setOrgSubscriptionOptions] = useState<IComboBoxOption[]>();
    const [orgRegion, setOrgRegion] = useState<string>();

    // Configuration
    const [webPortalEnabled, setWebPortalEnabled] = useState(true);

    // Deployment Scope
    const [scopeName, setScopeName] = useState<string>();
    const [scopeManagementGroup, setManagementScopeGroup] = useState<string>();
    const [scopeManagementGroupOptions, setScopeManagementGroupOptions] = useState<IDropdownOption[]>();
    const [scopeSubscriptions, setScopeSubscriptions] = useState<string[]>();
    const [scopeSubscriptionOptions, setScopeSubscriptionOptions] = useState<IComboBoxOption[]>();

    // Project Template
    const [templateName, setTemplateName] = useState<string>();
    const [templateUrl, setTemplateUrl] = useState<string>();
    const [templateVersion, setTemplateVersion] = useState<string>();
    const [templateToken, setTemplateToken] = useState<string>();

    // Tags
    const [tags, setTags] = useState<Tags>();

    // Misc.
    const [pivotKey, setPivotKey] = useState<string>('Basic Settings');
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [percentComplete, setPercentComplete] = useState<number>();
    const [errorText, setErrorText] = useState<string>();


    const pivotKeys = ['Basic Settings', 'Configuration', 'Deployment Scope', 'Project Template', 'Tags', 'Review + create'];


    const _orgComplete = () => orgName && orgSubscription && orgRegion;

    const _scopeComplete = () => scopeName && (scopeManagementGroup || scopeSubscriptions);

    const _templateComplete = () => templateName && templateUrl;

    useEffect(() => {
        if (!templateUrl) {
            setTemplateUrl('https://github.com/microsoft/TeamCloud-Project-Sample.git');
            setTemplateVersion('main');
            if (!templateName)
                setTemplateName('Sample Project Template');
        }
    }, [templateUrl, templateName]);

    useEffect(() => {
        const newTags: Tags = {}
        newTags[''] = ''
        setTags(newTags)
    }, []);

    useEffect(() => {
        const _resolveScopeGroup = async () => {

            try {
                const groups = await getManagementGroups();

                if (!groups)
                    return;

                console.log(groups);

                setScopeManagementGroupOptions(groups.map(g => ({ key: g.id, text: g.properties.displayName })))

                if (groups.length === 1 && groups[0].id === '/providers/Microsoft.Management/managementGroups/default') {

                    setScopeName(groups[0].properties.displayName);
                    setManagementScopeGroup(groups[0].id);
                }
            } catch (error) {
                console.error(error);
            }
        };
        _resolveScopeGroup();
    }, []);


    useEffect(() => {
        const _resolveSubscriptions = async () => {

            try {
                const subscriptions = await getSubscriptions();

                if (!subscriptions)
                    return;

                const options = subscriptions.map(s => ({ key: s.subscriptionId, text: s.displayName }));
                setOrgSubscriptionOptions(options);
                setScopeSubscriptionOptions(options);

                if (subscriptions.length === 1) {
                    setOrgSubscription(subscriptions[0].subscriptionId)
                }
            } catch (error) {
                console.error(error)
            }
        };
        _resolveSubscriptions();
    }, []);


    const _submitForm = async () => {
        if (_orgComplete()) {

            setFormEnabled(false);

            setPercentComplete(undefined);

            const orgDef = {
                displayName: orgName,
                subscriptionId: orgSubscription,
                location: orgRegion
            } as OrganizationDefinition;

            const orgResult = await api.createOrganization({ body: orgDef });

            const org = orgResult.data;

            setPercentComplete(.2);

            if (org) {

                props.onOrgSelected(org);

                if (_scopeComplete()) {
                    const scopeDef = {
                        displayName: scopeName,
                        managementGroupId: scopeManagementGroup,
                        subscriptionIds: scopeSubscriptions,
                        isDefault: true
                    } as DeploymentScopeDefinition;

                    await api.createDeploymentScope(org.id, { body: scopeDef, });
                }

                setPercentComplete(.4);

                if (_templateComplete()) {

                    const templateDef = {
                        displayName: templateName,
                        repository: {
                            url: templateUrl,
                            version: templateVersion ?? null,
                            token: templateToken ?? null
                        }
                    } as ProjectTemplateDefinition;

                    await api.createProjectTemplate(org.id, { body: templateDef });
                }

                setPercentComplete(1);
                history.push(`/orgs/${org.slug}`);

            } else {
                setPercentComplete(1);
                setErrorText(orgResult.status ?? 'failed to create new org');
            }
        }
    };

    const _resetAndCloseForm = async () => {

        setFormEnabled(false);
    };


    const _onTagKeyChange = (key: string, value: string, newKey?: string) => {
        const newTags: Tags = {}
        for (const k in tags) newTags[(k === key) ? newKey ?? '' : k] = value
        if (!newTags['']) newTags[''] = ''
        setTags(newTags)
    };

    const _onTagValueChange = (key: string, newValue?: string) => {
        const newTags: Tags = {}
        for (const k in tags) newTags[k] = (k === key) ? newValue ?? '' : tags[k]
        setTags(newTags)
    };

    const _getTagsTextFields = () => {
        let tagStack = [];
        if (tags) {
            let counter = 0
            for (const key in tags) {
                tagStack.push(
                    <Stack key={counter} horizontal tokens={{ childrenGap: '8px' }}>
                        <TextField disabled={!formEnabled} description='Name' value={key} onChange={(_ev, val) => _onTagKeyChange(key, tags[key], val)} />
                        <TextField disabled={!formEnabled} description='Value' value={tags[key]} onChange={(_ev, val) => _onTagValueChange(key, val)} />
                    </Stack>)
                counter++
            }
        }
        return (<Stack.Item>{tagStack}</Stack.Item>)
    };

    const _onReview = (): boolean => pivotKeys.indexOf(pivotKey) === pivotKeys.length - 1;

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

    const _getPrimaryButtonText = (): string => {
        const currentIndex = pivotKeys.indexOf(pivotKey);
        return currentIndex === pivotKeys.length - 1 ? 'Create organization' : `Next: ${pivotKeys[currentIndex + 1]}`;
    };

    return (
        <Stack styles={{ root: { height: '100%' } }}>
            <ContentProgress
                percentComplete={percentComplete}
                progressHidden={formEnabled} />
            <ContentHeader title='New Organization' coin={false} wide>
                <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.replace('/')} />
            </ContentHeader>
            <ContentContainer wide full>
                <Pivot selectedKey={pivotKey} onLinkClick={(i, ev) => setPivotKey(i?.props.itemKey ?? 'Basic Settings')} styles={{ root: { height: '100%' } }}>
                    <PivotItem headerText='Basic Settings' itemKey='Basic Settings'>
                        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Name'
                                    description='Organization display name'
                                    disabled={!formEnabled}
                                    value={orgName}
                                    onChange={(_ev, val) => setOrgName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <ComboBox
                                    required
                                    label='Subscription'
                                    disabled={!formEnabled}
                                    selectedKey={orgSubscription}
                                    options={orgSubscriptionOptions ?? []}
                                    onChange={(_ev, val) => setOrgSubscription(val ? val.key as string : undefined)} />
                            </Stack.Item>
                            <Stack.Item>
                                <ComboBox
                                    required
                                    label='Location'
                                    disabled={!formEnabled}
                                    allowFreeform
                                    autoComplete={'on'}
                                    selectedKey={orgRegion}
                                    options={AzureRegions.map(r => ({ key: r, text: r }))}
                                    onChange={(_ev, val) => setOrgRegion(val?.text ?? undefined)} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Configuration' itemKey='Configuration'>
                        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <ChoiceGroup
                                    required
                                    label='Web portal'
                                    disabled={!formEnabled}
                                    selectedKey={webPortalEnabled ? 'enabled' : 'disabled'}
                                    onChange={(ev, opt) => setWebPortalEnabled(opt?.key === 'enabled' ?? false)}
                                    options={[{ key: 'enabled', text: 'Enabled' }, { key: 'disabled', text: 'Disabled' }]} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Deployment Scope' itemKey='Deployment Scope'>
                        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: { padding: '24px 8px' } }}>
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
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Project Template' itemKey='Project Template'>
                        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Name'
                                    description='Project template display name'
                                    disabled={!formEnabled}
                                    value={templateName}
                                    onChange={(_ev, val) => setTemplateName(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    required
                                    label='Url'
                                    description='Git repository https url'
                                    disabled={!formEnabled}
                                    value={templateUrl}
                                    onChange={(_ev, val) => setTemplateUrl(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    label='Version'
                                    description='Branch/Tag/SHA'
                                    disabled={!formEnabled}
                                    value={templateVersion}
                                    onChange={(_ev, val) => setTemplateVersion(val)} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    label='Token'
                                    description='Personal access token (required for private repositories)'
                                    disabled={!formEnabled}
                                    value={templateToken}
                                    onChange={(_ev, val) => setTemplateToken(val)} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Tags' itemKey='Tags'>
                        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <Label disabled={!formEnabled}>Tags</Label>
                                {_getTagsTextFields()}
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Review + create' itemKey='Review + create'>
                        <Stack tokens={{ childrenGap: '40px' }} styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item>
                                <OrgSettingsDetail title='Basic Settings' details={[
                                    { label: 'Name', value: orgName, required: true },
                                    { label: 'Subscription', value: orgSubscription, required: true },
                                    { label: 'Location', value: orgRegion, required: true }
                                ]} />
                            </Stack.Item>
                            <Stack.Item>
                                <OrgSettingsDetail title='Configuration' details={[
                                    { label: 'Web Portal', value: webPortalEnabled ? 'Enabled' : 'Disabled', required: true }
                                ]} />
                            </Stack.Item>
                            {scopeManagementGroup && (
                                <Stack.Item>
                                    <OrgSettingsDetail title='Deployment Scope' details={[
                                        { label: 'Name', value: scopeName, required: true },
                                        { label: 'Management Group', value: scopeManagementGroup, required: true }
                                    ]} />
                                </Stack.Item>
                            )}
                            {scopeSubscriptions && (
                                <Stack.Item>
                                    <OrgSettingsDetail title='Deployment Scope' details={[
                                        { label: 'Name', value: scopeName, required: true },
                                        { label: 'Subscriptions', value: scopeSubscriptions.join(', '), required: true }
                                    ]} />
                                </Stack.Item>
                            )}
                            <Stack.Item>
                                <OrgSettingsDetail title='Project Template' details={[
                                    { label: 'Name', value: templateName, required: true },
                                    { label: 'Url', value: templateUrl, required: true },
                                    { label: 'Version', value: templateVersion },
                                    { label: 'Token', value: templateToken }
                                ]} />
                            </Stack.Item>
                        </Stack>
                    </PivotItem>
                </Pivot>
            </ContentContainer>
            <Stack.Item styles={{ root: { padding: '24px 52px' } }}>
                <PrimaryButton text={_getPrimaryButtonText()} disabled={!formEnabled || (_onReview() && !_orgComplete())} onClick={() => _onReview() ? _submitForm() : setPivotKey(pivotKeys[pivotKeys.indexOf(pivotKey) + 1])} styles={{ root: { marginRight: 8 } }} />
                <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            </Stack.Item>
            <Text>{errorText}</Text>
        </Stack>
    );
}
