// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Auth } from './Auth';
import { ErrorResult, Project, TeamCloud } from 'teamcloud';
import { HubConnection, HubConnectionBuilder, IHttpConnectionOptions } from '@microsoft/signalr'

const _getApiUrl = () => {
    if (process.env.NODE_ENV !== 'production') {
        if (!process.env.REACT_APP_TC_API_URL) throw new Error('Must set env variable $REACT_APP_TC_API_URL');
        return process.env.REACT_APP_TC_API_URL;
    }
    
    return "__REACT_APP_TC_API_URL__";
};

export const apiUrl = _getApiUrl();

export const auth = new Auth();
// export const api = new TeamCloud(auth, apiUrl, { credentialScopes: [], retryOptions: { maxRetries: 0 } });
export const api = new TeamCloud(auth, apiUrl, { credentialScopes: [] });

const httpOptions: IHttpConnectionOptions = {
    accessTokenFactory: async () => (await auth.getToken())?.token ?? '',
}

export const onResponse = (raw: any, flat: unknown) => {
    if (raw.status >= 400) {
        const error = flat as ErrorResult
        if (error)
            throw error
        throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
    }
}

let connection: HubConnection | undefined

export const resolveSignalR = async (project: Project | undefined, callback: (action: string, data: any) => void) => {

    if (!project) {
        await stopSignalR()
        return;
    }

    await startSignalR(project, callback);
}

export const startSignalR = async (project: Project, callback: (action: string, data: any) => void) => {

    try {
        const endpoint = `${apiUrl}/orgs/${project.organization}/projects/${project.id}`;

        if (connection) {
            if (connection.baseUrl === endpoint) {
                console.log('Already started.')
                return;
            } else {
                await stopSignalR();
            }
        }

        connection = new HubConnectionBuilder().withUrl(endpoint, httpOptions).withAutomaticReconnect().build();

        connection.onreconnecting((err) => console.log('Reconnecting SignalR'));
        connection.onreconnected((err) => console.log('Reconnected SignalR'));

        connection.on('create', data => {
            console.log(`Received SignalR Message CREATE: ${JSON.stringify(data)}`);
            callback('create', data)
        });

        connection.on('update', data => {
            console.log(`Received SignalR Message UPDATE: ${JSON.stringify(data)}`);
            callback('update', data)
        });

        connection.on('delete', data => {
            console.log(`Received SignalR Message DELETE: ${JSON.stringify(data)}`);
            callback('delete', data)
        });

        await connection.start();
    }
    catch (err) {
        console.error(err);
    }
}

export const stopSignalR = async () => {
    if (connection) {
        await connection.stop();
        connection = undefined;
    }
}
