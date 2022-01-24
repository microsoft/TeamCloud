// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Route, Routes, useNavigate } from 'react-router-dom';
import { IconButton, Stack } from '@fluentui/react';
import { DeploymentScopeDefinition, ProjectTemplateDefinition } from 'teamcloud';
import { OrgSettingsOverview, DeploymentScopeList, ProjectTemplateList, ContentHeader, ContentContainer, ContentProgress, MemberList, DeploymentScopeForm, ProjectTemplateForm, NoData, AuditList } from '../components';
import { useOrg, useAzureSubscriptions, useMembers, useDeploymentScopes, useProjectTemplates, useCreateDeploymentScope, useCreateProjectTemplate, useAddMembers } from '../hooks';

import business from '../img/MSC17_business_001_noBG.png'

export const OrgSettingsView: React.FC = () => {

    const navigate = useNavigate();

    const [progressHidden, setProgressHidden] = useState(true);

    const { data: org, isLoading: orgIsLoading } = useOrg();
    const { data: members, isLoading: membersIsLoading } = useMembers();

    const { isLoading: scopesIsLoading } = useDeploymentScopes();
    const { isLoading: templatesIsLoading } = useProjectTemplates();
    const { isLoading: subscriptionsIsLoading } = useAzureSubscriptions();

    const createDeploymentScope = useCreateDeploymentScope();
    const createProjectTemplate = useCreateProjectTemplate();
    const addMembers = useAddMembers();


    const _createDeploymentScope = async (scope: DeploymentScopeDefinition) => {
        setProgressHidden(false);
        await createDeploymentScope(scope);
        setProgressHidden(true);
        navigate(`/orgs/${org?.slug}/settings/scopes`);
    };


    const _createProjectTemplate = async (template: ProjectTemplateDefinition) => {
        setProgressHidden(false);
        await createProjectTemplate(template);
        setProgressHidden(true);
        navigate(`/orgs/${org?.slug}/settings/templates`);
    };


    return (
        <Stack>
            <Routes>
                <Route path='' element={<ContentProgress progressHidden={progressHidden && !orgIsLoading} />} />
                <Route path='members' element={<ContentProgress progressHidden={progressHidden && !membersIsLoading} />} />
                <Route path='scopes' element={<ContentProgress progressHidden={progressHidden && !scopesIsLoading} />} />
                <Route path='scopes/new' element={<ContentProgress progressHidden={progressHidden && !subscriptionsIsLoading} />} />
                <Route path='templates' element={<ContentProgress progressHidden={progressHidden && !templatesIsLoading} />} />
                <Route path='templates/new' element={<ContentProgress progressHidden={progressHidden && !templatesIsLoading} />} />
                <Route path='audit' element={<ContentProgress progressHidden />} />
                <Route path='usage' element={<ContentProgress progressHidden />} />
            </Routes>

            <Routes>
                <Route path='' element={<ContentHeader title={org?.displayName} />} />
                <Route path='members' element={<ContentHeader title='Members' />} />
                <Route path='scopes' element={<ContentHeader title='Deployment Scopes' />} />
                <Route path='scopes/new' element={
                    <ContentHeader title='New Deployment Scope'>
                        <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => navigate(`/orgs/${org?.slug}/settings/scopes`)} />
                    </ContentHeader>} />
                <Route path='templates' element={<ContentHeader title='Project Templates' />} />
                <Route path='templates/new' element={
                    <ContentHeader title='New Project Template'>
                        <IconButton iconProps={{ iconName: 'ChromeClose' }} onClick={() => navigate(`/orgs/${org?.slug}/settings/templates`)} />
                    </ContentHeader>} />
                <Route path='audit' element={<ContentHeader title='Auditing' />} />
                <Route path='usage' element={<ContentHeader title='Usage' />} />
            </Routes>

            <ContentContainer>
                <Routes>
                    <Route path='' element={<OrgSettingsOverview {...{}} />} />
                    <Route path='members' element={<MemberList {...{ members: members, addMembers: addMembers }} />} />
                    <Route path='scopes' element={<DeploymentScopeList {...{}} />} />
                    <Route path='scopes/new' element={<DeploymentScopeForm {...{ createDeploymentScope: _createDeploymentScope }} />} />
                    <Route path='templates' element={<ProjectTemplateList {...{}} />} />
                    <Route path='templates/new' element={<ProjectTemplateForm {...{ createProjectTemplate: _createProjectTemplate }} />} />
                    <Route path='audit' element={<AuditList {...{}} />} />
                    <Route path='usage' element={<NoData image={business} title='Coming soon' description='Come back to see usage information per org, project, and user.' />} />
                </Routes>
            </ContentContainer>
        </Stack>
    );
}
