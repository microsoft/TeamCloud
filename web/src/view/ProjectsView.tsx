// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Stack, PrimaryButton, Pivot, PivotItem } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Organization, Project } from 'teamcloud'
import { ContentContainer, ContentHeader, ContentProgress, ProjectList } from '../components';
import { api } from '../API';

export interface IProjectsViewProps {
    onProjectSelected?: (project: Project) => void;
}

export const ProjectsView: React.FC<IProjectsViewProps> = (props: IProjectsViewProps) => {

    let history = useHistory();
    let { orgId } = useParams() as { orgId: string };

    let isAuthenticated = useIsAuthenticated();

    const [org, setOrg] = useState<Organization>();
    const [projects, setProjects] = useState<Project[]>();

    useEffect(() => {
        if (isAuthenticated || orgId) {

            if (org && (org.id === orgId || org.slug === orgId))
                return;

            setOrg(undefined);
            setProjects(undefined);

            const _setOrg = async () => {

                const promises: any[] = [
                    api.getOrganization(orgId),
                    api.getProjects(orgId)
                ];

                var results = await Promise.all(promises);

                setOrg(results[0]?.data ?? undefined);
                setProjects(results[1].data ?? []);
            };

            _setOrg();
        }
    }, [isAuthenticated, orgId, org, projects]);

    return (
        <Stack>
            <ContentProgress
                progressHidden={org !== undefined && projects !== undefined} />
            <ContentHeader title={org?.displayName}>
                <PrimaryButton
                    disabled={org === undefined}
                    iconProps={{ iconName: 'Add' }} text='New project' onClick={() => history.push(`/orgs/${orgId}/projects/new`)} />
            </ContentHeader>
            <ContentContainer>
                <Pivot>
                    <PivotItem headerText='Projects'>
                        <Stack styles={{ root: { paddingTop: '24px', paddingRight: '12px' } }}>
                            <ProjectList
                                projects={projects}
                                onProjectSelected={props.onProjectSelected} />
                        </Stack>
                    </PivotItem>
                </Pivot>
            </ContentContainer>
        </Stack>
    );
}
