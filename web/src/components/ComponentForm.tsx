// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { DefaultButton, Dropdown, FontIcon, getTheme, IColumn, IconButton, IDropdownOption, Image, Persona, PersonaSize, PrimaryButton, Separator, Stack, Text, TextField } from '@fluentui/react';
import { ProjectComponentDefinition, ComponentTemplate } from 'teamcloud';
import { useHistory } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import { FuiForm } from '@rjsf/fluent-ui'
import { FieldTemplateProps, ISubmitEvent } from '@rjsf/core';
import { ContentContainer, ContentHeader, ContentList, ContentProgress } from '.';
import { OrgContext, ProjectContext } from '../Context';
import DevOps from '../img/devops.svg';
import GitHub from '../img/github.svg';
import Resource from '../img/resource.svg';
import { api } from '../API';


export const ComponentForm: React.FC = () => {

    const history = useHistory();

    const [template, setTemplate] = useState<ComponentTemplate>();
    const [deploymentScopeOptions, setDeploymentScopeOptions] = useState<IDropdownOption[]>();

    const [formEnabled, setFormEnabled] = useState<boolean>(true);

    const [displayName, setDisplayName] = useState<string>();
    const [deploymentScopeId, setDeploymentScopeId] = useState<string>();

    const { org, scopes } = useContext(OrgContext);
    const { project, templates, onComponentSelected } = useContext(ProjectContext);

    const theme = getTheme();

    useEffect(() => {
        if (project && scopes) {
            console.log(`+ setDeploymentScopeOptions (${project.slug})`);
            const options = scopes.map(s => ({ key: s.id, text: s.displayName ?? s.id } as IDropdownOption))
            setDeploymentScopeOptions(options);
            if (scopes.length === 1)
                setDeploymentScopeId(scopes[0].id);
        }
    }, [project, scopes]);


    const _submitForm = async (e: ISubmitEvent<any>) => {
        if (org && project && template && displayName && e.formData) {
            setFormEnabled(false);

            const componentDef: ProjectComponentDefinition = {
                displayName: displayName,
                templateId: template.id,
                inputJson: JSON.stringify(e.formData),
                deploymentScopeId: deploymentScopeId
            };

            const componentResult = await api.createProjectComponent(project.organization, project.id, { body: componentDef });
            const component = componentResult.data;

            if (component) {
                onComponentSelected(component);
                history.push(`/orgs/${org.slug}/projects/${project.slug}/components/${component.slug}`);
            } else {
                console.error(componentResult);
            }
        }
    };

    // const _resetAndCloseForm = () => {
    //     setComponentTemplate(undefined);
    //     setFormEnabled(true);
    //     // onFormClose();
    // };



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

    const _getRepoImage = (template: ComponentTemplate) => {
        switch (template.repository.provider) {
            // case 'Unknown': return;
            case 'DevOps': return DevOps;
            case 'GitHub': return GitHub;
        }
        return undefined;
    };

    const _getTypeIcon = (template: ComponentTemplate) => {
        if (template.type)
            switch (template.type) { // VisualStudioIDELogo32
                case 'Custom': return 'Link'; // Link12, FileSymlink, OpenInNewWindow, VSTSLogo
                case 'Readme': return 'PageList'; // Preview, Copy, FileHTML, FileCode, MarkDownLanguage, Document
                case 'Environment': return 'AzureLogo'; // Processing, Settings, Globe, Repair
                case 'AzureResource': return 'AzureLogo'; // AzureServiceEndpoint
                case 'GitRepository': return 'OpenSource';
                default: return undefined;
            }
    };

    const onRenderNameColumn = (template?: ComponentTemplate, index?: number, column?: IColumn) => {
        if (!template) return undefined;
        const name = template.displayName?.replaceAll('-', ' ');
        return (
            <Stack tokens={{ padding: '5px' }}>
                <Persona
                    text={name}
                    size={PersonaSize.size32}
                    imageUrl={_getTypeImage(template)}
                    coinProps={{ styles: { initials: { borderRadius: '4px' } } }}
                    styles={{
                        root: { color: 'inherit' },
                        primaryText: { color: 'inherit', textTransform: 'capitalize' }
                    }} />
            </Stack>
        );
    };

    const onRenderTypeColumn = (template?: ComponentTemplate, index?: number, column?: IColumn) => {
        if (!template) return undefined;
        return (
            <Stack horizontal >
                <FontIcon iconName={_getTypeIcon(template)} className='component-type-icon' />
                <Text styles={{ root: { paddingLeft: '4px' } }}>{template.type}</Text>
            </Stack>
        )
    };

    // const onRenderDescriptionColumn = (template?: ComponentTemplate, index?: number, column?: IColumn) => {

    // };

    const onRenderRepoColumn = (template?: ComponentTemplate, index?: number, column?: IColumn) => {
        if (!template) return undefined;
        const name = template.repository.repository?.replaceAll('-', ' ') ?? template.repository.url;
        return (
            <Stack horizontal >
                <Image src={_getRepoImage(template)} styles={{ image: { width: '18px', height: '18px' } }} />
                <Text styles={{ root: { paddingLeft: '4px' } }}>{name}</Text>
            </Stack>
        )
    };

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 220, maxWidth: 220, isResizable: false, onRender: onRenderNameColumn, styles: { cellName: { paddingLeft: '5px' } } },
        { key: 'type', name: 'Type', minWidth: 160, maxWidth: 160, isResizable: false, onRender: onRenderTypeColumn },
        { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
        { key: 'blank', name: '', minWidth: 40, maxWidth: 40, onRender: (_: ComponentTemplate) => undefined },
        { key: 'repository', name: 'Repository', minWidth: 240, maxWidth: 240, onRender: onRenderRepoColumn },
        { key: 'version', name: 'Version', minWidth: 80, maxWidth: 80, onRender: (t: ComponentTemplate) => t.repository.version },
    ];

    const _applyFilter = (template: ComponentTemplate, filter: string): boolean => {
        const f = filter?.toUpperCase();
        if (!f) return true;
        return (
            template.displayName?.toUpperCase().includes(f)
            || template.id?.toUpperCase().includes(f)
            || template.description?.toUpperCase().includes(f)
            || template.parentId?.toUpperCase().includes(f)
            || template.repository.organization?.toUpperCase().includes(f)
            || template.repository.project?.toUpperCase().includes(f)
            || template.repository.repository?.toUpperCase().includes(f)
            || template.repository.url?.toUpperCase().includes(f)
            || template.type?.toUpperCase().includes(f)
        ) ?? false

    };

    const _onItemInvoked = (template: ComponentTemplate): void => {
        setTemplate(template);
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setDeploymentScopeId((scopes && option) ? scopes.find(s => s.id === option.key)?.id : undefined);
    };

    return (
        <>
            <ContentProgress progressHidden={formEnabled && project !== undefined && templates !== undefined} />
            <ContentHeader title='New Component'>
                <IconButton iconProps={{ iconName: 'ChromeClose' }}
                    onClick={() => history.push(`/orgs/${org?.slug}/projects/${project?.slug}`)} />
            </ContentHeader>
            <ContentContainer>
                {/* <ComponentForm /> */}


                <Stack tokens={{ childrenGap: '40px' }}>
                    <Stack.Item>
                        <ContentList
                            columns={columns}
                            items={template ? [template] : templates}
                            onItemInvoked={_onItemInvoked}
                            noCheck
                            noHeader={template !== undefined}
                            applyFilter={template ? undefined : _applyFilter}
                            filterPlaceholder='Filter components' />
                    </Stack.Item>
                    {template && (
                        <Stack.Item>
                            <Stack horizontal tokens={{ childrenGap: '40px' }}>
                                <Stack.Item grow styles={{ root: { minWidth: '40%', } }}>
                                    <Stack
                                        styles={{ root: { paddingTop: '20px' } }}
                                        tokens={{ childrenGap: '20px' }}>
                                        <Stack.Item>
                                            <TextField
                                                required
                                                label='Name'
                                                // description='Component display name'
                                                disabled={!formEnabled}
                                                value={displayName}
                                                onChange={(_ev, val) => setDisplayName(val)} />
                                        </Stack.Item>
                                        <Stack.Item>
                                            <Dropdown
                                                required
                                                label='Deployment scope'
                                                disabled={!formEnabled}
                                                options={deploymentScopeOptions || []}
                                                onChange={_onDropdownChange} />
                                        </Stack.Item>
                                        <Stack.Item>
                                            <Separator styles={{ root: { selectors: { '::before': { backgroundColor: theme.palette.neutralQuaternary } } } }} />
                                        </Stack.Item>
                                        <Stack.Item>
                                            <FuiForm
                                                disabled={!formEnabled}
                                                onSubmit={_submitForm}
                                                FieldTemplate={TCFieldTemplate}
                                                // widgets={{ 'SelectWidget': TCSelectWidget }}
                                                schema={template?.inputJsonSchema ? JSON.parse(template.inputJsonSchema) : {}}>
                                                <Separator styles={{ root: { selectors: { '::before': { backgroundColor: theme.palette.neutralQuaternary } } } }} />
                                                <div style={{ paddingTop: '24px' }}>
                                                    <PrimaryButton type='submit' text='Create component' disabled={!formEnabled || !(template)} styles={{ root: { marginRight: 8 } }} />
                                                    <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => setTemplate(undefined)} />
                                                </div>
                                            </FuiForm>
                                        </Stack.Item>
                                    </Stack>
                                </Stack.Item>
                                <Stack.Item grow styles={{
                                    root: {
                                        minWidth: '40%',
                                        padding: '10px 40px',
                                        borderRadius: theme.effects.roundedCorner4,
                                        boxShadow: theme.effects.elevation4,
                                        backgroundColor: theme.palette.white
                                    }
                                }}>
                                    <ReactMarkdown>{template?.description ?? undefined as any}</ReactMarkdown>
                                </Stack.Item>
                            </Stack>
                        </Stack.Item>
                    )}
                </Stack>
            </ContentContainer>
        </>
    );
}


export const TCFieldTemplate: React.FC<FieldTemplateProps> = (props) => {
    return props.id === 'root' ? (
        <Stack styles={{ root: { minWidth: '460px' } }} tokens={{ childrenGap: '14px' }}>
            {props.children}
        </Stack>
    ) : (
            <Stack.Item grow styles={{ root: { paddingBottom: '16px' } }}>
                {props.children}
            </Stack.Item>
        );
}
