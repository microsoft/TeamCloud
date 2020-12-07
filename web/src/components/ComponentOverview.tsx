// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, useEffect, useState } from 'react';
import { FontIcon, getTheme, IColumn, Image, Link, Persona, PersonaSize, Pivot, PivotItem, Separator, Stack, Text, TextField } from '@fluentui/react';
import { useHistory, useParams } from 'react-router-dom';
import { Component, ComponentTemplate, DeploymentScope } from 'teamcloud';
import { ContentList } from '.';
import { OrgContext, ProjectContext } from '../Context';
import collaboration from '../img/MSC17_collaboration_010_noBG.png'
import DevOps from '../img/devops.svg';
import GitHub from '../img/github.svg';
import Resource from '../img/resource.svg';
import { ProjectMember } from '../model';
import { FuiForm } from '@rjsf/fluent-ui';
import { ComponentDeploymentList } from './ComponentDeploymentList';
import ReactMarkdown from 'react-markdown';
import { FieldTemplateProps } from '@rjsf/core';

// export const ComponentOverview: React.FC<{component: Component}> = (props) => {
export const ComponentOverview: React.FC = (props) => {

    const theme = getTheme();

    const { orgId, projectId, itemId } = useParams() as { orgId: string, projectId: string, itemId: string };

    const { scopes } = useContext(OrgContext);
    const { component, templates, members } = useContext(ProjectContext);

    const [template, setTemplate] = useState<ComponentTemplate>();
    const [creator, setCreator] = useState<ProjectMember>();
    const [scope, setScope] = useState<DeploymentScope>();
    const [pivotKey, setPivotKey] = useState<string>('Parameters');

    // const pivotKeys = ['Parameters', 'Deployments', 'Settings'];

    // const [component, setComponent] = useState<Component>();
    // const [template, setTemplate] = useState<ComponentTemplate>();

    useEffect(() => {
        if (component && templates && (template === undefined || component.templateId.toLowerCase() !== template.id.toLowerCase())) {
            // console.log(component);
            console.log(component.inputJson);
            setTemplate(templates.find(t => component.templateId.toLowerCase() === t.id.toLowerCase()) ?? undefined);
        }
    }, [component, template, templates])


    useEffect(() => {
        if (component && members && (creator === undefined || creator.user.id.toLowerCase() !== component.requestedBy.toLowerCase())) {
            // console.log(component.resourceId);
            setCreator(members.find(m => component.requestedBy.toLowerCase() === m.user.id.toLowerCase()) ?? undefined);
        }
    }, [component, creator, members])


    useEffect(() => {
        if (component && scopes && (scope === undefined || (component.deploymentScopeId && scope.id.toLowerCase() !== component.deploymentScopeId.toLowerCase()))) {
            // console.log(component.resourceId);
            setScope(scopes.find(s => component.deploymentScopeId?.toLowerCase() === s.id.toLowerCase()) ?? undefined);
        }
    }, [component, scope, scopes])


    const _getTypeImage = (template: ComponentTemplate) => {
        const provider = template.repository.provider.toLowerCase();
        switch (template.type) {
            // case 'Custom': return 'Link';
            // case 'Readme': return 'PageList';
            case 'Environment': return Resource;
            case 'AzureResource': return Resource;
            case 'GitRepository': return provider === 'github' ? GitHub : provider === 'devops' ? DevOps : undefined;
        }
        return undefined;
    };

    const _getRepoImage = (template?: ComponentTemplate) => {
        if (template?.repository.provider) {
            switch (template.repository.provider) {
                // case 'Unknown': return;
                case 'DevOps': return DevOps;
                case 'GitHub': return GitHub;
            }
        }
        return undefined;
    };

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

    // const _getOverviewHeaderSection = (text: string) => (
    //     <Stack.Item>
    //         <Text variant='medium' styles={{ root: { color: theme.palette.neutralSecondaryAlt, fontWeight: '600' } }}>{text}</Text>
    //     </Stack.Item>
    // );


    // const getFieldTemplate: React.StatelessComponent<FieldTemplateProps> = (fieldProps: FieldTemplateProps) => {
    //     console.log(fieldProps);
    //     return (<TextField readOnly label={prps.label} />);
    //     }

    return component ? (
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
                    <ComponentOverviewHeaderSection grow title='Template'>
                        <Link onClick={() => { }} >{template?.repository.repository?.replaceAll('-', ' ') ?? template?.repository.url}</Link>
                        {/* <Text>{template?.repository.repository?.replaceAll('-', ' ') ?? template?.repository.url}</Text> */}
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection grow title='Type'>
                        {/* <Stack horizontal styles={{ root: { color: theme.palette.blue } }}> */}
                        <Stack horizontal>
                            <FontIcon iconName={_getTypeIcon(template)} className='component-type-icon' />
                            <Text styles={{ root: { paddingLeft: '4px' } }}>{template?.type}</Text>
                        </Stack>
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection grow title='Scope'>
                        <Text>{scope?.displayName}</Text>
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection grow title='State'>
                        <Text>{component.resourceState}</Text>
                    </ComponentOverviewHeaderSection>
                    <ComponentOverviewHeaderSection title='Creator'>
                        <Persona
                            text={creator?.graphUser?.displayName ?? creator?.user.id}
                            showSecondaryText
                            secondaryText={creator?.graphUser?.mail ?? (creator?.graphUser?.otherMails && creator.graphUser.otherMails.length > 0 ? creator.graphUser.otherMails[0] : undefined)}
                            imageUrl={creator?.graphUser?.imageUrl}
                            // styles={{ primaryText: { fontSize: '16px' } }}
                            size={PersonaSize.size32} />
                    </ComponentOverviewHeaderSection>
                </Stack>
            </Stack.Item>
            <Stack.Item styles={{ root: { height: '100%', padding: '0px' } }}>
                <Pivot selectedKey={pivotKey} onLinkClick={(i, ev) => setPivotKey(i?.props.itemKey ?? 'Parameters')} styles={{ root: { height: '100%' } }}>
                    <PivotItem headerText='Parameters' itemKey='Parameters'>
                        <Stack horizontal tokens={{ childrenGap: '40px' }} styles={{ root: { height: '100%', padding: '24px 8px' } }}>
                            <Stack.Item grow styles={{ root: { minWidth: '40%', } }}>
                                <FuiForm
                                    // FieldTemplate={(prps) => { console.log(prps); return (<TextField readOnly label={prps.label} />); }}
                                    // uiSchema={{ 'ui:field': 'readonly' }}
                                    disabled
                                    schema={template?.inputJsonSchema ? JSON.parse(template.inputJsonSchema) : {}}
                                    formData={component.inputJson ? JSON.parse(component.inputJson) : undefined}
                                    onChange={() => { }}>
                                    <></>
                                </FuiForm>
                            </Stack.Item>
                            {template?.description && (
                                <Stack.Item grow styles={{
                                    root: {
                                        height: '100%',
                                        minWidth: '40%',
                                        padding: '10px 40px',
                                        borderRadius: theme.effects.roundedCorner4,
                                        boxShadow: theme.effects.elevation4,
                                        backgroundColor: theme.palette.white
                                    }
                                }}>
                                    <ReactMarkdown>{template?.description ?? undefined as any}</ReactMarkdown>
                                </Stack.Item>
                            )}
                        </Stack>
                    </PivotItem>
                    <PivotItem headerText='Deployments' itemKey='Deployments'>
                        <Stack
                            horizontal
                            verticalFill
                            tokens={{ childrenGap: '40px' }}
                            styles={{ root: { padding: '24px 8px' } }}>
                            <Stack.Item grow styles={{ root: { minWidth: '40%', } }}>
                                <ComponentDeploymentList />
                            </Stack.Item>
                            <Stack.Item grow styles={{
                                root: {
                                    height: '100%',
                                    minWidth: '40%',
                                    padding: '10px 40px',
                                    borderRadius: theme.effects.roundedCorner4,
                                    // boxShadow: theme.effects.elevation4,
                                    backgroundColor: theme.palette.black
                                }
                            }}>
                                <Text block styles={{ root: { color: theme.palette.white } }}>
                                    Lorem ipsum dolor sit amet consectetur adipisicing elit. Perferendis nesciunt dolor dolore nemo aperiam tenetur ullam, tempora nostrum nisi voluptates quod delectus, repudiandae suscipit consequuntur voluptate voluptatum dolorum animi illum.
                                </Text>
                                {/* <ReactMarkdown>{template?.description ?? undefined as any}</ReactMarkdown> */}
                            </Stack.Item>
                        </Stack>

                        {/* <Stack tokens={{ childrenGap: '20px' }} styles={{ root: { padding: '24px 8px' } }}> */}

                        {/* </Stack> */}
                    </PivotItem>
                    <PivotItem headerText='Settings' itemKey='Settings'>
                        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: { padding: '24px 8px' } }}>
                        </Stack>
                    </PivotItem>
                </Pivot>
            </Stack.Item>
        </Stack>
    ) : <></>;
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
        <Stack.Item grow={props.grow} styles={{ root: { minWidth: props.minWidth } }}>
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
