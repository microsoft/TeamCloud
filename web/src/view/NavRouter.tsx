// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Route, Routes } from 'react-router-dom';
import { OrgSettingsNav, ProjectNav, ProjectSettingsNav, RootNav } from '../components';

export const NavRouter: React.FC = () => (
    <div style={{ height: "calc(100vh - 70px)", overflow: "scroll", overflowX: "hidden", overflowY: "auto" }}>
        <Routes>
            <Route path='' element={<RootNav {...{}} />} />
            <Route path='orgs/:orgId' element={<RootNav {...{}} />} />
            <Route path='orgs/:orgId/settings/*' element={<OrgSettingsNav {...{}} />} />
            <Route path='orgs/:orgId/settings/:settingId/*' element={<OrgSettingsNav {...{}} />} />
            <Route path='orgs/:orgId/projects/new' element={<RootNav {...{}} />} />
            <Route path='orgs/:orgId/projects/:projectId/*' element={<ProjectNav {...{}} />} />
            <Route path='orgs/:orgId/projects/:projectId/:navId/*' element={<ProjectNav {...{}} />} />
            <Route path='orgs/:orgId/projects/:projectId/:navId/:itemId/*' element={<ProjectNav {...{}} />} />
            <Route path='orgs/:orgId/projects/:projectId/:navId/:itemId/tasks/:subitemId/*' element={<ProjectNav {...{}} />} />
            <Route path='orgs/:orgId/projects/:projectId/settings/*' element={<ProjectSettingsNav {...{}} />} />
            <Route path='orgs/:orgId/projects/:projectId/settings/:settingId/*' element={<ProjectSettingsNav {...{}} />} />
            <Route path='orgs/:orgId/projects/:projectId/settings/:settingId/:itemId/*' element={<ProjectSettingsNav {...{}} />} />
        </Routes>
    </div>
);
