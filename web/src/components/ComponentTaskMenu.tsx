// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { DefaultButton, Dialog, DialogFooter, DialogType, getTheme, PrimaryButton, Separator, Stack } from '@fluentui/react';
import { ComponentTaskTemplate } from 'teamcloud';
import { useCreateProjectComponentTask, useOrg, useProjectComponent, useProjectComponentTemplates, useDeleteProjectComponent } from '../hooks';

export interface IComponentTaskMenuProps { }

export const ComponentTaskMenu: React.FunctionComponent<IComponentTaskMenuProps> = (props) => {

    const theme = getTheme();

    const { data: org } = useOrg();
    const { data: component } = useProjectComponent();
    const { data: templates } = useProjectComponentTemplates();

    const createComponentTask = useCreateProjectComponentTask();
    const deleteComponent = useDeleteProjectComponent();

    const [dialogHidden, setDialogHidden] = useState<boolean>(true);
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

    const onClickDeleteButton = async () => {
        if (org && component) {
            if (dialogHidden) {
                showDialog();
            } else {
                await deleteComponent(component);
                hideDialog();
            }
        }
    }

    const hideDialog = () => setDialogHidden(true);

    const showDialog = () => setDialogHidden(false);

    const dialogContentProps = {
        type: DialogType.normal,
        title: 'Delete Component',
        subText: `Do you want to delete component ${component?.displayName}?`
    };

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
                    <PrimaryButton
                        theme={theme}
                        text='Delete'
                        hidden={!(org && component)}
                        styles={{
                            root: { backgroundColor: theme.palette.red, border: '1px solid transparent' },
                            rootHovered: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                            rootPressed: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                            label: { fontWeight: 700 }
                        }}
                        disabled={component === undefined || component.deleted !== undefined}
                        onClick={() => onClickDeleteButton()} />
                    <Dialog
                        hidden={dialogHidden}
                        onDismiss={hideDialog}
                        dialogContentProps={dialogContentProps}
                        modalProps={{ isBlocking: true }}>
                        <DialogFooter>
                            <PrimaryButton styles={{
                                root: { backgroundColor: theme.palette.red, border: '1px solid transparent' },
                                rootHovered: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                                rootPressed: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                                label: { fontWeight: 700 }
                            }} onClick={onClickDeleteButton} text="Delete" />
                            <DefaultButton onClick={hideDialog} text="Cancel" />
                        </DialogFooter>
                    </Dialog>
                </Stack.Item>
            </Stack>

        </>
    );
}
