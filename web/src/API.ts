// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


import { Auth } from './Auth';
import { Project, TeamCloud } from 'teamcloud';
import { HubConnection, HubConnectionBuilder, IHttpConnectionOptions } from '@microsoft/signalr'

const _getApiUrl = () => {
    if (!process.env.REACT_APP_TC_API_URL) throw new Error('Must set env variable $REACT_APP_TC_API_URL');
    return process.env.REACT_APP_TC_API_URL;
};

export const apiUrl = _getApiUrl();

export const auth = new Auth();
export const api = new TeamCloud(auth, apiUrl, { credentialScopes: [] });

const httpOptions: IHttpConnectionOptions = {
    accessTokenFactory: async () => (await auth.getToken())?.token ?? '',
}

let connection: HubConnection | undefined

export const resolveSignalR = async (project: Project | undefined) => {

    if (!project) {
        await stopSignalR()
        return;
    }

    await startSignalR(project);
}

export const startSignalR = async (project: Project) => {

    const endpoint = `${apiUrl}/orgs/${project.organization}/projects/${project.id}`;

    if (connection) {
        if (connection.baseUrl === endpoint) {
            console.log('Already started.')
            return;
        } else {
            await stopSignalR();
        }
    }

    connection = new HubConnectionBuilder().withUrl(endpoint, httpOptions).build();

    connection.on('create', data => {
        console.log(`$ create: ${data}`);
    })

    connection.on('update', data => {
        console.log(`$ update: ${data}`);
    })

    connection.on('delete', data => {
        console.log(`$ delete: ${data}`);
    })

    connection.on('custom', data => {
        console.log(`$ custom: ${data}`);
    })

    await connection.start();
}

export const stopSignalR = async () => {
    if (connection) {
        await connection.stop();
        connection = undefined;
    }
}
