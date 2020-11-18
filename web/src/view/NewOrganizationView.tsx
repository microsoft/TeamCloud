// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Stack, TextField, Dropdown, IDropdownOption, ProgressIndicator, Text, PrimaryButton, DefaultButton, getTheme, IconButton, Pivot, PivotItem, ComboBox, ChoiceGroup, Label, IComboBoxOption } from '@fluentui/react';
import { OrganizationDefinition, DeploymentScopeDefinition, ProjectTemplateDefinition } from 'teamcloud'
import { getManagementGroups, getSubscriptions } from '../Azure'
import { AzureRegions, Tags } from '../model';
import { OrgSettingsDetail } from '../components';
import { api } from '../API';

export interface INewOrganizationViewProps { }

export const NewOrganizationView: React.FunctionComponent<INewOrganizationViewProps> = (props) => {

    let history = useHistory();

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
    const [percentComplete, setPercentComplete] = useState(0);
    const [errorText, setErrorText] = useState<string>();


    const pivotKeys = ['Basic Settings', 'Configuration', 'Deployment Scope', 'Project Template', 'Tags', 'Review + create'];


    const _formComplete = () =>
        orgName
        && orgSubscription
        && orgRegion
        && scopeName
        && scopeManagementGroup
        && templateName
        && templateUrl;


    useEffect(() => {
        const newTags: Tags = {}
        newTags[''] = ''
        setTags(newTags)
    }, []);

    useEffect(() => {
        const _resolveScopeGroup = async () => {

            const groups = await getManagementGroups();

            if (!groups)
                return;

            setScopeManagementGroupOptions(groups.map(g => ({ key: g.id, text: g.properties.displayName })))

            if (groups.length === 1 && groups[0].id === '/providers/Microsoft.Management/managementGroups/default') {

                setScopeName(groups[0].properties.displayName);
                setManagementScopeGroup(groups[0].id);
            }

            const subscriptions = await getSubscriptions();

            setOrgSubscriptionOptions(subscriptions.map(s => ({ key: s.subscriptionId, text: s.displayName })));

            if (subscriptions.length === 1) {
                setOrgSubscription(subscriptions[0].subscriptionId)
            }
        };
        _resolveScopeGroup();
    }, []);


    const _submitForm = async () => {
        if (_formComplete()) {
            setFormEnabled(false);

            const orgDef = { displayName: orgName } as OrganizationDefinition;

            const orgResult = await api.createOrganization({ body: orgDef });

            orgResult.
        }
    };

    const _resetAndCloseForm = async () => {

        // await msal.instance.acquireTokenRedirect({ scopes: ['https://management.azure.com/.default'] })
        // const authResult = await msal.instance.handleRedirectPromise()
        // console.error(authResult)
        // if (authResult?.accessToken)

        // setOrgName(undefined);
        // setFormEnabled(true);
        // props.onFormClose();
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

    const _getPrimaryButtonText = (): string => {
        const currentIndex = pivotKeys.indexOf(pivotKey);
        return currentIndex === pivotKeys.length - 1 ? 'Create organization' : `Next: ${pivotKeys[currentIndex + 1]}`;
    };

    const theme = getTheme();

    // React.useEffect(() => {
    //     const id = setInterval(() => {
    //         setPercentComplete((0.1 + percentComplete) % 1);
    //     }, 1000);
    //     return () => {
    //         clearInterval(id);
    //     };
    // });

    return (
        <Stack styles={{ root: { height: '100%' } }}>
            <ProgressIndicator
                percentComplete={percentComplete}
                progressHidden={formEnabled}
                styles={{ itemProgress: { padding: '0px', marginTop: '-2px' } }} />
            <Stack.Item styles={{ root: { margin: '0px', padding: '24px 30px 20px 32px', backgroundColor: theme.palette.white, borderBottom: `${theme.palette.neutralLight} solid 1px` } }}>
                <Stack horizontal
                    verticalFill
                    horizontalAlign='space-between'
                    verticalAlign='baseline'>
                    <Stack.Item>
                        <Text styles={{ root: { fontSize: theme.fonts.xxLarge.fontSize, fontWeight: '700', letterSpacing: '-1.12px', marginLeft: '12px' } }}>
                            New Organization
                        </Text>
                    </Stack.Item>
                    <Stack.Item styles={{ root: { paddingRight: '12px' } }}>
                        <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.replace('/')} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item styles={{ root: { padding: '24px 44px', height: '100%' } }}>
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
                                    required
                                    label='Management Group'
                                    disabled={!formEnabled || !scopeManagementGroupOptions}
                                    selectedKey={scopeManagementGroup}
                                    options={scopeManagementGroupOptions ?? []}
                                    onChange={(_ev, val) => setManagementScopeGroup(val ? val.key as string : undefined)} />
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
                            <Stack.Item>
                                <OrgSettingsDetail title='Deployment Scope' details={[
                                    { label: 'Name', value: scopeName, required: true },
                                    { label: 'Management Group', value: scopeManagementGroup, required: true }
                                ]} />
                            </Stack.Item>
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
            </Stack.Item>
            <Stack.Item styles={{ root: { padding: '24px 52px' } }}>
                <PrimaryButton text={_getPrimaryButtonText()} disabled={!formEnabled || (_onReview() && !_formComplete())} onClick={() => _onReview() ? _submitForm() : setPivotKey(pivotKeys[pivotKeys.indexOf(pivotKey) + 1])} styles={{ root: { marginRight: 8 } }} />
                <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            </Stack.Item>
            <Text>{errorText}</Text>
        </Stack>
    );
}
