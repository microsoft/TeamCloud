// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Checkbox, IColumn, Label, Panel, PanelType, Stack, Text } from '@fluentui/react';
import ReactMarkdown from 'react-markdown';
import { ProjectTemplate } from 'teamcloud';
import { ContentList } from '.';


export interface IProjectTemplateListProps {
    templates?: ProjectTemplate[]
}

export const ProjectTemplateList: React.FC<IProjectTemplateListProps> = (props) => {

    const [selectedTemplate, setSelectedTemplate] = useState<ProjectTemplate>();
    const [panelIsOpen, setPanelIsOpen] = useState(false);

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 240, fieldName: 'displayName' },
        { key: 'isDefault', name: 'Default', minWidth: 120, onRender: (t: ProjectTemplate) => <Checkbox checked={t.isDefault} disabled /> },
        { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
        { key: 'repository', name: 'Repository', minWidth: 460, onRender: (t: ProjectTemplate) => t.repository.url },
    ];

    const _onItemInvoked = (template: ProjectTemplate): void => {
        // console.error(template)
        if (template) {
            setSelectedTemplate(template);
            setPanelIsOpen(true);
        } else {
            console.error('nope');
        }
    };

    return (
        <>
            <ContentList
                columns={columns}
                items={props.templates}
                onItemInvoked={_onItemInvoked}
                // filterPlaceholder='Filter scopes'
                buttonText='New template'
                buttonIcon='Add'
            // onButtonClick={() => history.push(`/orgs/${orgId}/projects/new`)}
            />
            <Panel
                isLightDismiss
                headerText={selectedTemplate?.displayName}
                type={PanelType.medium}
                isOpen={panelIsOpen}
                onDismiss={() => { setSelectedTemplate(undefined); setPanelIsOpen(false) }}>
                <Stack tokens={{ childrenGap: '12px' }}>
                    <Stack.Item>
                        <Label >Repository</Label>
                        <Text>{selectedTemplate?.repository.url}</Text>
                    </Stack.Item>
                    <Stack.Item>
                    </Stack.Item>
                    <Stack.Item>
                        <ReactMarkdown>{selectedTemplate?.description ?? undefined as any}</ReactMarkdown>
                    </Stack.Item>
                </Stack>
            </Panel>
        </>
    );
}
