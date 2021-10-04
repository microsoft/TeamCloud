// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { DefaultButton, getTheme, Separator, Stack } from '@fluentui/react';
import { ComponentTaskTemplate } from 'teamcloud';
import { useCreateProjectComponentTask, useProjectComponent, useProjectComponentTemplates, useDeleteProjectComponent } from '../hooks';
import { ConfirmationButton } from './common/ConfirmationButton';

export interface IComponentTaskMenuProps { }

export const ComponentTaskMenu: React.FunctionComponent<IComponentTaskMenuProps> = (props) => {

    const theme = getTheme();

    const { data: component } = useProjectComponent();
    const { data: templates } = useProjectComponentTemplates();

    const createComponentTask = useCreateProjectComponentTask();
    const deleteComponent = useDeleteProjectComponent();

    const [taskTemplates, setTaskTemplates] = useState<ComponentTaskTemplate[]>();

    useEffect(() => {
        if (component && templates && templates.length > 0) {
            let template = templates.find(t => t.id === component.templateId);
            if (template && template.tasks) {
                // console.log(stringify(template.tasks));
                setTaskTemplates(template.tasks);
                return;
            }
        }
        setTaskTemplates(new Array<ComponentTaskTemplate>());
    }, [component, templates]);

    const onClickTaskButton = async (componentTaskTemplate: ComponentTaskTemplate) => {
        if (component && componentTaskTemplate.id) {
            await createComponentTask({
                taskId: componentTaskTemplate.id
            });
        }
    }

    const onClickDeleteButton = async () => {
        if (component) {
            await deleteComponent(component);
        }
    }

    return (
        <>
            <Stack horizontal tokens={{ childrenGap: '6px' }}>
                {taskTemplates && component?.deleted === undefined ? taskTemplates.map((tt, i) => (
                    <Stack.Item
                        key={tt.id}>
                        <DefaultButton
                            // key={tt.id}
                            theme={theme}
                            text={tt.displayName ?? ''}
                            alt={tt.description ?? ''}
                            onClick={() => onClickTaskButton(tt)} />
                    </Stack.Item>
                )) : []}
                {(taskTemplates && taskTemplates.length > 0) && (<Stack.Item key='Seperator'><Separator vertical /></Stack.Item>)}
                <Stack.Item
                    key='delete'>
                    <ConfirmationButton
                        theme={theme}
                        text='Delete'
                        hidden={!(component)}
                        styles={{
                            root: { backgroundColor: theme.palette.red, border: '1px solid transparent' },
                            rootHovered: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                            rootPressed: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                            label: { fontWeight: 700 }
                        }}
                        disabled={component === undefined || component.deleted !== undefined}
                        confirmationTitle='Delete component'
                        confirmationBody={`Do you want to delete component ${component?.displayName}?`}
                        onClick={() => onClickDeleteButton()} />
                </Stack.Item>
            </Stack>

        </>
    );
}
