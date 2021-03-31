// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { useIsAuthenticated } from '@azure/msal-react';
import { GraphUser } from '../model';
import { GraphUserContext } from '../Context';
import { getMe } from '../MSGraph';

export const GraphUserProvider = (props: any) => {

    const isAuthenticated = useIsAuthenticated();

    const [graphUser, setGraphUser] = useState<GraphUser>();

    useEffect(() => { // Graph User
        if (isAuthenticated) {
            if (graphUser === undefined) {
                const _setGraphUser = async () => {
                    console.log(`- setGraphUser`);
                    const result = await getMe();
                    setGraphUser(result);
                    console.log(`+ setGraphUser`);
                };
                _setGraphUser();
            }
        } else if (graphUser) {
            console.log(`+ setGraphUser (undefined)`);
            setGraphUser(undefined);
        }
    }, [isAuthenticated, graphUser]);


    return <GraphUserContext.Provider value={{
        graphUser: graphUser,
    }} {...props} />
}
