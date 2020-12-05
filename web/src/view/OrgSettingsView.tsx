// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useContext } from 'react';
import { Route, useHistory } from 'react-router-dom';
import { IconButton, Stack } from '@fluentui/react';
import { DeploymentScopeDefinition, ProjectTemplateDefinition } from 'teamcloud';
import { OrgSettingsOverview, DeploymentScopeList, ProjectTemplateList, ContentHeader, ContentContainer, ContentProgress, MemberList, DeploymentScopeForm, ProjectTemplateForm } from '../components';
import { GraphUserContext, OrgContext } from '../Context';

export const OrgSettingsView: React.FC = () => {

    const history = useHistory();

    const [progressHidden, setProgressHidden] = useState(true);

    const { subscriptions, managementGroups } = useContext(GraphUserContext);
    const { org, members, scopes, templates, onCreateDeploymentScope, onCreateProjectTemplate, onAddUsers } = useContext(OrgContext);


    const _onCreateDeploymentScope = async (scope: DeploymentScopeDefinition) => {
        setProgressHidden(false);
        await onCreateDeploymentScope(scope);
        setProgressHidden(true);
        history.push(`/orgs/${org?.slug}/settings/scopes`);
    };


    const _onCreateProjectTemplate = async (template: ProjectTemplateDefinition) => {
        setProgressHidden(false);
        await onCreateProjectTemplate(template);
        setProgressHidden(true);
        history.push(`/orgs/${org?.slug}/settings/templates`);
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
                    <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.push(`/orgs/${org?.slug}/settings/scopes`)} />
                </ContentHeader>
            </Route>
            <Route exact path='/orgs/:orgId/settings/templates'>
                <ContentProgress progressHidden={progressHidden && templates !== undefined} />
                <ContentHeader title='Project Templates' />
            </Route>
            <Route exact path={['/orgs/:orgId/settings/templates/new']}>
                <ContentProgress progressHidden={progressHidden && templates !== undefined} />
                <ContentHeader title='New Project Template'>
                    <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => history.push(`/orgs/${org?.slug}/settings/templates`)} />
                </ContentHeader>
            </Route>

            <ContentContainer>
                <Route exact path='/orgs/:orgId/settings'>
                    <OrgSettingsOverview {...{}} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/members'>
                    <MemberList {...{ members: members, onAddUsers: onAddUsers }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/scopes'>
                    <DeploymentScopeList {...{}} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/scopes/new'>
                    <DeploymentScopeForm {...{ onCreateDeploymentScope: _onCreateDeploymentScope }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/templates'>
                    <ProjectTemplateList {...{}} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/templates/new'>
                    <ProjectTemplateForm {...{ onCreateProjectTemplate: _onCreateProjectTemplate }} />
                </Route>
            </ContentContainer>
        </Stack>
    );
}
