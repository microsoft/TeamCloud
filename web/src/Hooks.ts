// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useEffect, useRef, useContext } from 'react';
import { AzureManagementContext, GraphUserContext, OrgContext, OrgsContext, ProjectContext } from './Context';

export const useInterval = (callback: () => void, delay?: number) => {

    const savedCallback = useRef(callback);

    // Remember the latest callback.
    useEffect(() => {
        savedCallback.current = callback;
    }, [callback]);

    // Set up the interval.
    useEffect(() => {
        const tick = () => {
            savedCallback.current();
        }
        if (delay !== undefined) {
            let id = setInterval(tick, delay);
            return () => clearInterval(id);
        }
    }, [delay]);
}

export const useGraphUser = () => {
    const context = useContext(GraphUserContext);
    if (!context) {
        throw new Error('useGraphUser must be used within a GraphUserProvider')
    }
    return context
}

export const useAzureManagement = () => {
    const context = useContext(AzureManagementContext);
    if (!context) {
        throw new Error('useAzureManagement must be used within a AzureManagementProvider')
    }
    return context
}

export const useOrgs = () => {
    const context = useContext(OrgsContext);
    if (!context) {
        throw new Error('useOrgs must be used within a OrgsProvider')
    }
    return context
}

export const useOrg = () => {
    const context = useContext(OrgContext);
    if (!context) {
        throw new Error('useOrg must be used within a OrgProvider')
    }
    return context
}

export const useProject = () => {
    const context = useContext(ProjectContext);
    if (!context) {
        throw new Error('useProject must be used within a ProjectProvider')
    }
    return context
}
