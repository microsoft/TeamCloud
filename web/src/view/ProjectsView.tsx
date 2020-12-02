// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext } from 'react';
import { useHistory } from 'react-router-dom';
import { Stack, PrimaryButton, Pivot, PivotItem } from '@fluentui/react';
import { ContentContainer, ContentHeader, ContentProgress, ProjectList } from '../components';
import { OrgContext } from '../Context';

export const ProjectsView: React.FC = () => {

    const history = useHistory();

    const { org, projects } = useContext(OrgContext);

    return (
        <Stack>
            <ContentProgress
                progressHidden={org !== undefined && projects !== undefined} />
            <ContentHeader title={org?.displayName}>
                <PrimaryButton
                    text='New project'
                    iconProps={{ iconName: 'Add' }}
                    disabled={org === undefined}
                    onClick={() => history.push(`/orgs/${org?.slug}/projects/new`)} />
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
}
