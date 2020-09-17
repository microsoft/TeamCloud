// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { getToken } from './Auth';
import { DataResult, StatusResult, ErrorResult, Project, User, ProjectType, Provider, ProjectDefinition, UserDefinition, ProjectLink } from './model';

const logRequests = false

const _getApiUrl = () => {
    if (!process.env.REACT_APP_TC_API_URL) throw new Error('Must set env variable $REACT_APP_TC_API_URL');
    return process.env.REACT_APP_TC_API_URL;
};

const scope = 'http://TeamCloud.Web/user_impersonation'
const apiUrl = _getApiUrl();

export const getProject = async (id: string) => {
    return getResource<Project>(`${apiUrl}/api/projects/${id}`);
}

export const getProjects = async () => getResource<Array<Project>>(`${apiUrl}/api/projects`);


export const createProject = async (definition: ProjectDefinition) => createResource(`${apiUrl}/api/projects`, definition);

export const deleteProject = async (id: string) => deleteResource<Project>(`${apiUrl}/api/projects/${id}`);


export const getUser = async (id: string) => getResource<User>(`${apiUrl}/api/users/${id}`);

export const getUsers = async () => getResource<Array<User>>(`${apiUrl}/api/users`);

export const createUser = async (definition: UserDefinition) => createResource(`${apiUrl}/api/users`, definition);

export const updateUser = async (user: User) => updateResource(`${apiUrl}/api/users${user.id}`, user);

export const deleteUser = async (id: string) => deleteResource<User>(`${apiUrl}/api/users/${id}`);


export const getProjectUser = async (projectId: string, id: string) => getResource<User>(`${apiUrl}/api/projects/${projectId}/users/${id}`);

export const getProjectUsers = async (projectId: string) => getResource<Array<User>>(`${apiUrl}/api/projects/${projectId}/users`);

export const createProjectUser = async (projectId: string, definition: UserDefinition) => createResource(`${apiUrl}/api/projects/${projectId}/users`, definition);

export const updateProjectUser = async (projectId: string, user: User) => updateResource<User>(`${apiUrl}/api/projects/${projectId}/users/${user.id}`, user);

export const deleteProjectUser = async (projectId: string, id: string) => deleteResource<User>(`${apiUrl}/api/projects/${projectId}/users/${id}`);


export const getProjectType = async (id: string) => getResource<ProjectType>(`${apiUrl}/api/projectTypes/${id}`);

export const getProjectTypes = async () => getResource<Array<ProjectType>>(`${apiUrl}/api/projectTypes`);

export const createProjectType = async (projectType: ProjectType) => createResource(`${apiUrl}/api/projectTypes`, projectType);

export const deleteProjectType = async (id: string) => deleteResource<ProjectType>(`${apiUrl}/api/projectTypes/${id}`);


export const getProvider = async (id: string) => getResource<Provider>(`${apiUrl}/api/providers/${id}`);

export const getProviders = async () => getResource<Array<Provider>>(`${apiUrl}/api/providers`);


export const getProjectLink = async (projectId: string, id: string) => getResource<ProjectLink>(`${apiUrl}/api/projects/${projectId}/links/${id}`);

export const getProjectLinks = async (projectId: string) => getResource<Array<ProjectLink>>(`${apiUrl}/api/projects/${projectId}/links`);


export const getResource = async <T>(url: string): Promise<ErrorResult | DataResult<T>> => {

    let retry = false;

    while (true) {

        if (logRequests) console.log('==> GET ' + url);

        let response: Response = await fetch(url, {
            method: 'GET',
            mode: 'cors',
            credentials: 'include',
            headers: {
                'Authorization': 'Bearer ' + await getToken(scope, retry)
            }
        });

        if (logRequests) console.log('<== GET ' + url);

        retry = response.status === 403 && !retry;

        // let json = await response.json();
        // console.log('=== JSON (' + url + ') ' + JSON.stringify(json));

        if (!retry) {
            try {
                const json = await response.json();
                return response.status >= 400 ? json as ErrorResult : json as DataResult<T>;
            } catch (error) {
                console.log(response)
                console.error(error)
                return { code: response.status, status: response.statusText ?? "Unknown Error" } as ErrorResult
            }
        }
    }
}

export const deleteResource = async <T>(url: string): Promise<ErrorResult | StatusResult | DataResult<T>> => {

    let retry = false;

    while (true) {

        if (logRequests) console.log('==> DELETE ' + url);

        let response: Response = await fetch(url, {
            method: 'DELETE',
            mode: 'cors',
            headers: {
                'Authorization': 'Bearer ' + await getToken(scope, retry)
            }
        });

        if (logRequests) console.log('<== DELETE ' + url);

        // var json = await response.json();
        // console.log('=== JSON (' + url + ') ' + JSON.stringify(json));

        retry = response.status === 403 && !retry;

        if (!retry) {
            try {
                const json = await response.json();
                return response.status === 202 ? json as StatusResult : response.status >= 400 ? json as ErrorResult : json as DataResult<T>;
            } catch (error) {
                console.log(response)
                console.error(error)
                return { code: response.status, status: response.statusText ?? "Unknown Error" } as ErrorResult
            }
        }
    }
}

export const createResource = async<T>(url: string, resource: T): Promise<ErrorResult | StatusResult | DataResult<T>> => {

    let retry = false;
    let body = JSON.stringify(resource);

    while (true) {

        if (logRequests) console.log('==> POST ' + url);

        let response: Response = await fetch(url, {
            method: 'POST',
            mode: 'cors',
            headers: {
                'Authorization': 'Bearer ' + await getToken(scope, retry),
                'Content-Type': 'application/json'
            },
            body: body
        });

        if (logRequests) console.log('<== POST ' + url);

        // var json = await response.json();
        // console.log('=== JSON (' + url + ') ' + JSON.stringify(json));

        retry = response.status === 403 && !retry;

        if (!retry) {
            try {
                const json = await response.json();
                return response.status === 202 ? json as StatusResult : response.status >= 400 ? json as ErrorResult : json as DataResult<T>;
            } catch (error) {
                console.log(response)
                console.error(error)
                return { code: response.status, status: response.statusText ?? "Unknown Error" } as ErrorResult
            }
        }
        // TODO: Poll status
    }
}


export const updateResource = async <T>(url: string, resource: T): Promise<DataResult<T> | StatusResult | ErrorResult> => {

    let retry = false;
    let body = JSON.stringify(resource);

    while (true) {

        console.log('==> PUT ' + url);

        let response: Response = await fetch(url, {
            method: 'PUT',
            mode: 'cors',
            headers: {
                'Authorization': 'Bearer ' + await getToken(scope, retry),
                'Content-Type': 'application/json'
            },
            body: body
        });

        console.log('<== PUT ' + url);

        // var json = await response.json();
        // console.log('=== JSON (' + url + ') ' + JSON.stringify(json));

        retry = response.status === 403 && !retry;

        if (!retry) {
            try {
                const json = await response.json();
                return response.status === 202 ? json as StatusResult : response.status >= 400 ? json as ErrorResult : json as DataResult<T>;
            } catch (error) {
                console.log(response)
                console.error(error)
                return { code: response.status, status: response.statusText ?? "Unknown Error" } as ErrorResult
            }
        }
        // TODO: Poll status
    }
}
