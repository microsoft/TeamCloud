// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useIsAuthenticated } from '@azure/msal-react';
import { GraphUserContext } from '../Context';
import { getMe } from '../MSGraph';
import { useQuery } from 'react-query';

export const GraphUserProvider = (props: any) => {

    const isAuthenticated = useIsAuthenticated();

    const { data: graphUser } = useQuery('graphUser', async () => {
        console.log(`- setGraphUser`);
        const response = await getMe();
        console.log(`+ setGraphUser`);
        return response
    }, {
        refetchOnMount: false,
        refetchOnWindowFocus: false,
        enabled: isAuthenticated
    });

    return <GraphUserContext.Provider value={{
        graphUser: graphUser,
    }} {...props} />
}
