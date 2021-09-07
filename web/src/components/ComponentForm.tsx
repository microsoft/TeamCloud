// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory } from 'react-router-dom';
import { ActionButton, DefaultButton, Dropdown, FontIcon, getTheme, IColumn, IconButton, IDropdownOption, Image, Persona, PersonaSize, PrimaryButton, Stack, Text, TextField } from '@fluentui/react';
import ReactMarkdown from 'react-markdown';
import gfm from 'remark-gfm'
import { FuiForm } from '@rjsf/fluent-ui'
import { ISubmitEvent } from '@rjsf/core';
import { ComponentDefinition, ComponentTemplate } from 'teamcloud';
import { ContentContainer, ContentHeader, ContentList, ContentProgress, ContentSeparator, TCFieldTemplate } from '.';
import { useOrg, useProject, useDeploymentScopes, useCreateProjectComponent, useProjectComponentTemplates } from '../hooks';

import DevOps from '../img/devops.svg';
import GitHub from '../img/github.svg';
import Resource from '../img/resource.svg';

export const ComponentForm: React.FC = () => {

    const titleRe = /^(?:#+.*)$/gm
    const deviderRe = /^(?:-{3,})$/gm
    const imageOrLinkRe = /^(?:!?\[[^\]]*\]\([^[\]()]*\))$/gm

    const history = useHistory();

    const [template, setTemplate] = useState<ComponentTemplate>();
    const [deploymentScopeOptions, setDeploymentScopeOptions] = useState<IDropdownOption[]>();

    const [formEnabled, setFormEnabled] = useState<boolean>(true);

    const [displayName, setDisplayName] = useState<string>();
    const [deploymentScopeId, setDeploymentScopeId] = useState<string>();

    const { data: org, isLoading: orgIsLoading } = useOrg();
    const { data: scopes, isLoading: scopesIsLoading } = useDeploymentScopes();
    const { data: project, isLoading: projectIsLoading } = useProject();
    const { data: templates, isLoading: templatesIsLoading } = useProjectComponentTemplates();

    const createComponent = useCreateProjectComponent();

    const theme = getTheme();

    useEffect(() => {
        if (project && scopes) {
            const options = scopes
                .filter(s => s.authorized && s.componentTypes && s.componentTypes.includes(template?.type as string))
                .map(s => ({ key: s.id, text: s.displayName ?? s.id } as IDropdownOption));
            setDeploymentScopeOptions(options);
            if (scopes.length === 1)
                setDeploymentScopeId(scopes[0].id);
        }
    }, [project, scopes, template]);


    const _submitForm = async (e: ISubmitEvent<any>) => {
        if (org && project && template && displayName && (e.formData || template.inputJsonSchema === null)) {
            setFormEnabled(false);

            const componentDef: ComponentDefinition = {
                displayName: displayName,
                templateId: template.id,
                inputJson: JSON.stringify(e.formData),
                deploymentScopeId: deploymentScopeId
            };

            await createComponent(componentDef);
        }
    };

    const _getTypeIcon = (template: ComponentTemplate) => {
        switch (template?.type.toLowerCase()) {
            case 'environment': return 'AzureLogo';
            case 'repository': return 'OpenSource';
        }
        console.log(`Icon for component type '${template?.type}' not found`);
        return undefined;
    };

    const _getTypeImage = (template: ComponentTemplate) => {
        switch (template?.type.toLowerCase()) {
            case 'environment': return Resource;
            case 'repository': return Resource;
        }
        console.log(`Icon for component type '${template?.type}' not found`);
        return Resource;
    };

    const _getRepoImage = (template: ComponentTemplate) => {
        switch (template?.repository.provider) {
            // case 'Unknown': return;
            case 'DevOps': return DevOps;
            case 'GitHub': return GitHub;
        }
        return undefined;
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
        { key: 'type', name: 'Resource', minWidth: 160, maxWidth: 160, isResizable: false, onRender: onRenderTypeColumn },
        { key: 'description', name: 'Description', minWidth: 460, onRender: (t: ComponentTemplate) => t.description?.replace(imageOrLinkRe, '').replace(titleRe, '').replace(deviderRe, '') },
        { key: 'blank', name: '', minWidth: 40, maxWidth: 40, onRender: (_: ComponentTemplate) => undefined },
        { key: 'repository', name: 'Repository', minWidth: 240, maxWidth: 240, onRender: onRenderRepoColumn },
        { key: 'version', name: 'Version', minWidth: 80, maxWidth: 80, onRender: (t: ComponentTemplate) => t.repository.version },
    ];


    const _onItemInvoked = (template: ComponentTemplate): void => {
        setTemplate(template);
    };

    const _onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        setDeploymentScopeId((scopes && option) ? scopes.find(s => s.id === option.key)?.id : undefined);
    };

    const _onBackInvoked = (): void => {
        setFormEnabled(true);
        setTemplate(undefined);
        setDisplayName(undefined);
        setDeploymentScopeId(undefined);
    };


    return (
        <>
            <ContentProgress progressHidden={formEnabled && !orgIsLoading && !scopesIsLoading && !projectIsLoading && !templatesIsLoading} />
            <ContentHeader title='New Component'>
                <IconButton iconProps={{ iconName: 'ChromeClose' }}
                    onClick={() => history.push(`/orgs/${org?.slug}/projects/${project?.slug}`)} />
            </ContentHeader>
            <ContentContainer>
                <Stack tokens={{ childrenGap: '40px' }}>
                    <Stack.Item>
                        <ContentList
                            columns={columns}
                            items={template ? [template] : templates ?? undefined}
                            onItemInvoked={_onItemInvoked}
                            noCheck
                            noHeader={template !== undefined}
                            noSearch={template !== undefined}
                            filterPlaceholder='Filter components' />
                    </Stack.Item>
                    {template && (
                        <Stack.Item>
                            <Stack horizontal tokens={{ childrenGap: '40px' }}>
                                <Stack.Item grow styles={{ root: { minWidth: '40%', } }}>
                                    <Stack
                                        tokens={{ childrenGap: '20px' }}>
                                        <Stack.Item>
                                            <ActionButton
                                                iconProps={{ iconName: 'ChromeBack' }}
                                                styles={{ icon: { marginLeft: '0px' }, textContainer: { paddingBottom: '2px', color: theme.palette.themeDarkAlt } }}
                                                onClick={_onBackInvoked}>
                                                Back to components
                                            </ActionButton>
                                        </Stack.Item>
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
                                        {(template?.inputJsonSchema ? JSON.parse(template.inputJsonSchema) : {}).properties && (
                                            <Stack.Item>
                                                <ContentSeparator />
                                            </Stack.Item>
                                        )}
                                        <Stack.Item>
                                            <FuiForm
                                                disabled={!formEnabled}
                                                onSubmit={_submitForm}
                                                FieldTemplate={TCFieldTemplate}
                                                schema={template?.inputJsonSchema ? JSON.parse(template.inputJsonSchema) : {}}>
                                                <ContentSeparator />
                                                <div style={{ paddingTop: '24px' }}>
                                                    <PrimaryButton type='submit' text='Create component' disabled={!formEnabled || !(template) || (deploymentScopeOptions?.length ?? 0) === 0} styles={{ root: { marginRight: 8 } }} />
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
                                    <ReactMarkdown plugins={[gfm]}>{template?.description ?? undefined as any}</ReactMarkdown>
                                </Stack.Item>
                            </Stack>
                        </Stack.Item>
                    )}
                </Stack>
            </ContentContainer>
        </>
    );
}
