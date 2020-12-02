// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { Route, useParams } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Component, UserDefinition } from 'teamcloud';
import { ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';
import { getGraphUser } from '../MSGraph';
import { OrgContext, ProjectContext } from '../Context';
import { useProject } from '../Hooks';

export const ProjectView: React.FC = () => {



    const [favorite, setFavorate] = useState(false);
    // const { project, user } = useContext(OrgContext);

    // const isAuthenticated = useIsAuthenticated();
    // const { navId } = useParams() as { navId: string };
    // const [members, setMembers] = useState<ProjectMember[]>();
    // const [components, setComponents] = useState<Component[]>();


    // useEffect(() => {
    //     if (isAuthenticated && project && (navId === undefined || navId.toLowerCase() === 'members')) {
    //         if (members && members.length > 0 && members[0].projectMembership.projectId === project.id)
    //             return;
    //         const _setMembers = async () => {
    //             let _users = await api.getProjectUsers(project!.organization, project!.id);
    //             if (_users.data) {
    //                 let _members = await Promise.all(_users.data.map(async u => ({
    //                     user: u,
    //                     graphUser: await getGraphUser(u.id),
    //                     projectMembership: u.projectMemberships!.find(m => m.projectId === project!.id)!
    //                 })));
    //                 setMembers(_members);
    //             }
    //             console.log(`setProjectMembers (${project.slug})`);
    //         };
    //         _setMembers();
    //     }
    // }, [isAuthenticated, project, members, navId]);


    // useEffect(() => {
    //     if (isAuthenticated && project && (navId === undefined || navId.toLowerCase() === 'components')) {
    //         if (components === undefined || (components.length > 0 && components[0].projectId !== project.id)) {
    //             const _setComponents = async () => {
    //                 const result = await api.getProjectComponents(project!.organization, project!.id);
    //                 setComponents(result.data ?? undefined);
    //                 console.log(`setProjectComponents (${project.slug})`);
    //             };
    //             _setComponents();
    //         }
    //     }
    // }, [isAuthenticated, project, components, navId]);


    // const onAddUsers = async (users: UserDefinition[]) => {
    //     if (project) {
    //         console.log(`addMembers (${project.slug})`);
    //         const results = await Promise
    //             .all(users.map(async d => await api.createProjectUser(project.organization, project.id, { body: d })));

    //         results.forEach(r => {
    //             if (!r.data)
    //                 console.error(r);
    //         });

    //         const newMembers = await Promise.all(results
    //             .filter(r => r.data)
    //             .map(r => r.data!)
    //             .map(async u => ({
    //                 user: u,
    //                 graphUser: await getGraphUser(u.id),
    //                 projectMembership: u.projectMemberships!.find(m => m.projectId === project.id)!
    //             })));

    //         setMembers(members ? [...members, ...newMembers] : newMembers)
    //     }
    // }

    // const { members, components, onAddUsers } = useProject(isAuthenticated, project, navId);
    const { navId, user, project, members, components, onAddUsers } = useProject();

    const progressHidden = () => {
        if (navId === undefined) {
            return project !== undefined && components !== undefined && members !== undefined
        } else if (navId.toLowerCase() === 'members') {
            return project !== undefined && members !== undefined
        } else if (navId.toLowerCase() === 'components') {
            return project !== undefined && components !== undefined
        }
        return true;
    }

    return (
        <ProjectContext.Provider value={{
            user: user,
            project: project,
            members: members,
            components: components,
            onAddUsers: onAddUsers
        }}>
            <Stack>
                <ContentProgress progressHidden={progressHidden()} />
                <ContentHeader title={navId ? navId : project?.displayName} coin={!navId}>
                    {!navId && (
                        <IconButton
                            toggle
                            checked={favorite}
                            onClick={() => setFavorate(!favorite)}
                            iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
                    )}
                </ContentHeader>
                <ContentContainer>
                    <Route exact path='/orgs/:orgId/projects/:projectId'>
                        <ProjectOverview />
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/components'>
                        <ComponentList />
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                        <MemberList {...{ project: project, members: members, onAddUsers: onAddUsers }} />
                    </Route>
                </ContentContainer>
            </Stack>
        </ProjectContext.Provider>
    );
}
