// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useHistory } from 'react-router-dom';
import { Stack, PrimaryButton, Pivot, PivotItem } from '@fluentui/react';
import { Organization, Project } from 'teamcloud'
import { ContentContainer, ContentHeader, ContentProgress, ProjectList } from '../components';

export interface IProjectsViewProps {
    org?: Organization;
    projects?: Project[];
    onProjectSelected?: (project: Project) => void;
}

export const ProjectsView: React.FC<IProjectsViewProps> = (props: IProjectsViewProps) => {

    const history = useHistory();

    return (
        <Stack>
            <ContentProgress
                progressHidden={props.org !== undefined && props.projects !== undefined} />
            <ContentHeader title={props.org?.displayName}>
                <PrimaryButton
                    text='New project'
                    iconProps={{ iconName: 'Add' }}
                    disabled={props.org === undefined}
                    onClick={() => history.push(`/orgs/${props.org?.slug}/projects/new`)} />
            </ContentHeader>
            <ContentContainer>
                <Pivot>
                    <PivotItem headerText='Projects'>
                        <Stack styles={{ root: { paddingTop: '24px', paddingRight: '12px' } }}>
                            <ProjectList
                                projects={props.projects}
                                onProjectSelected={props.onProjectSelected} />
                        </Stack>
                    </PivotItem>
                </Pivot>
            </ContentContainer>
        </Stack>
    );
}
