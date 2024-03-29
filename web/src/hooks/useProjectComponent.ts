// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useProject, useUrl } from '.';
import { useErrorHandler } from '../view/ErrorHandler';
import { HttpError } from '../Errors';

export const useProjectComponent = (required?: boolean) => {

    const { navId, itemId } = useUrl() as { navId: string, itemId: string };
    const { data: project } = useProject();

    const isAuthenticated = useIsAuthenticated();
    const { setErrorStatusCode } = useErrorHandler();

    const handleError = (err: Error) => {
        if (err instanceof HttpError && err.statusCode === 404)
            setErrorStatusCode(err.statusCode);
        else
            throw err;
    };

    // TODO: what?
    return useQuery(['org', project?.organization, 'project', project?.id, 'component', itemId], async () => {

        const { data } = await api.getComponent(itemId, project!.organization, project!.id, {
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new HttpError(raw.status, raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`);
            }
        });

        return data;

    }, {
        enabled: isAuthenticated && !!project?.id && !!navId && navId.toLowerCase() === 'components' && !!itemId,
        retry: (cnt, err) => !(err instanceof HttpError && err.statusCode === 404),
        onError: required ? handleError : undefined
    });
}
