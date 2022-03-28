// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useNavigate } from 'react-router-dom';
import { Stack, PrimaryButton, Pivot, PivotItem } from '@fluentui/react';
import { ContentContainer, ContentHeader, ContentProgress, NoData, ProjectList } from '../components';
import { useOrg, useProjects } from '../hooks';
import { KnownOrganizationPortal } from 'teamcloud';

import collaboration from '../img/MSC17_collaboration_010_noBG.png'

export const ProjectsView: React.FC = () => {

    const navigate = useNavigate();

    const { data: org, isLoading: orgIsLoading } = useOrg();
    const { isLoading: projectsIsLoading } = useProjects();

    const _openPortal = () => {
        const newWindow = window.open(org?.portalUrl, '_blank', 'noopener,noreferrer')
        if (newWindow) newWindow.opener = null
    }

    if (org?.portal !== KnownOrganizationPortal.TeamCloud) {

        return (
            <Stack>
                <ContentHeader title={org?.displayName}>
                </ContentHeader>
                <ContentContainer>
                    <NoData
                        title={`You chose to go with ${org?.portal} for this organization`}
                        description={ org?.portalUrl ? '' : 'provisioning ...' }
                        image={collaboration}
                        buttonText={`Open ${org?.portal}`}
                        buttonDisabled={!(org?.portalUrl)}
                        onButtonClick={_openPortal} 
                        />
                </ContentContainer>
            </Stack>
        );

    } else {

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

    }
}
