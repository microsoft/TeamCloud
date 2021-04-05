// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { getTheme, PrimaryButton, Stack } from '@fluentui/react';
import { ComponentTaskTemplate } from 'teamcloud';
import { useCreateProjectComponentTask, useOrg, useProjectComponent, useProjectComponentTemplates } from '../hooks';

export interface IComponentTaskMenuProps { }

export const ComponentTaskMenu: React.FunctionComponent<IComponentTaskMenuProps> = (props) => {

    const theme = getTheme();

    const { data: org } = useOrg();
    const { data: component } = useProjectComponent();
    const { data: templates } = useProjectComponentTemplates();
    const createComponentTask = useCreateProjectComponentTask();

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
        if (org && component && componentTaskTemplate.id) {
            await createComponentTask({
                taskId: componentTaskTemplate.id
            });
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
