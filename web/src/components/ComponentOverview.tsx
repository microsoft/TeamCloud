// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { FontIcon, getTheme, Pivot, PivotItem, Stack, Text, TextField } from '@fluentui/react';
import ReactMarkdown from 'react-markdown';
import gfm from 'remark-gfm'
import { FuiForm } from '@rjsf/fluent-ui';
import { FieldTemplateProps, WidgetProps } from '@rjsf/core';
import { ComponentTemplate, DeploymentScope } from 'teamcloud';
import { ProjectMember } from '../model';
import { ComponentTaskList, UserPersona, ComponentLink, ComponentTemplateLink } from '.';
import { useDeploymentScopes, useProjectComponent, useProjectComponentTemplates, useProjectMembers } from '../hooks';

export const ComponentOverview: React.FC = (props) => {

    const theme = getTheme();

    const { data: scopes } = useDeploymentScopes();

    const { data: members } = useProjectMembers();
    const { data: component } = useProjectComponent();
    const { data: templates } = useProjectComponentTemplates();

    const [template, setTemplate] = useState<ComponentTemplate>();
    const [creator, setCreator] = useState<ProjectMember>();
    const [scope, setScope] = useState<DeploymentScope>();
    const [pivotKey, setPivotKey] = useState<string>('Runs');


    useEffect(() => {
        if (component && templates && (template === undefined || component.templateId.toLowerCase() !== template.id.toLowerCase())) {
            // console.log(`+ setComponentTemplate (${component.slug})`);
            setTemplate(templates.find(t => component.templateId.toLowerCase() === t.id.toLowerCase()) ?? undefined);
        }
    }, [component, template, templates])


    useEffect(() => {
        if (component && members && (creator === undefined || creator.user.id.toLowerCase() !== component.creator.toLowerCase())) {
            const ctr = members.find(m => component.creator.toLowerCase() === m.user.id.toLowerCase()) ?? undefined
            // console.log(`+ setComponentCreator (${ctr?.graphUser?.displayName})`);
            setCreator(ctr);
        }
    }, [component, creator, members])


    useEffect(() => {
        if (component && scopes && (scope === undefined || (component.deploymentScopeId && scope.id.toLowerCase() !== component.deploymentScopeId.toLowerCase()))) {
            // console.log(`+ setComponentScope (${component.slug})`);
            setScope(scopes.find(s => component.deploymentScopeId?.toLowerCase() === s.id.toLowerCase()) ?? undefined);
        }
    }, [component, scope, scopes])


    const _getTypeIcon = (template?: ComponentTemplate) => {
        if (template?.type)
            switch (template.type) { // VisualStudioIDELogo32
                case 'Custom': return 'Link'; // Link12, FileSymlink, OpenInNewWindow, VSTSLogo
                case 'Readme': return 'PageList'; // Preview, Copy, FileHTML, FileCode, MarkDownLanguage, Document
                case 'Environment': return 'AzureLogo'; // Processing, Settings, Globe, Repair
                case 'AzureResource': return 'AzureLogo'; // AzureServiceEndpoint
                case 'GitRepository': return 'OpenSource';
                default: return undefined;
            }
    };

    return (
        <Stack styles={{ root: { height: '100%', } }} tokens={{ childrenGap: '40px' }}>
            <Stack.Item>
                <Stack
                    horizontal
                    horizontalAlign='space-between'
                    verticalAlign='center'
                    // tokens={{ childrenGap: '20px' }}
                    styles={{
                        root: {
                            padding: '40px',
                            borderRadius: theme.effects.roundedCorner4,
                            boxShadow: theme.effects.elevation4,
                            backgroundColor: theme.palette.white
                        }
                    }}>
                    <ComponentOverviewHeaderSection grow title='Type'>
                        {/* <Stack horizontal styles={{ root: { color: theme.palette.blue } }}> */}
                        <Stack horizontal>
                            <FontIcon iconName={_getTypeIcon(template)} className='component-type-icon' />
                            <Text styles={{ root: { paddingLeft: '4px' } }}>{template?.type}</Text>
                        </Stack>
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection grow title='Link'>
                        <ComponentLink component={component} />
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection grow title='Template'>
                        <ComponentTemplateLink componentTemplate={template} />
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection grow title='Scope'>
                        <Text>{scope?.displayName}</Text>
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection grow title='State'>
                        <Text>{component?.resourceState}</Text>
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection title='Creator'>
                        <UserPersona user={creator?.graphUser} showSecondaryText styles={{ root: { minWidth: '220px' } }} />
                    </ComponentOverviewHeaderSection>
                </Stack>
            </Stack.Item>
            <Stack.Item styles={{ root: { height: '100%', padding: '0px' } }}>
                <Pivot selectedKey={pivotKey} onLinkClick={(i, ev) => setPivotKey(i?.props.itemKey ?? 'Runs')} styles={{ root: { height: '100%' } }}>
                    <PivotItem headerText='Overview' itemKey='Overview'>
                        <Stack
                            horizontal
                            horizontalAlign='start'
                            tokens={{ childrenGap: '20px' }}
                            styles={{ root: { height: '100%', padding: '24px 8px' } }}>
                            <Stack.Item styles={{ root: { minWidth: '460px' } }}>
                                <FuiForm
                                    widgets={{ 'SelectWidget': ReadonlySelectWidget }}
                                    FieldTemplate={ReadonlyFieldTemplate}
                                    schema={template?.inputJsonSchema ? JSON.parse(template.inputJsonSchema) : {}}
                                    formData={component?.inputJson ? JSON.parse(component.inputJson) : undefined}
                                    onChange={() => { }}>
                                    <></>
                                </FuiForm>
                            </Stack.Item>
                            {template?.description && (
                                <Stack.Item grow={2} styles={{
                                    root: {
                                        // height: '100%',
                                        // minWidth: '400px',
                                        // maxWidth: '1000px',
                                        height: '720px',
                                        padding: '10px 40px',
                                        borderRadius: theme.effects.roundedCorner4,
                                        boxShadow: theme.effects.elevation4,
                                        backgroundColor: theme.palette.white
                                    }
                                }}>
                                    <ReactMarkdown plugins={[gfm]}>{template?.description ?? undefined as any}</ReactMarkdown>
                                </Stack.Item>
                            )}
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Task Runs' itemKey='Runs'>
                        <ComponentTaskList />
                    </PivotItem>
                    <PivotItem headerText='Tasks' itemKey='Tasks'>
                        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: { padding: '24px 8px' } }}>
                        </Stack>
                    </PivotItem>
                </Pivot>
            </Stack.Item>
        </Stack>
    );
}


export interface IComponentOverviewHeaderSectionProps {
    gap?: string;
    grow?: boolean;
    title?: string;
    minWidth?: string;
}

export const ComponentOverviewHeaderSection: React.FC<IComponentOverviewHeaderSectionProps> = (props) => {
    const theme = getTheme();

    return (
        <Stack.Item grow={props.grow} styles={{ root: { minWidth: props.minWidth, minHeight: '70px' } }}>
            <Stack tokens={{ childrenGap: props.gap ?? '16px' }}>
                <Stack.Item>
                    <Text variant='medium' styles={{ root: { color: theme.palette.neutralSecondaryAlt, fontWeight: '600' } }}>{props.title}</Text>
                </Stack.Item>
                <Stack.Item>
                    {props.children}
                </Stack.Item>
            </Stack>
        </Stack.Item>
    );
}

export const ReadonlyFieldTemplate: React.FC<FieldTemplateProps> = (props) =>
    props.id === 'root' ? (
        <Stack styles={{ root: { paddingTop: '16px', minWidth: '460px' } }} tokens={{ childrenGap: '14px' }}>
            {props.children}
        </Stack>
    ) : (
        <Stack.Item grow styles={{ root: { paddingBottom: '16px' } }}>
            {props.children}
        </Stack.Item>
    );


export const ReadonlySelectWidget: React.FC<WidgetProps> = (props) => (
    <TextField
        readOnly
        label={props.schema.description}
        defaultValue={props.value}
        styles={{

        }} />
);
