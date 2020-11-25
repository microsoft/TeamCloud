// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Checkbox, CheckboxVisibility, DetailsList, DetailsListLayoutMode, FontWeights, getTheme, IColumn, IDetailsHeaderProps, IDetailsRowProps, IRenderFunction, ITextStyles, Panel, PanelType, PrimaryButton, Stack, Text } from '@fluentui/react';
import { useParams } from 'react-router-dom';
import { Organization, ProjectTemplate } from 'teamcloud';
import { api } from '../API';
import ReactMarkdown from 'react-markdown';


export interface IOrgSettingsProjectTemplateProps {
    templates?: ProjectTemplate[]
}

export const OrgSettingsProjectTemplate: React.FunctionComponent<IOrgSettingsProjectTemplateProps> = (props) => {

    let { orgId } = useParams() as { orgId: string };

    // const [templates, setTemplates] = useState<ProjectTemplate[]>();
    // const [memberFilter, setMemberFilter] = useState<string>();
    const [selectedTemplate, setSelectedTemplate] = useState<ProjectTemplate>();
    const [panelIsOpen, setPanelIsOpen] = useState(false);

    // useEffect(() => {
    //     if (orgId) {
    //         const _setTemplates = async () => {
    //             let _templates = await api.getProjectTemplates(orgId);
    //             setTemplates(_templates.data ?? undefined)
    //         };
    //         _setTemplates();
    //     }
    // }, [orgId]);

    const theme = getTheme();

    const columns: IColumn[] = [
        { key: 'displayName', name: 'Name', minWidth: 240, fieldName: 'displayName' },
        { key: 'isDefault', name: 'Default', minWidth: 240, onRender: (t: ProjectTemplate) => <Checkbox checked={t.isDefault} disabled /> },
        { key: 'description', name: 'Description', minWidth: 460, fieldName: 'description' },
        { key: 'repository', name: 'Repository', minWidth: 460, onRender: (t: ProjectTemplate) => t.repository.url },
    ];

    const _onLinkClicked = (template: ProjectTemplate): void => {
        // if (props.onProjectSelected)
        //     props.onProjectSelected(project);
    }

    const _onItemInvoked = (template: ProjectTemplate): void => {
        console.error(template)
        if (template) {
            setSelectedTemplate(template);
            setPanelIsOpen(true);
            // _onLinkClicked(template);
            // history.push(`${orgId}/projects/${project.slug}`);
        } else {
            console.error('nope');
        }
    };

    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (props?: IDetailsRowProps, defaultRender?: (props?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (props) props.styles = { fields: { alignItems: 'center' }, check: { minHeight: '62px' }, cell: { fontSize: '14px' } }
        return defaultRender ? defaultRender(props) : null;
    };

    const _onRenderDetailsHeader: IRenderFunction<IDetailsHeaderProps> = (props?: IDetailsHeaderProps, defaultRender?: (props?: IDetailsHeaderProps) => JSX.Element | null): JSX.Element | null => {
        if (props) props.styles = { root: { paddingTop: '8px' } }
        return defaultRender ? defaultRender(props) : null;
    };

    const _titleStyles: ITextStyles = {
        root: {
            fontSize: '14px',
            fontWeight: FontWeights.regular,
        }
    }

    const _calloutStyles: ITextStyles = {
        root: {
            fontSize: '11px',
            fontWeight: FontWeights.regular,
            color: 'rgb(102, 102, 102)',
            backgroundColor: theme.palette.neutralLighter,
            padding: '2px 9px',
            borderRadius: '14px',
        }
    }


    if (props.templates === undefined)
        return (<></>);

    if (props.templates.length === 0)
        return (<Text styles={{ root: { width: '100%', paddingLeft: '8px' } }}>No Project Templates</Text>)

    return (
        <>
            <Stack styles={{
                root: {
                    borderRadius: theme.effects.roundedCorner4,
                    boxShadow: theme.effects.elevation4,
                    backgroundColor: theme.palette.white
                }
            }} >
                <Stack horizontal verticalFill verticalAlign='baseline' horizontalAlign='space-between'
                    styles={{ root: { padding: '16px 16px 0px 16px', } }}>
                    <Stack.Item>
                        <Stack horizontal verticalFill verticalAlign='baseline' tokens={{ childrenGap: '5px' }}>
                            <Stack.Item>
                                <Text styles={_titleStyles}>Total</Text>
                            </Stack.Item>
                            <Stack.Item>
                                <Text styles={_calloutStyles}>{props.templates.length}</Text>
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
                    <Stack.Item>
                        <PrimaryButton
                            disabled={orgId === undefined}
                            iconProps={{ iconName: 'Add' }}
                            text='New template'
                        // onClick={() => history.push(`/orgs/${orgId}/projects/new`)}
                        />
                    </Stack.Item>
                </Stack>
                <DetailsList
                    items={props.templates}
                    columns={columns}
                    // isHeaderVisible={false}
                    onRenderRow={_onRenderRow}
                    onRenderDetailsHeader={_onRenderDetailsHeader}
                    // selectionMode={SelectionMode.none}
                    layoutMode={DetailsListLayoutMode.justified}
                    checkboxVisibility={CheckboxVisibility.always}
                    selectionPreservedOnEmptyClick={true}
                    onItemInvoked={_onItemInvoked} />
            </Stack>
            <Panel
                isLightDismiss
                headerText={selectedTemplate?.displayName}
                type={PanelType.medium}
                isOpen={panelIsOpen}
                onDismiss={() => { setSelectedTemplate(undefined); setPanelIsOpen(false) }}>
                <Stack tokens={{ childrenGap: '12px' }}>
                    <Stack.Item>
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
