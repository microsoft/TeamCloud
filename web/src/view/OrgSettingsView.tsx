// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Route, useParams } from 'react-router-dom';
import { Stack, Persona, PersonaSize, getTheme, IconButton, ProgressIndicator } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Organization, Project, User } from 'teamcloud';
import { ProjectMembersForm, ProjectMemberForm, ProjectMembers, ProjectComponents, ProjectOverview, OrgSettingsOverview, OrgSettingsMembers, OrgSettingsConfiguration, OrgSettingsDeploymentScopes, OrgSettingsProjectTemplates, ContentHeader, ContentContainer, ContentProgress } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';

export interface IOrgSettingsViewProps {
    org?: Organization;
    user?: User;
}

export const OrgSettingsView: React.FunctionComponent<IOrgSettingsViewProps> = (props) => {

    let isAuthenticated = useIsAuthenticated();
    let { orgId, settingId } = useParams() as { orgId: string, settingId: string };

    const [org, setOrg] = useState(props.org);
    const [user, setUser] = useState(props.user);

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

    const theme = getTheme();

    return org?.id ? (
        <Stack>
            <ContentHeader title={org.displayName} />
            <ContentContainer>
                <Route exact path='/orgs/:orgId/settings'>
                    <OrgSettingsOverview {...{ org: org, user: user }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/members'>
                    <OrgSettingsMembers {...{ org: org, user: user }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/configuration'>
                    <OrgSettingsConfiguration {...{ org: org, user: user }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/scopes'>
                    <OrgSettingsDeploymentScopes {...{ org: org, user: user }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/templates'>
                    <OrgSettingsProjectTemplates {...{ org: org, user: user }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/providers'>
                    <OrgSettingsOverview {...{ org: org, user: user }} />
                </Route>
            </ContentContainer>
        </Stack>
    ) : (<ContentProgress progressHidden={org !== undefined} />);
}
