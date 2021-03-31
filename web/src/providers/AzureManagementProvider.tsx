// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { ManagementGroup, Subscription } from '../model';
import { AzureManagementContext } from '../Context';
import { endsWithAnyLowerCase } from '../Utils';
import { getManagementGroups, getSubscriptions } from '../Azure';

export const AzureManagementProvider = (props: any) => {

    const location = useLocation();
    const isAuthenticated = useIsAuthenticated();

    const [subscriptions, setSubscriptions] = useState<Subscription[]>();
    const [managementGroups, setManagementGroups] = useState<ManagementGroup[]>();

    useEffect(() => { // Azure Subscriptions
        if (isAuthenticated) {
            if (endsWithAnyLowerCase(location.pathname, '/orgs/new', '/scopes/new') && subscriptions === undefined) {
                const _setSubscriptions = async () => {
                    console.log(`- setSubscriptions`);
                    try {
                        const subs = await getSubscriptions();
                        setSubscriptions(subs ?? []);
                    } catch (error) {
                        setSubscriptions([]);
                    } finally {
                        console.log(`+ setSubscriptions`);
                    }
                };
                _setSubscriptions();
            }
        } else if (subscriptions) {
            console.log(`+ setSubscriptions (undefined)`);
            setSubscriptions(undefined);
        }
    }, [isAuthenticated, subscriptions, location]);


    useEffect(() => { // Azure Management Groups
        if (isAuthenticated) {
            if (endsWithAnyLowerCase(location.pathname, '/orgs/new', '/scopes/new') && managementGroups === undefined) {
                const _setManagementGroups = async () => {
                    console.log(`setManagementGroups`);
                    try {
                        const groups = await getManagementGroups();
                        setManagementGroups(groups ?? []);
                    } catch (error) {
                        setManagementGroups([]);
                    }
                };
                _setManagementGroups();
            }
        } else {
            console.log(`setManagementGroups (undefined)`);
            setManagementGroups(undefined);
        }
    }, [isAuthenticated, managementGroups, location]);


    return <AzureManagementContext.Provider value={{
        subscriptions: subscriptions,
        managementGroups: managementGroups
    }} {...props} />
}
