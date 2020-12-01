// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Checkbox, IColumn, Label, Panel, PanelType, Stack, Text } from '@fluentui/react';
import { useHistory, useParams } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import { ProjectTemplate } from 'teamcloud';
import { ContentList } from '.';
import collaboration from '../img/MSC17_collaboration_010_noBG.png'


export interface IProjectTemplateListProps {
    templates?: ProjectTemplate[]
}

export const ProjectTemplateList: React.FC<IProjectTemplateListProps> = (props) => {

    const history = useHistory();
    const { orgId } = useParams() as { orgId: string };

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
                filterPlaceholder='Filter templates'
                buttonText='New template'
                buttonIcon='Add'
                onButtonClick={() => history.push(`/orgs/${orgId}/settings/templates/new`)}
                noDataTitle='You do not have any project templates yet'
                noDataImage={collaboration}
                noDataDescription='Project templates are...'
                noDataButtonText='Create template'
                noDataButtonIcon='Add'
                onNoDataButtonClick={() => history.push(`/orgs/${orgId}/settings/templates/new`)}
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
