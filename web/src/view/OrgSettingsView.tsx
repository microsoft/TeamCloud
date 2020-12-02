// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { Route, useHistory, useLocation, useParams } from 'react-router-dom';
import { IconButton, Stack } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { DeploymentScope, DeploymentScopeDefinition, ProjectTemplate, ProjectTemplateDefinition, UserDefinition } from 'teamcloud';
import { OrgSettingsOverview, DeploymentScopeList, ProjectTemplateList, ContentHeader, ContentContainer, ContentProgress, MemberList, DeploymentScopeForm, ProjectTemplateForm } from '../components';
import { getGraphUser } from '../MSGraph';
import { ManagementGroup, Member, Subscription } from '../model';
import { api } from '../API';
import { getManagementGroups, getSubscriptions } from '../Azure';
import { OrgContext } from '../Context';

export const OrgSettingsView: React.FC = () => {

    const location = useLocation();
    const history = useHistory();

    const isAuthenticated = useIsAuthenticated();

    const { orgId, settingId } = useParams() as { orgId: string, settingId: string };

    const [members, setMembers] = useState<Member[]>();
    const [scopes, setScopes] = useState<DeploymentScope[]>();
    const [templates, setTemplates] = useState<ProjectTemplate[]>();
    const [progressHidden, setProgressHidden] = useState(true);
    const [subscriptions, setSubscriptions] = useState<Subscription[]>();
    const [managementGroups, setManagementGroups] = useState<ManagementGroup[]>();

    const { org } = useContext(OrgContext);

    useEffect(() => {
        if (isAuthenticated && org && (settingId === undefined || settingId.toLowerCase() === 'members') && members === undefined) {
            const _setMembers = async () => {
                console.log(`setMembers (${org.slug})`);
                let _users = await api.getOrganizationUsers(org.id);
                if (_users.data) {
                    let _members = await Promise.all(_users.data.map(async u => ({
                        user: u,
                        graphUser: await getGraphUser(u.id)
                    })));
                    setMembers(_members);
                }
            };
            _setMembers();
        }
    }, [isAuthenticated, org, settingId, members]);


    useEffect(() => {
        if (isAuthenticated && org && settingId?.toLowerCase() === 'scopes' && !location.pathname.toLowerCase().endsWith('/new') && scopes === undefined) {
            const _setScopes = async () => {
                console.log(`setDeploymentScopes (${org.slug})`);
                let _scopes = await api.getDeploymentScopes(org.id);
                setScopes(_scopes.data ?? undefined)
            };
            _setScopes();
        }
    }, [isAuthenticated, org, scopes, settingId, location]);


    useEffect(() => {
        if (isAuthenticated && org && settingId?.toLowerCase() === 'templates' && !location.pathname.toLowerCase().endsWith('/new') && templates === undefined) {
            const _setTemplates = async () => {
                console.log(`setProjectTemplates (${org.slug})`);
                let _templates = await api.getProjectTemplates(org.id);
                setTemplates(_templates.data ?? undefined)
            };
            _setTemplates();
        }
    }, [isAuthenticated, org, templates, settingId, location]);


    useEffect(() => {
        if (isAuthenticated && org && settingId?.toLowerCase() === 'scopes' && location.pathname.toLowerCase().endsWith('/new') && subscriptions === undefined) {
            const _setSubscriptions = async () => {
                console.log(`setSubscriptions (${org.slug})`);
                try {
                    const subs = await getSubscriptions();
                    setSubscriptions(subs ?? []);
                } catch (error) {
                    setSubscriptions([]);
                }
            };
            _setSubscriptions();
        }
    }, [isAuthenticated, org, subscriptions, settingId, location]);


    useEffect(() => {
        if (isAuthenticated && org && settingId?.toLowerCase() === 'scopes' && location.pathname.toLowerCase().endsWith('/new') && managementGroups === undefined) {
            const _setManagementGroups = async () => {
                console.log(`setManagementGroups (${org.slug})`);
                try {
                    const groups = await getManagementGroups();
                    setManagementGroups(groups ?? []);
                } catch (error) {
                    setManagementGroups([]);
                }
            };
            _setManagementGroups();
        }
    }, [isAuthenticated, org, managementGroups, settingId, location]);


    const onCreateDeploymentScope = async (scope: DeploymentScopeDefinition) => {
        if (org) {
            setProgressHidden(false);
            const result = await api.createDeploymentScope(org.id, { body: scope, });
            if (result.data) {
                setScopes(scopes ? [...scopes, result.data] : [result.data]);
                history.push(`/orgs/${org.slug}/settings/scopes`);
            } else {
                console.error(`Failed to create new DeploymentScope: ${result}`);
            }
            setProgressHidden(true);
        }
    };


    const onCreateProjectTemplate = async (template: ProjectTemplateDefinition) => {
        if (org) {
            setProgressHidden(false);
            const result = await api.createProjectTemplate(org.id, { body: template, });
            if (result.data) {
                setTemplates(templates ? [...templates, result.data] : [result.data]);
                history.push(`/orgs/${org.slug}/settings/templates`);
            } else {
                console.error(`Failed to create new ProjectTemplate: ${result}`);
            }
            setProgressHidden(true);
        }
    };


    const onAddUsers = async (users: UserDefinition[]) => {
        if (org) {
            setProgressHidden(false);
            console.log(`addMembers (${org.slug})`);
            const results = await Promise
                .all(users.map(async d => await api.createOrganizationUser(org.id, { body: d })));

            results.forEach(r => {
                if (!r.data)
                    console.error(r);
            });

            const newMembers = await Promise.all(results
                .filter(r => r.data)
                .map(r => r.data!)
                .map(async u => ({
                    user: u,
                    graphUser: await getGraphUser(u.id)
                })));

            setMembers(members ? [...members, ...newMembers] : newMembers)

            setProgressHidden(true);
        }
    };

    return (
        <Stack>
            <Route exact path='/orgs/:orgId/settings'>
                <ContentProgress progressHidden={progressHidden && org !== undefined} />
                <ContentHeader title={org?.displayName} />
            </Route>
            <Route exact path='/orgs/:orgId/settings/members'>
                <ContentProgress progressHidden={progressHidden && members !== undefined} />
                <ContentHeader title='Members' />
            </Route>
            <Route exact path={['/orgs/:orgId/settings/scopes']}>
                <ContentProgress progressHidden={progressHidden && scopes !== undefined} />
                <ContentHeader title='Deployment Scopes' />
            </Route>
            <Route exact path={['/orgs/:orgId/settings/scopes/new']}>
                <ContentProgress progressHidden={progressHidden && subscriptions !== undefined && managementGroups !== undefined} />
                <ContentHeader title='New Deployment Scope'>
                    <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.push(`/orgs/${orgId}/settings/scopes`)} />
                </ContentHeader>
            </Route>
            <Route exact path='/orgs/:orgId/settings/templates'>
                <ContentProgress progressHidden={progressHidden && templates !== undefined} />
                <ContentHeader title='Project Templates' />
            </Route>
            <Route exact path={['/orgs/:orgId/settings/templates/new']}>
                <ContentProgress progressHidden={progressHidden && templates !== undefined} />
                <ContentHeader title='New Project Template'>
                    <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.push(`/orgs/${orgId}/settings/templates`)} />
                </ContentHeader>
            </Route>

            <ContentContainer>
                <Route exact path='/orgs/:orgId/settings'>
                    <OrgSettingsOverview {...{ org: org, members: members }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/members'>
                    <MemberList {...{ members: members, onAddUsers: onAddUsers }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/scopes'>
                    <DeploymentScopeList {...{ org: org, scopes: scopes }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/scopes/new'>
                    <DeploymentScopeForm {...{ subscriptions: subscriptions, managementGroups: managementGroups, onCreateDeploymentScope: onCreateDeploymentScope }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/templates'>
                    <ProjectTemplateList {...{ templates: templates }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/templates/new'>
                    <ProjectTemplateForm {...{ onCreateProjectTemplate: onCreateProjectTemplate }} />
                </Route>
            </ContentContainer>
        </Stack>
    );
}
