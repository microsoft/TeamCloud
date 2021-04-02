// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useParams } from 'react-router-dom';
import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';

export const useProject = () => {

    const { projectId } = useParams() as { projectId: string };

    const isAuthenticated = useIsAuthenticated();

    // const queryClient = useQueryClient();

    const { data: org } = useOrg();

    return useQuery(['org', org?.id, 'project', projectId], async () => {
        const { data } = await api.getProject(projectId, org!.id);
        return data;
    }, {
        enabled: isAuthenticated && !!org?.id && !!projectId
    });


    // const handleMessage = useCallback((action: string, data: any) => {

    //     const message = data as Message;

    //     if (!message)
    //         throw Error('Message is not in the correct format');

    //     let typeQueries: [string[]] = [[]];
    //     let itemQueries: [string[]] = [[]];

    //     message.items.forEach(item => {

    //         if (!item.organization || !item.project || !item.type || !item.id)
    //             throw Error('Missing required stuff');

    //         let queryId = ['org', item.organization, 'project', item.project];

    //         if (item.component)
    //             queryId.push('component', item.component);

    //         queryId.push(item.type);

    //         if (!typeQueries.includes(queryId))
    //             typeQueries.push(queryId);

    //         queryId.push(item.id);

    //         if (!itemQueries.includes(queryId))
    //             itemQueries.push(queryId);
    //     });

    //     switch (action) {
    //         case 'create':
    //             typeQueries.forEach(q => queryClient.invalidateQueries(q));
    //             break;
    //         case 'update':
    //             itemQueries.forEach(q => queryClient.invalidateQueries(q));
    //             typeQueries.forEach(q => queryClient.invalidateQueries(q));
    //             break;
    //         case 'delete':
    //             itemQueries.forEach(q => queryClient.removeQueries(q));
    //             typeQueries.forEach(q => queryClient.invalidateQueries(q));
    //             break;
    //         // case 'custom':
    //         //     console.log(`$ unhandled ${action}: ${data}`);
    //         //     break;
    //         default:
    //             console.log(`$ unhandled ${action}: ${data}`);
    //             break;
    //     }
    // }, [queryClient]);


    // useEffect(() => {
    //     const _resolve = async () => {
    //         try {
    //             await resolveSignalR(project, handleMessage);
    //         } catch (error) {
    //             console.error(error);
    //         }
    //     }
    //     _resolve();
    // }, [project, handleMessage]);
}
