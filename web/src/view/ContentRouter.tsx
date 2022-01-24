// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Route, Routes } from 'react-router-dom';
import { Error404, NewOrgView, NewProjectView, ProjectView, ProjectsView, OrgSettingsView } from '.';
import { ErrorHandler } from './ErrorHandler';

export const ContentRouter: React.FC = () => (
    <div style={{ height: "100%", overflow: "scroll", overflowX: "hidden", overflowY: "auto" }}>
        <ErrorHandler>
            <Routes>
                <Route path='' element={<></>} />
                <Route path='orgs/new' element={<NewOrgView {...{}} />} />
                <Route path='orgs/:orgId' element={<ProjectsView {...{}} />} />
                <Route path='orgs/:orgId/projects/new' element={<NewProjectView {...{}} />} />
                <Route path='orgs/:orgId/settings/*' element={<OrgSettingsView {...{}} />} />
                <Route path='orgs/:orgId/projects/:projectId/*' element={<ProjectView {...{}} />} />
            </Routes>
        </ErrorHandler>
    </div>
);
