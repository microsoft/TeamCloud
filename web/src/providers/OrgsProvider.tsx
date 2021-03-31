// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { Organization } from 'teamcloud';
import { OrgsContext } from '../Context';
import { matchesRouteParam } from '../Utils';
import { api } from '../API';

export const OrgsProvider = (props: any) => {

    const { orgId } = useParams() as { orgId: string };

    const isAuthenticated = useIsAuthenticated();

    const [org, setOrg] = useState<Organization>();
    const [orgs, setOrgs] = useState<Organization[]>();

    const onOrgSelected = useCallback((selectedOrg?: Organization) => {
        if (selectedOrg && org && selectedOrg.id === org.id)
            return;
        console.log(`+ setOrg (${selectedOrg?.slug})`);
        setOrg(selectedOrg);
        // setProjects(undefined);
    }, [org]);

    useEffect(() => { // Ensure selected Org matches route
        if (orgId) {
            if (org && matchesRouteParam(org, orgId)) {
                return;
            } else if (orgs) {
                const find = orgs.find(o => matchesRouteParam(o, orgId));
                if (find) {
                    console.log(`+ getOrgFromRoute (${orgId})`);
                    onOrgSelected(find);
                }
            }
        } else if (org) {
            console.log(`+ getOrgFromRoute (undefined)`);
            onOrgSelected(undefined);
        }
    }, [orgId, org, orgs, onOrgSelected]);

    useEffect(() => { // Orgs
        if (isAuthenticated) {
            // no orgs OR selected org isn't in the orgs list (new org was created)
            if (orgs === undefined || (org && !orgs.some(o => o.id === org.id))) {
                const _setOrgs = async () => {
                    console.log('- setOrgs');
                    const result = await api.getOrganizations();
                    setOrgs(result.data ?? undefined);
                    console.log('+ setOrgs');
                };
                _setOrgs();
            }
        } else if (orgs) {
            console.log('+ setOrgs (undefined)');
            setOrgs(undefined);
        }
    }, [isAuthenticated, org, orgs]);

    return <OrgsContext.Provider value={{
        org: org,
        orgs: orgs,
        onOrgSelected: onOrgSelected,
    }} {...props} />

}
