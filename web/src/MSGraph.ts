// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Client as GraphClient, GraphError, ResponseType } from '@microsoft/microsoft-graph-client'
import { auth } from './API'
import { GraphUser } from './model';

export enum PhotoSize {
    size48x48 = '48x48',
    size64x64 = '64x64',
    size96x96 = '96x96',
    size120x120 = '120x120',
    size240x240 = '240x240',
    size360x360 = '360x360',
    size432x432 = '432x432',
    size504x504 = '504x504',
    size648x648 = '648x648'
}

const Client = GraphClient;
const client = Client.initWithMiddleware({ authProvider: auth });

const _userSelect = ['id', 'userPrincipalName', 'displayName', 'givenName', 'sirname', 'mail', 'otherMails', 'companyName', 'jobTitle', 'preferredLanguage', 'userType', 'department']

export const getMe = async (): Promise<GraphUser> => {
    let response = await client
        .api('/me')
        .select(_userSelect)
        .get();
    let me = response as GraphUser;
    if (me.userType?.toLowerCase() === 'member')
        me.imageUrl = await getMePhoto();
    return me;
}

export const getGraphUser = async (id: string): Promise<GraphUser> => {
    try {
        let response = await client
            .api('/users/' + id)
            .select(_userSelect)
            // .header('X-PeopleQuery-QuerySources', 'Directory')
            .get();
        let user = response as GraphUser;
        if (user.userType?.toLowerCase() === 'member')
            user.imageUrl = await getUserPhoto(user.id);
        return user;
    } catch (error) {
        console.error(error as GraphError);
        throw error;
    }
}

export const getGraphUsers = async (): Promise<GraphUser[]> => {
    try {
        let response = await client
            .api('/users')
            .select(_userSelect)
            // .header('X-PeopleQuery-QuerySources', 'Directory')
            .get();
        let users: GraphUser[] = response.value;
        await Promise.all(users.map(async u => u.imageUrl = (u.userType?.toLowerCase() === 'member') ? await getUserPhoto(u.id) : undefined));
        return users;
    } catch (error) {
        console.error(error as GraphError);
        throw error;
    }
}

export const searchGraphUsers = async (search: string): Promise<GraphUser[]> => {
    try {
        let response = await client
            .api('/users')
            .filter(`startswith(displayName,'${search}')`)
            .select(_userSelect)
            // .header('X-PeopleQuery-QuerySources', 'Directory')
            .get();
        let users: GraphUser[] = response.value;
        await Promise.all(users.map(async u => u.imageUrl = (u.userType?.toLowerCase() === 'member') ? await getUserPhoto(u.id) : undefined));
        return users;
    } catch (error) {
        console.error(error as GraphError);
        throw error;
    }
}

export const getMePhoto = async (size: PhotoSize = PhotoSize.size240x240): Promise<string | undefined> => {
    try {
        let api = `/me/photos/${size}/$value`;
        // let api = `/me/photo/$value`;
        let response = await client
            .api(api)
            .header('Cache-Control', 'no-cache')
            .version('beta') // currently only work in beta: https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/568
            .responseType(ResponseType.BLOB)
            .get();
        return URL.createObjectURL(response);
    } catch (error) {

        if ((error as GraphError).statusCode === 404) return undefined;

        // console.warn('Failed to get me photo.');
        // console.warn(error as GraphError);

        // swollow this error see: https://docs.microsoft.com/en-us/graph/known-issues#photo-restrictions
        return undefined;
        // throw error;
    }
}

export const getUserPhoto = async (id: string, size: PhotoSize = PhotoSize.size240x240): Promise<string | undefined> => {
    try {
        let api = `/users/${id}/photos/${size}/$value`;
        let response = await client
            .api(api)
            .header('Cache-Control', 'no-cache')
            .version('beta') // currently only work in beta: https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/568
            .responseType(ResponseType.BLOB)
            .get();
        return URL.createObjectURL(response);
    } catch (error) {
        if ((error as GraphError).statusCode === 404) return undefined;

        // console.warn(`Failed to get user photo (${id}).`);
        // console.error(error as GraphError);

        // swollow this error see: https://docs.microsoft.com/en-us/graph/known-issues#photo-restrictions
        return undefined;
        // throw error;
    }
}

export const getGraphDirectoryObject = async (id: string): Promise<GraphUser> => {
    try {
        let response = await client
            .api('/directoryObjects/' + id)
            // .header('X-PeopleQuery-QuerySources', 'Directory')
            .get();
        return response as GraphUser;
    } catch (error) {
        console.error(error as GraphError);
        throw error;
    }
}

export const getGraphDirectoryObjects = async (): Promise<GraphUser[]> => {
    try {
        let response = await client
            .api('/directoryObjects')
            // .header('X-PeopleQuery-QuerySources', 'Directory')
            .get();
        return response.value as GraphUser[];
    } catch (error) {
        console.error(error as GraphError);
        throw error;
    }
}
