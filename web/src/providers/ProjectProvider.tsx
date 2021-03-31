// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useState } from 'react';
import { useLocation, useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { ProjectMember } from '../model';
import { ProjectContext } from '../Context';
import { getGraphUser } from '../MSGraph';
import { Component, ComponentTask, ComponentTemplate, UserDefinition } from 'teamcloud';
import { endsWithLowerCase, matchesAnyLowerCase, matchesLowerCase, matchesRouteParam } from '../Utils';
import { api, resolveSignalR } from '../API';
import { useOrg } from '../Hooks';

export const ProjectProvider = (props: any) => {

    const { projectId, navId, itemId } = useParams() as { projectId: string, navId: string, itemId: string };

    const isAuthenticated = useIsAuthenticated();

    const location = useLocation();

    const [projectMembers, setProjectMembers] = useState<ProjectMember[]>();
    const [projectComponent, setProjectComponent] = useState<Component>();
    const [projectComponents, setProjectComponents] = useState<Component[]>();
    const [projectComponentTemplates, setProjectComponentTemplates] = useState<ComponentTemplate[]>();
    const [projectComponentTask, setProjectComponentTask] = useState<ComponentTask>();
    const [projectComponentTasks, setProjectComponentTasks] = useState<ComponentTask[]>();

    const { project, user } = useOrg()


    useEffect(() => {
        const _resolve = async () => {
            await resolveSignalR(project)
        }
        _resolve();
    }, [project])


    const onComponentSelected = useCallback((selectedComponent?: Component) => {
        if (selectedComponent && projectComponent && selectedComponent.id === projectComponent.id && selectedComponent.resourceState === projectComponent.resourceState)
            return;
        console.log(`+ setComponent (${selectedComponent?.slug})`);
        setProjectComponent(selectedComponent);
    }, [projectComponent]);


    const onComponentTaskSelected = useCallback((selectedComponentTask?: ComponentTask) => {
        if (selectedComponentTask && projectComponentTask
            && selectedComponentTask.id === projectComponentTask.id
            && selectedComponentTask.resourceState === projectComponentTask.resourceState
            && selectedComponentTask.output === projectComponentTask.output)
            return;
        console.log(`+ setComponentTask (${selectedComponentTask?.id})`);
        setProjectComponentTask(selectedComponentTask);
    }, [projectComponentTask]);


    const onAddProjectUsers = async (users: UserDefinition[]) => {
        if (project) {
            console.log(`- addProjectMembers (${project.slug})`);
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

            setProjectMembers(projectMembers ? [...projectMembers, ...newMembers] : newMembers);
            console.log(`+ addProjectMembers (${project.slug})`);
        }
    };


    useEffect(() => { // Esure selected Project Component matches route
        if (projectId && navId && matchesLowerCase(navId, 'components') && itemId) {
            if (projectComponent && matchesRouteParam(projectComponent, itemId)) {
                return;
            } else if (projectComponents) {
                const find = projectComponents.find(c => matchesRouteParam(c, itemId));
                if (find) {
                    console.log(`+ getComponentFromRoute (${itemId})`);
                    onComponentSelected(find);
                }
            }
        } else if (projectComponent) {
            console.log(`+ getComponentFromRoute (undefined)`);
            onComponentSelected(undefined);
        }
    }, [itemId, navId, projectId, projectComponents, projectComponent, location, onComponentSelected]);


    useEffect(() => { // Project Members
        if (isAuthenticated && projectId && project) {
            // on project Overview, Members, or Components page
            if (navId === undefined || matchesAnyLowerCase(navId, 'members', 'components')) {
                // no project members OR project members are from wrong project
                if (projectMembers === undefined || projectMembers.some(m => m.projectMembership.projectId !== project.id)) {
                    const _setProjectMembers = async () => {
                        console.log(`- setProjectMembers (${project.slug})`);
                        let _users = await api.getProjectUsers(project!.organization, project!.id);
                        if (_users.data) {
                            let _members = await Promise.all(_users.data.map(async u => ({
                                user: u,
                                graphUser: await getGraphUser(u.id),
                                projectMembership: u.projectMemberships!.find(m => m.projectId === project!.id)!
                            })));
                            setProjectMembers(_members);
                        }
                        console.log(`+ setProjectMembers (${project.slug})`);
                    };
                    _setProjectMembers();
                }
            }
        } else if (projectMembers) {
            console.log('+ setProjectMembers (undefined)');
            setProjectMembers(undefined);
        }
    }, [isAuthenticated, projectId, project, projectMembers, navId]);


    useEffect(() => { // Project Components
        if (isAuthenticated && projectId && project) {
            // on project Overview page
            if ((navId === undefined && !endsWithLowerCase(location.pathname, '/settings'))
                // OR on project Components page
                || (navId && matchesLowerCase(navId, 'components'))) {
                // AND no components OR components are from wrong project
                if (projectComponents === undefined || projectComponents.some(c => c.projectId !== project.id)
                    // OR selected component  is not in components (new component created)
                    || (projectComponent && !projectComponents.some(c => c.id === projectComponent.id))) {
                    const _setProjectComponents = async () => {
                        console.log(`- setProjectComponents (${project.slug})`);
                        const result = await api.getComponents(project!.organization, project!.id);
                        setProjectComponents(result.data ?? undefined);
                        console.log(`+ setProjectComponents (${project.slug})`);
                    };
                    _setProjectComponents();
                }
            }
        } else if (projectComponents) {
            console.log('+ setProjectComponents (undefined)');
            setProjectComponents(undefined);
        }
    }, [isAuthenticated, projectId, project, projectComponents, projectComponent, navId, location]);


    useEffect(() => {// Project Component Templates
        if (isAuthenticated && projectId && project) {
            // on project Overview page
            if ((navId === undefined && !endsWithLowerCase(location.pathname, '/settings'))
                // OR on project Components page
                || (navId && matchesLowerCase(navId, 'components'))) {
                // AND no component templates
                if (projectComponentTemplates === undefined) {
                    const _setComponentTemplates = async () => {
                        console.log(`- setProjectComponentTemplates (${project.slug})`);
                        const result = await api.getComponentTemplates(project!.organization, project!.id);
                        setProjectComponentTemplates(result.data ?? undefined);
                        console.log(`+ setProjectComponentTemplates (${project.slug})`);
                    };
                    _setComponentTemplates();
                }
            }
        } else if (projectComponentTemplates) {
            console.log('+ setProjectComponentTemplates (undefined)');
            setProjectComponentTemplates(undefined);
        }
    }, [isAuthenticated, projectId, project, projectComponentTemplates, navId, location]);


    useEffect(() => {// Project Component Tasks
        if (isAuthenticated && projectId && project && projectComponent) {
            // on project component page AND the route matches the selected component
            if (navId && matchesLowerCase(navId, 'components') && itemId && matchesRouteParam(projectComponent, itemId)) {
                // AND no component tasks OR component tasks are from wrong component
                if (projectComponentTasks === undefined || projectComponentTasks.some(d => d.componentId !== projectComponent.id)
                    // OR selected component task isn't in the component task list (new component task was created)
                    || (projectComponentTask && !projectComponentTasks.some(t => t.id === projectComponentTask.id))) {
                    const _setComponentTasks = async () => {
                        console.log(`- setProjectComponentTasks (${projectComponent.slug})`);
                        const result = await api.getComponentTasks(project.organization, project.id, projectComponent.id);
                        setProjectComponentTasks(result.data ?? undefined);
                        console.log(`+ setProjectComponentTasks (${projectComponent.slug})`);
                        if (result.data) {
                            onComponentTaskSelected(result.data[result.data.length - 1]);
                        }
                    };
                    _setComponentTasks();
                }
            }
        } else if (projectComponentTasks) {
            console.log('+ setProjectComponentTasks (undefined)');
            setProjectComponentTasks(undefined);
        }
    }, [isAuthenticated, projectId, project, projectComponent, navId, itemId, projectComponentTasks, projectComponentTask, onComponentTaskSelected]);


    useEffect(() => {// Project Component Task
        if (isAuthenticated && projectId && project && projectComponent && projectComponentTasks) {
            // on project component page AND the route matches the selected component
            if (navId && matchesLowerCase(navId, 'components') && itemId && matchesRouteParam(projectComponent, itemId)) {

                if (projectComponentTask && projectComponentTasks.some(t => t.id === projectComponentTask.id)) {
                    // Only expand if the component task is finished, if its in progress the list will poll to expand
                    if (projectComponentTask.resourceState
                        && (projectComponentTask.resourceState.toLowerCase() === 'succeeded' || projectComponentTask.resourceState.toLowerCase() === 'failed')
                        && !projectComponentTask.output) {
                        const _expandComponetTask = async () => {
                            console.log(`- expandProjectComponentTask (${projectComponentTask.id})`);
                            const result = await api.getComponentTask(projectComponentTask.id, project.organization, project.id, projectComponentTask.componentId);
                            setProjectComponentTask(result.data ?? undefined);
                            console.log(`+ expandProjectComponentTask (${projectComponentTask.id})`);
                        };
                        _expandComponetTask()
                    } else {
                        const task = projectComponentTasks.find(t => t.id === projectComponentTask.id);
                        if (task && (task.resourceState !== projectComponentTask.resourceState || task?.output !== projectComponentTask.output)) {
                            const _setComponentTask = async () => {
                                const index = projectComponentTasks.indexOf(task);
                                const newArr = [...projectComponentTasks];
                                newArr[index] = projectComponentTask;
                                console.log(`+ updateProjectComponentTasks (${projectComponent.slug})`);
                                setProjectComponentTasks(newArr);
                            };
                            _setComponentTask();
                        }
                    }
                }
            }
        } else if (projectComponentTask) {
            console.log('+ setProjectComponentTask (undefined)');
            setProjectComponentTask(undefined);
        }
    }, [isAuthenticated, projectId, project, projectComponent, navId, itemId, projectComponentTasks, projectComponentTask]);


    return <ProjectContext.Provider value={{
        user: user,
        project: project,
        members: projectMembers,
        component: projectComponent,
        components: projectComponents,
        templates: projectComponentTemplates,
        componentTask: projectComponentTask,
        componentTasks: projectComponentTasks,
        onAddUsers: onAddProjectUsers,
        onRemoveUsers: () => Promise.resolve(),
        onComponentSelected: onComponentSelected,
        onComponentTaskSelected: onComponentTaskSelected
    }} {...props} />
}
