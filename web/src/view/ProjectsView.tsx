// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Pivot, PivotItem, PrimaryButton, Stack } from '@fluentui/react';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ContentContainer, ContentHeader, ContentProgress, ProjectList } from '../components';
import { useOrg, useProjects } from '../hooks';

export const ProjectsView: React.FC = () => {

    const navigate = useNavigate();

    const { data: org, isLoading: orgIsLoading } = useOrg();
    const { isLoading: projectsIsLoading } = useProjects();

    return (
        <Stack>
            <ContentProgress
                progressHidden={!orgIsLoading && !projectsIsLoading} />
            <ContentHeader title={org?.displayName}>
                <PrimaryButton
                    text='New project'
                    iconProps={{ iconName: 'Add' }}
                    disabled={!org}
                    onClick={() => navigate(`/orgs/${org?.slug}/projects/new`)} />
            </ContentHeader>
            <ContentContainer>
                <Pivot>
                    <PivotItem headerText='Projects'>
                        <Stack styles={{ root: { paddingTop: '24px', paddingRight: '12px' } }}>
                            <ProjectList />
                        </Stack>
                    </PivotItem>
                </Pivot>
            </ContentContainer>
        </Stack>
    );
};
