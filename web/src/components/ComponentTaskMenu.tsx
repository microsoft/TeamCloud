// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { getTheme, PrimaryButton, Stack } from '@fluentui/react';
import { OrgContext, ProjectContext } from '../Context';
import { ComponentTaskDefinition, ComponentTaskTemplate } from 'teamcloud';
import { api } from '../API';
// import { stringify } from 'querystring';

export interface IComponentTaskMenuProps {

}

export const ComponentTaskMenu: React.FunctionComponent<IComponentTaskMenuProps> = (props) => {

    const theme = getTheme();

    const { org } = useContext(OrgContext);
    const { component, templates, onComponentTaskSelected } = useContext(ProjectContext);

    const [taskTemplates, setTaskTemplates] = useState<ComponentTaskTemplate[]>();

    useEffect(() => {
        if (org && component && templates && templates.length > 0) {
            let template = templates.find(t => t.id === component.templateId);
            if (template && template.tasks) {
                // console.log(stringify(template.tasks));
                setTaskTemplates(template.tasks);
                return;
            }
        }
        setTaskTemplates(new Array<ComponentTaskTemplate>());
    }, [org, component, templates]);

    const onClickTaskButton = async (componentTaskTemplate: ComponentTaskTemplate) => {
        if (org && component) {
            console.log(`- createTask`);
            const result = await api.createComponentTask(org.id, component.projectId, component.id, {
                body: {
                    taskId: componentTaskTemplate.id
                } as ComponentTaskDefinition
            })
            if (result.data) {
                onComponentTaskSelected(result.data);
            } else {
                console.error(result);
            }
            console.log(`+ createTask`);
        }
    }

    return (
        <Stack horizontal tokens={{ childrenGap: '6px' }}>
            { taskTemplates ? taskTemplates.map(tt => (
                <Stack.Item
                    key={tt.id}>
                    <PrimaryButton
                        theme={theme}
                        text={tt.displayName ?? ''}
                        alt={tt.description ?? ''}
                        onClick={() => onClickTaskButton(tt)} />
                </Stack.Item>
            )) : []}
        </Stack>
    );
}
