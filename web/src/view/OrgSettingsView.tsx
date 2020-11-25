// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Route, useParams } from 'react-router-dom';
import { Stack } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { DeploymentScope, Organization, ProjectTemplate, User } from 'teamcloud';
import { OrgSettingsOverview, DeploymentScopeList, ProjectTemplateList, ContentHeader, ContentContainer, ContentProgress, MemberList } from '../components';
import { getGraphUser } from '../MSGraph';
import { Member } from '../model';
import { api } from '../API';

export interface IOrgSettingsViewProps {
    org?: Organization;
    user?: User;
}

export const OrgSettingsView: React.FC<IOrgSettingsViewProps> = (props) => {

    let isAuthenticated = useIsAuthenticated();

    let { orgId, settingId } = useParams() as { orgId: string, settingId: string };

    const [org, setOrg] = useState(props.org);
    const [members, setMembers] = useState<Member[]>();
    const [scopes, setScopes] = useState<DeploymentScope[]>();
    const [templates, setTemplates] = useState<ProjectTemplate[]>();

    useEffect(() => {
        if (isAuthenticated && orgId) {

            if (org && (org.id === orgId || org.slug === orgId))
                return;

            setOrg(undefined);

            const _setOrg = async () => {
                const result = await api.getOrganization(orgId);
                setOrg(result.data);
            };

            _setOrg();
        }
    }, [isAuthenticated, org, orgId]);


    useEffect(() => {
        if (isAuthenticated && orgId && settingId === 'members' && members === undefined) {
            console.log('setMembers');
            const _setMembers = async () => {
                let _users = await api.getOrganizationUsers(orgId);
                if (_users.data) {
                    let _members = await Promise.all(_users.data.map(async u => ({
                        user: u,
                        graphUser: await getGraphUser(u.id)
                    })));
                    _members.forEach(m => console.log(m));
                    setMembers(_members);
                }
            };
            _setMembers();
        }
    }, [isAuthenticated, orgId, settingId, members]);


    useEffect(() => {
        if (isAuthenticated && orgId && settingId === 'scopes' && scopes === undefined) {
            console.log('setScopes');
            const _setScopes = async () => {
                let _scopes = await api.getDeploymentScopes(orgId);
                setScopes(_scopes.data ?? undefined)
            };
            _setScopes();
        }
    }, [isAuthenticated, orgId, settingId, scopes]);


    useEffect(() => {
        if (isAuthenticated && orgId && settingId === 'templates' && templates === undefined) {
            console.log('setTemplates');
            const _setTemplates = async () => {
                let _templates = await api.getProjectTemplates(orgId);
                setTemplates(_templates.data ?? undefined)
            };
            _setTemplates();
        }
    }, [isAuthenticated, orgId, settingId, templates]);


    return (
        <Stack>
            <Route exact path='/orgs/:orgId/settings'>
                <ContentProgress progressHidden={org !== undefined} />
                <ContentHeader title={org?.displayName} />
            </Route>
            <Route exact path='/orgs/:orgId/settings/members'>
                <ContentProgress progressHidden={members !== undefined} />
                <ContentHeader title='Members' />
            </Route>
            <Route exact path='/orgs/:orgId/settings/scopes'>
                <ContentProgress progressHidden={scopes !== undefined} />
                <ContentHeader title='Deployment Scopes' />
            </Route>
            <Route exact path='/orgs/:orgId/settings/templates'>
                <ContentProgress progressHidden={templates !== undefined} />
                <ContentHeader title='Project Templates' />
            </Route>

            <ContentContainer>
                <Route exact path='/orgs/:orgId/settings'>
                    <OrgSettingsOverview {...{ org: org }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/members'>
                    <MemberList {...{ members: members }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/scopes'>
                    <DeploymentScopeList {...{ scopes: scopes }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/templates'>
                    <ProjectTemplateList {...{ templates: templates }} />
                </Route>
            </ContentContainer>
        </Stack>
    );
}
