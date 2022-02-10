// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Checkbox, IColumn, IconButton, Label, Modal, Stack, Text } from '@fluentui/react';
import ReactMarkdown from 'react-markdown';
import { ProjectTemplate } from 'teamcloud';
import { ContentList, ContentSeparator } from '.';
import { useProjectTemplates, useUrl } from '../hooks';

import collaboration from '../img/MSC17_collaboration_010_noBG.png'

export const ProjectTemplateList: React.FC = () => {

    const navigate = useNavigate();

    const { orgId } = useUrl() as { orgId: string };

    const { data: templates } = useProjectTemplates();

    const [selectedTemplate, setSelectedTemplate] = useState<ProjectTemplate>();
    const [modalIsOpen, setModalIsOpen] = useState(false);

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 240, fieldName: 'displayName' },
        { key: 'isDefault', name: 'Default', minWidth: 120, onRender: (t: ProjectTemplate) => <Checkbox checked={t.isDefault} disabled /> },
        { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
        { key: 'repository', name: 'Repository', minWidth: 460, onRender: (t: ProjectTemplate) => t.repository.url },
    ];

    const _onItemInvoked = (template: ProjectTemplate): void => {
        if (template) {
            setSelectedTemplate(template);
            setModalIsOpen(true);
        } else {
            console.error('nope');
        }
    };

    return (
        <>
            <ContentList
                columns={columns}
                items={templates ?? undefined}
                onItemInvoked={_onItemInvoked}
                filterPlaceholder='Filter templates'
                buttonText='New template'
                buttonIcon='Add'
                onButtonClick={() => navigate(`/orgs/${orgId}/settings/templates/new`)}
                noDataTitle='You do not have any project templates yet'
                noDataImage={collaboration}
                noDataDescription='Project templates are...'
                noDataButtonText='Create template'
                noDataButtonIcon='Add'
                onNoDataButtonClick={() => navigate(`/orgs/${orgId}/settings/templates/new`)}
            />
            <Modal
                // isLightDismiss
                // headerText={selectedTemplate?.displayName}
                // type={PanelType.medium}
                styles={{ main: { margin: 'auto 100px' }, scrollableContent: { padding: '50px' } }}
                isBlocking={false}
                isOpen={modalIsOpen}
                onDismiss={() => { setSelectedTemplate(undefined); setModalIsOpen(false) }}>
                <Stack tokens={{ childrenGap: '12px' }}>
                    <Stack.Item>
                        <Stack horizontal horizontalAlign='space-between'>
                            <Stack.Item>
                                <Text variant='xxLargePlus'>{selectedTemplate?.displayName}</Text>
                            </Stack.Item>
                            <Stack.Item>
                                <IconButton iconProps={{ iconName: 'ChromeClose' }}
                                    onClick={() => setModalIsOpen(false)} />
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
                    <Label >Repository</Label>
                    <Text>{selectedTemplate?.repository.url}</Text>
                    <Stack.Item>
                    </Stack.Item>
                    <Stack.Item>
                        <ContentSeparator />
                    </Stack.Item>
                    <Stack.Item>
                        <ReactMarkdown>{selectedTemplate?.description ?? undefined as any}</ReactMarkdown>
                    </Stack.Item>
                </Stack>
            </Modal>
        </>
    );
}
