import React, { useState, useEffect } from "react";
import { CommandBar, ICommandBarItemProps, SearchBox, Stack, Breadcrumb, IBreadcrumbItem, Separator, IStackStyles, ICommandBarStyles, Panel } from '@fluentui/react';
import { getProjects } from '../API'
import { Project, DataResult } from '../model'
import { ProjectList, ProjectForm } from "../components";

export interface IHomeViewProps {
    onProjectSelected?: (project: Project) => void;
}

export const HomeView: React.FunctionComponent<IHomeViewProps> = (props) => {

    const [projects, setProjects] = useState<Project[]>();
    const [projectFilter, setProjectFilter] = useState<string>();
    const [panelOpen, setPanelOpen] = useState(false);

    useEffect(() => {
        if (projects === undefined) {
            const _setProjects = async () => {
                const result = await getProjects();
                const data = (result as DataResult<Project[]>).data;
                setProjects(data);
            };
            _setProjects();
        }
    }, []);

    const _refresh = async () => {
        let result = await getProjects();
        let data = (result as DataResult<Project[]>).data;
        setProjects(data);
    }

    const _commandBarItems: ICommandBarItemProps[] = [
        { key: 'search', onRender: () => <SearchBox className="searchBox" iconProps={{ iconName: 'Filter' }} placeholder="Filter" onChange={(_, filter) => setProjectFilter(filter)} /> },
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'refresh' }, onClick: () => { _refresh() } },
        { key: 'create', text: 'Create', iconProps: { iconName: 'CirclePlus' }, onClick: () => { setPanelOpen(true) } },
    ];

    const breadcrumbs: IBreadcrumbItem[] = [
        { text: 'Projects', key: 'projects', href: '/', isCurrentItem: true }
    ];

    return (
        <>
            <Stack styles={{ root: { paddingTop: '8px' } }}>
                <Stack horizontal
                    verticalFill
                    horizontalAlign='space-between'
                    verticalAlign='baseline'
                    styles={{ root: {} }}>
                    <Stack.Item>
                        <Breadcrumb
                            items={breadcrumbs}
                            styles={{ root: { minWidth: '100px', marginLeft: '24px' } }} />
                    </Stack.Item>
                    <Stack.Item>
                        <CommandBar
                            styles={{ root: { minWidth: '333px' }, primarySet: { alignItems: 'baseline' } }}
                            items={_commandBarItems} />
                    </Stack.Item>
                </Stack>
                <Separator />
                <ProjectList
                    projects={projects}
                    projectFilter={projectFilter}
                    onProjectSelected={props.onProjectSelected} />
            </Stack>
            <Panel
                isOpen={panelOpen}
                onDismiss={() => setPanelOpen(false)}>
                <ProjectForm />
            </Panel>
        </>
    );
}
