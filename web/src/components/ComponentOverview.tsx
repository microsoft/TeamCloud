// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { getTheme, Pivot, PivotItem, Stack, Text } from '@fluentui/react';
import ReactMarkdown from 'react-markdown';
import gfm from 'remark-gfm'
import { FuiForm } from '@rjsf/fluent-ui';
import { ComponentTemplate, DeploymentScope } from 'teamcloud';
import { ProjectMember } from '../model';
import { ComponentTaskList, UserPersona, ComponentLink, ComponentTemplateLink } from '.';
import { useDeploymentScopes, useProjectComponent, useProjectComponentTemplates, useProjectMembers } from '../hooks';

export const ComponentOverview: React.FC = () => {

    const theme = getTheme();

    const { data: scopes } = useDeploymentScopes();

    const { data: members } = useProjectMembers();
    const { data: component } = useProjectComponent(true);
    const { data: templates } = useProjectComponentTemplates();

    const [template, setTemplate] = useState<ComponentTemplate>();
    const [creator, setCreator] = useState<ProjectMember>();
    const [scope, setScope] = useState<DeploymentScope>();
    const [pivotKey, setPivotKey] = useState<string>('Runs');


    useEffect(() => {
        if (component && templates && (template === undefined || component.templateId.toLowerCase() !== template.id.toLowerCase())) {
            // console.log(`+ setComponentTemplate (${component.slug})`);
            const tmpl = templates.find(t => component.templateId.toLowerCase() === t.id.toLowerCase()) ?? undefined
            if (tmpl?.inputJsonSchema) {
                const schema = JSON.parse(tmpl.inputJsonSchema)
                if (schema.properties)
                    Object.keys(schema.properties).forEach(key => {
                        schema.properties[key]['readOnly'] = true
                    });
                tmpl.inputJsonSchema = JSON.stringify(schema)
            }
            setTemplate(tmpl ?? undefined);
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

                    <ComponentOverviewHeaderSection grow title='Resource'>
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
                        <UserPersona principal={creator?.graphPrincipal} showSecondaryText styles={{ root: { minWidth: '220px' } }} />
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
                            {template?.inputJsonSchema && (
                                <Stack.Item styles={{ root: { minWidth: '460px' } }}>
                                    <FuiForm
                                        // widgets={{ 'SelectWidget': ReadonlySelectWidget }}
                                        // FieldTemplate={ReadonlyFieldTemplate}
                                        schema={JSON.parse(template.inputJsonSchema)}
                                        formData={component?.inputJson ? JSON.parse(component.inputJson) : undefined}
                                        onChange={() => { }}>
                                        <></>
                                    </FuiForm>
                                </Stack.Item>
                            )}
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
