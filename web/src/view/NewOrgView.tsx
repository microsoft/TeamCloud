// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useCallback } from 'react';
import { useHistory } from 'react-router-dom';
import { Stack, TextField, Text, PrimaryButton, DefaultButton, IconButton, Pivot, PivotItem, ComboBox, ChoiceGroup, Label, IComboBoxOption } from '@fluentui/react';
import { OrganizationDefinition, DeploymentScopeDefinition, ProjectTemplateDefinition } from 'teamcloud'
import { AzureRegions, Tags } from '../model';
import { CalloutLabel, ContentContainer, ContentHeader, ContentProgress, DeploymentScopeForm, ProjectTemplateForm } from '../components';
import { useCreateOrg, useAzureSubscriptions } from '../hooks';

export const NewOrgView: React.FC = () => {

    const history = useHistory();

    const { data: subscriptions } = useAzureSubscriptions();

    const createOrg = useCreateOrg();

    // Basic Settings
    const [orgName, setOrgName] = useState<string>();
    const [orgSubscription, setOrgSubscription] = useState<string>();
    const [orgSubscriptionOptions, setOrgSubscriptionOptions] = useState<IComboBoxOption[]>();
    const [orgRegion, setOrgRegion] = useState<string>();

    const [webPortalEnabled, setWebPortalEnabled] = useState(true);
    const [scope, setScope] = useState<DeploymentScopeDefinition>();
    const [template, setTemplate] = useState<ProjectTemplateDefinition>();
    const [tags, setTags] = useState<Tags>();

    // Misc.
    const [pivotKey, setPivotKey] = useState<string>('Basic Settings');
    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [percentComplete, setPercentComplete] = useState<number>();


    const pivotKeys = ['Basic Settings', 'Configuration', 'Deployment Scope', 'Project Template', 'Tags', 'Review + create'];


    const _orgComplete = () => orgName && orgSubscription && orgRegion;

    const _scopeComplete = () => scope?.displayName && scope?.type && scope?.inputData;

    const _templateComplete = () => template?.displayName && template.repository.url;


    useEffect(() => {
        const newTags: Tags = {}
        newTags[''] = ''
        setTags(newTags)
    }, []);


    useEffect(() => {
        if (subscriptions && orgSubscriptionOptions === undefined) {
            // console.log('+ setOrgSubscriptionOptions')
            setOrgSubscriptionOptions(subscriptions?.map(s => ({ key: s.subscriptionId, text: s.displayName })));
        }
    }, [subscriptions, orgSubscriptionOptions]);

    useEffect(() => {
        if (orgSubscriptionOptions && orgSubscriptionOptions.length === 1 && orgSubscription === undefined) {
            // console.log('+ setOrgSubscription')
            setOrgSubscription(orgSubscriptionOptions[0].key as string);
        }
    }, [orgSubscription, orgSubscriptionOptions]);


    const _submitForm = async () => {
        if (_orgComplete()) {

            setFormEnabled(false);

            setPercentComplete(undefined);

            const orgDef = {
                displayName: orgName,
                subscriptionId: orgSubscription,
                location: orgRegion
            } as OrganizationDefinition;

            const def = {
                orgDef: orgDef,
                scopeDef: scope && _scopeComplete() ? scope : undefined,
                templateDef: template && _templateComplete() ? template : undefined
            }

            await createOrg(def);
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


    const _getPrimaryButtonText = (): string => {
        const currentIndex = pivotKeys.indexOf(pivotKey);
        return currentIndex === pivotKeys.length - 1 ? 'Create organization' : `Next: ${pivotKeys[currentIndex + 1]}`;
    };

    const onScopeChange = useCallback((scope?: DeploymentScopeDefinition) => {
        // console.log(`+ onScopeChange: ${scope}`)
        setScope(scope);
    }, []);

    const onTemplateChange = useCallback((template?: ProjectTemplateDefinition) => {
        // console.log(`+ onTemplateChange: ${template}`)
        setTemplate(template);
    }, []);

    return (
        <Stack styles={{ root: { height: '100%' } }}>
            <ContentProgress
                percentComplete={percentComplete}
                progressHidden={formEnabled} />
            <ContentHeader title='New Organization' coin={false} wide>
                <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.push('/')} />
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
                        <DeploymentScopeForm embedded onScopeChange={onScopeChange} />
                    </PivotItem>
                    <PivotItem headerText='Project Template' itemKey='Project Template'>
                        <ProjectTemplateForm embedded onTemplateChange={onTemplateChange} />
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
                                <NewOrgReviewSection title='Basic Settings' details={[
                                    { label: 'Name', value: orgName, required: true },
                                    { label: 'Subscription', value: orgSubscription, required: true },
                                    { label: 'Location', value: orgRegion, required: true }
                                ]} />
                            </Stack.Item>
                            <Stack.Item>
                                <NewOrgReviewSection title='Configuration' details={[
                                    { label: 'Web Portal', value: webPortalEnabled ? 'Enabled' : 'Disabled', required: true }
                                ]} />
                            </Stack.Item>
                            <Stack.Item>
                                <NewOrgReviewSection title='Deployment Scope' details={[
                                    { label: 'Name', value: scope?.displayName ?? '', required: true },
                                    { label: 'Type', value: scope?.type ?? '', required: true },
                                    { label: 'Data', value: scope?.inputData ?? '', required: true }
                                ]} />
                            </Stack.Item>
                            <Stack.Item>
                                <NewOrgReviewSection title='Project Template' details={[
                                    { label: 'Name', value: template?.displayName, required: true },
                                    { label: 'Url', value: template?.repository.url, required: true },
                                    { label: 'Version', value: template?.repository.version ?? undefined },
                                    { label: 'Token', value: template?.repository.token ?? undefined }
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
        </Stack>
    );
}

export interface INewOrgReviewSection {
    title: string;
    details: { label: string, value?: string, required?: boolean }[]
}

export const NewOrgReviewSection: React.FC<INewOrgReviewSection> = (props) => {

    const _getDetailStacks = () => props.details.map(d => (
        <Stack
            horizontal
            verticalAlign='baseline'
            key={`${props.title}${d.label}`}
            tokens={{ childrenGap: 10 }}>
            <Label required={d.required}>{d.label}:</Label>
            <Text>{d.value}</Text>
        </Stack>
    ));

    return (
        <Stack>
            <CalloutLabel title={props.title} />
            {_getDetailStacks()}
        </Stack>
    );
}
