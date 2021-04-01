// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Route, useHistory } from 'react-router-dom';
import { IconButton, Stack } from '@fluentui/react';
import { DeploymentScopeDefinition, ProjectTemplateDefinition } from 'teamcloud';
import { OrgSettingsOverview, DeploymentScopeList, ProjectTemplateList, ContentHeader, ContentContainer, ContentProgress, MemberList, DeploymentScopeForm, ProjectTemplateForm, NoData } from '../components';
import { useAzureManagement, useOrg } from '../Hooks';
import business from '../img/MSC17_business_001_noBG.png'

export const OrgSettingsView: React.FC = () => {

    const history = useHistory();

    const [progressHidden, setProgressHidden] = useState(true);

    const { subscriptions } = useAzureManagement();

    const { org, members, scopes, templates, createDeploymentScope, createProjectTemplate, addUsers: onAddUsers } = useOrg();


    const _createDeploymentScope = async (scope: DeploymentScopeDefinition) => {
        setProgressHidden(false);
        await createDeploymentScope(scope);
        setProgressHidden(true);
        history.push(`/orgs/${org?.slug}/settings/scopes`);
    };


    const _createProjectTemplate = async (template: ProjectTemplateDefinition) => {
        setProgressHidden(false);
        await createProjectTemplate(template);
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
                {/* <ContentProgress progressHidden={progressHidden && subscriptions !== undefined && managementGroups !== undefined} /> */}
                <ContentProgress progressHidden={progressHidden && subscriptions !== undefined} />
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
            <Route exact path='/orgs/:orgId/settings/audit'>
                <ContentProgress progressHidden={true} />
                <ContentHeader title='Auditing' />
            </Route>
            <Route exact path='/orgs/:orgId/settings/usage'>
                <ContentProgress progressHidden={true} />
                <ContentHeader title='Usage' />
            </Route>


            <ContentContainer>
                <Route exact path='/orgs/:orgId/settings'>
                    <OrgSettingsOverview {...{}} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/members'>
                    <MemberList {...{ members: members, addUsers: onAddUsers }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/scopes'>
                    <DeploymentScopeList {...{}} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/scopes/new'>
                    <DeploymentScopeForm {...{ createDeploymentScope: _createDeploymentScope }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/templates'>
                    <ProjectTemplateList {...{}} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/templates/new'>
                    <ProjectTemplateForm {...{ createProjectTemplate: _createProjectTemplate }} />
                </Route>
                <Route exact path='/orgs/:orgId/settings/audit'>
                    <NoData image={business} title='Coming soon' description='Come back to see usage policy and compliance infomation.' />
                </Route>
                <Route exact path='/orgs/:orgId/settings/usage'>
                    <NoData image={business} title='Coming soon' description='Come back to see usage information per org, project, and user.' />
                </Route>

            </ContentContainer>
        </Stack>
    );
}
