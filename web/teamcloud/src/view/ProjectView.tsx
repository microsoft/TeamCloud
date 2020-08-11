import React, { useState, useEffect } from 'react';
import { Project, DataResult } from '../model';
import { getProject } from '../API';
import { Stack, Text, Spinner, IBreadcrumbItem, IStackStyles, Breadcrumb, CommandBar, ICommandBarItemProps, Separator, Label, IStackTokens, ITextStyles, getTheme, FontWeights, Link } from '@fluentui/react';

export interface IProjectViewProps {
    project?: Project
    projectId: string;
    onProjectSelected?: (project: Project) => void;
}

export const ProjectView: React.FunctionComponent<IProjectViewProps> = (props) => {

    const [project, setProject] = useState(props.project);

    useEffect(() => {
        if (project === undefined) {
            const _setProject = async () => {
                const result = await getProject(props.projectId);
                const data = (result as DataResult<Project>).data;
                setProject(data);
                if (props.onProjectSelected)
                    props.onProjectSelected(data)
            };
            _setProject();
        }
    }, []);

    const _refresh = async () => {
        let result = await getProject(props.project?.id ?? props.projectId);
        let data = (result as DataResult<Project>).data;
        setProject(data);
        if (props.onProjectSelected)
            props.onProjectSelected(data)
    }

    const _ensureBreadcrumb = () => {
        if (project && breadcrumbs.length === 1)
            breadcrumbs.push({ text: project.name, key: 'project', isCurrentItem: true })
    }

    const _commandBarItems: ICommandBarItemProps[] = [
        // { key: 'search', onRender: () => <SearchBox className="searchBox" iconProps={{ iconName: 'Filter' }} placeholder="Filter" onChange={(_, filter) => setProjectFilter(filter)} /> },
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'refresh' }, onClick: () => { _refresh() } },
        // { key: 'create', text: 'Create', iconProps: { iconName: 'CirclePlus' }, onClick: () => { _refresh() } },
    ];

    const breadcrumbs: IBreadcrumbItem[] = [
        { text: 'Projects', key: 'projects', href: '/' }
    ];


    const theme = getTheme();

    const dataStackTokens: IStackTokens = { childrenGap: 10 };

    const headingStyles: ITextStyles = {
        root: {
            fontSize: theme.fonts.xLarge.fontSize,
            fontWeight: FontWeights.bold,
            marginTop: '24px',
            selectors: {
                ':not(:first-child)': {
                    marginTop: '24px',
                }
            }

        }
    }

    const userViews = () => {
        if (!project)
            return <></>

        const views = project.users.map(user => {

        });
    }

    if (project?.id) {

        _ensureBreadcrumb();
        return (
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
                            styles={{ root: { minWidth: '86px' }, primarySet: { alignItems: 'baseline' } }}
                            items={_commandBarItems} />
                    </Stack.Item>
                </Stack>
                <Separator />
                <Stack horizontal
                    horizontalAlign='space-evenly'
                    verticalAlign='baseline'
                    styles={{ root: {} }}
                    tokens={{ padding: '0 15%', childrenGap: 100 }}>
                    <Stack verticalFill styles={{ root: { width: '100%' } }}>
                        <Text styles={headingStyles}>Project</Text>
                        <Separator></Separator>
                        <Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>ID:</Label>
                                <Text>{project.id}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Name:</Label>
                                <Text>{project.name}</Text>
                            </Stack>
                        </Stack>
                        <Text styles={headingStyles}>Project Type</Text>
                        <Separator></Separator>
                        <Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>ID:</Label>
                                <Text>{project.type.id}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Default:</Label>
                                <Text>{project.type.isDefault ? 'Yes' : 'No'}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Location:</Label>
                                <Text>{project.type.region}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Providers:</Label>
                                <Text>{project.type.providers.map(p => p.id).join()}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Subscription Capacity:</Label>
                                <Text>{project.type.subscriptionCapacity}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Subscriptions:</Label>
                                <Text>{project.type.subscriptions.join()}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Resource Group Name Prefix:</Label>
                                <Text>{project.type.resourceGroupNamePrefix}</Text>
                            </Stack>
                        </Stack>
                        <Text styles={headingStyles}>Resource Group</Text>
                        <Separator></Separator>
                        <Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Name</Label>
                                <Text>{project.resourceGroup.name}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Location</Label>
                                <Text>{project.resourceGroup.region}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Subscription</Label>
                                <Text>{project.resourceGroup.subscriptionId}</Text>
                            </Stack>
                        </Stack>
                    </Stack>
                    <Stack verticalFill styles={{ root: { width: '100%' } }}>
                        <Text styles={headingStyles}>Links</Text>
                        <Separator></Separator>
                        <Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>GitHub Repository:</Label>
                                <Link href='https://github.com'>https://github.com</Link>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Azure DevTestLabs:</Label>
                                <Link href='https://github.com'>https://github.com</Link>
                            </Stack>
                        </Stack>
                        <Text styles={headingStyles}>Users</Text>
                        <Separator></Separator>
                        <Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>ID:</Label>
                                <Text>{project.users[0].id}</Text>
                            </Stack>
                            <Stack horizontal verticalAlign='baseline' tokens={dataStackTokens}>
                                <Label>Role:</Label>
                                <Text>{project.users[0].projectMemberships![0].role}</Text>
                            </Stack>
                        </Stack>
                    </Stack>
                    {/* <Text>{project.users[0].id}</Text>
                    <Text>{project.users[0].role}</Text>
                    <Text>{project.users[0].userType}</Text> */}
                    {/* <Text>{this.state.project.users[0].projectMemberships[0].projectId}</Text>
                    <Text>{this.state.project.users[0].projectMemberships[0].role}</Text> */}
                </Stack>
            </Stack>
        );
    }
    return (<Stack verticalFill verticalAlign='center' horizontalAlign='center'><Spinner /></Stack>);
}
