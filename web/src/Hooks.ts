// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useIsAuthenticated } from '@azure/msal-react';
import React, { useState, useEffect, useContext } from 'react';
import { useParams } from 'react-router-dom';
import { Component, Project, UserDefinition } from 'teamcloud';
import { api } from './API';
import { OrgContext } from './Context';
import { ProjectMember } from './model';
import { getGraphUser } from './MSGraph';

export const useProject = () => {

    const isAuthenticated = useIsAuthenticated();

    const { navId } = useParams() as { navId: string };

    const [members, setMembers] = useState<ProjectMember[]>();
    const [components, setComponents] = useState<Component[]>();
    // const [favorite, setFavorate] = useState(false);

    const { project, user } = useContext(OrgContext);

    useEffect(() => {
        if (isAuthenticated && project && (navId === undefined || navId.toLowerCase() === 'members')) {
            if (members && members.length > 0 && members[0].projectMembership.projectId === project.id)
                return;
            const _setMembers = async () => {
                console.log(`setProjectMembers (${project.slug})`);
                let _users = await api.getProjectUsers(project!.organization, project!.id);
                if (_users.data) {
                    let _members = await Promise.all(_users.data.map(async u => ({
                        user: u,
                        graphUser: await getGraphUser(u.id),
                        projectMembership: u.projectMemberships!.find(m => m.projectId === project!.id)!
                    })));
                    setMembers(_members);
                }
            };
            _setMembers();
        }
    }, [isAuthenticated, project, members, navId]);


    useEffect(() => {
        if (isAuthenticated && project && (navId === undefined || navId.toLowerCase() === 'components')) {
            if (components === undefined || (components.length > 0 && components[0].projectId !== project.id)) {
                const _setComponents = async () => {
                    console.log(`setProjectComponents (${project.slug})`);
                    const result = await api.getProjectComponents(project!.organization, project!.id);
                    setComponents(result.data ?? undefined);
                };
                _setComponents();
            }
        }
    }, [isAuthenticated, project, components, navId]);


    const onAddUsers = async (users: UserDefinition[]) => {
        if (project) {
            console.log(`addMembers (${project.slug})`);
            const results = await Promise
                .all(users.map(async d => await api.createProjectUser(project.organization, project.id, { body: d })));

            results.forEach(r => {
                if (!r.data)
                    console.error(r);
            });

            const newMembers = await Promise.all(results
                .filter(r => r.data)
                .map(r => r.data!)
                .map(async u => ({
                    user: u,
                    graphUser: await getGraphUser(u.id),
                    projectMembership: u.projectMemberships!.find(m => m.projectId === project.id)!
                })));

            setMembers(members ? [...members, ...newMembers] : newMembers)
        }
    }

    return { navId: navId, user: user, project: project, members: members, components: components, onAddUsers: onAddUsers }
};
