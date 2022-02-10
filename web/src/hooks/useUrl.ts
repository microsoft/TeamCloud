// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useEffect, useState } from "react";
import { useLocation } from 'react-router-dom';
import { matchesLowerCase } from "../Utils";

export const useUrl = () => {

    const { pathname } = useLocation();

    const [orgId, setOrgId] = useState<string>();
    const [projectId, setProjectId] = useState<string>();
    const [settingId, setSettingId] = useState<string>();
    const [navId, setNavId] = useState<string>();
    const [subnavId, setSubnavId] = useState<string>();
    const [itemId, setItemId] = useState<string>();
    const [subitemId, setSubitemId] = useState<string>();

    useEffect(() => {

        const parts = pathname.split('/').filter(e => e);

        const length = parts.length;

        let org = undefined;
        let project = undefined;
        let setting = undefined;
        let nav = undefined;
        let subnav = undefined;
        let item = undefined;
        let subitem = undefined;

        if (length >= 2 && matchesLowerCase('orgs', parts[0]) && !matchesLowerCase('new', parts[1])) {

            org = parts[1]; // orgs/:orgId

            if (length >= 3 && matchesLowerCase('settings', parts[2])) {

                if (length >= 4) {
                    setting = parts[3]; // orgs/:orgId/settings/:settingId

                    if (length >= 5 && !matchesLowerCase('new', parts[4]))
                        item = parts[4] // orgs/:orgId/settings/:settingId/:itemId
                }

            } else if (length >= 4 && matchesLowerCase('projects', parts[2]) && !matchesLowerCase('new', parts[3])) {

                project = parts[3]; // orgs/:orgId/projects/:projectId

                if (length >= 5) {

                    if (matchesLowerCase('settings', parts[4])) {

                        if (length >= 6) {
                            setting = parts[5]; // orgs/:orgId/projects/:projectId/settings/:settingId

                            if (length >= 7 && !matchesLowerCase('new', parts[6]))
                                item = parts[6]; // orgs/:orgId/projects/:projectId/settings/:settingId/:itemId
                        }
                    } else {

                        nav = parts[4]; // orgs/:orgId/projects/:projectId/:navId

                        if (length >= 6 && !matchesLowerCase('new', parts[5])) {
                            item = parts[5]; // orgs/:orgId/projects/:projectId/:navId/:itemId

                            if (length >= 7) {
                                subnav = parts[6]; // orgs/:orgId/projects/:projectId/:navId/:itemId/:subNavId

                                if (length >= 8)
                                    subitem = parts[7]; // orgs/:orgId/projects/:projectId/:navId/:itemId/:subNavId/:subItemId
                            }
                        }
                    }
                }
            }
        }

        if (org !== orgId)
            setOrgId(org);
        if (project !== projectId)
            setProjectId(project);
        if (setting !== settingId)
            setSettingId(setting);
        if (nav !== navId)
            setNavId(nav);
        if (subnav !== subnavId)
            setSubnavId(subnav);
        if (item !== itemId)
            setItemId(item);
        if (subitem !== subitemId)
            setSubitemId(subitem);

    }, [itemId, navId, orgId, pathname, projectId, settingId, subitemId, subnavId]);

    return { orgId: orgId, projectId: projectId, settingId: settingId, navId: navId, subnavId: subnavId, itemId: itemId, subitemId: subitemId }
}
