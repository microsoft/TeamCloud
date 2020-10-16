// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
    TeamCloud,
    StatusResult,
    ErrorResult,
    Project,
    User,
    ProjectType,
    Provider,
    ProjectDefinition,
    UserDefinition,
    ProjectLink,
    ComponentRequest,
    Component,
    ComponentOffer
} from 'teamcloud';
import { DataResult } from './model'
import { Auth } from './Auth';
// import { getToken } from './Auth';
// import { DataResult, StatusResult, ErrorResult, Project, User, ProjectType, Provider, ProjectDefinition, UserDefinition, ProjectLink, ComponentRequest, Component, ComponentOffer } from './model';



const logRequests = true

const _getApiUrl = () => {
    if (!process.env.REACT_APP_TC_API_URL) throw new Error('Must set env variable $REACT_APP_TC_API_URL');
    return process.env.REACT_APP_TC_API_URL;
};

const scope = 'http://TeamCloud.Web/user_impersonation'
const apiUrl = _getApiUrl();

// const token = getToken(scope);
export const auth = new Auth();
export const teamcloud = new TeamCloud(auth, apiUrl);


export const getProject = async (id: string) => (await teamcloud.getProjectByNameOrId(id)).data

// export const getProject = async (id: string) => {
//     return getResource<Project>(`${apiUrl}/api/projects/${id}`);
// }

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


export const getProviderOffer = async (providerId: string, id: string) => getResource<ComponentOffer>(`${apiUrl}/api/providers/${providerId}/offers/${id}`);

export const getProviderOffers = async (providerId: string) => getResource<Array<ComponentOffer>>(`${apiUrl}/api/providers/${providerId}/offers`);


export const getProjectLink = async (projectId: string, id: string) => getResource<ProjectLink>(`${apiUrl}/api/projects/${projectId}/links/${id}`);

export const getProjectLinks = async (projectId: string) => getResource<Array<ProjectLink>>(`${apiUrl}/api/projects/${projectId}/links`);


export const getProjectOffer = async (projectId: string, id: string) => getResource<ComponentOffer>(`${apiUrl}/api/projects/${projectId}/offers/${id}`);

export const getProjectOffers = async (projectId: string) => getResource<Array<ComponentOffer>>(`${apiUrl}/api/projects/${projectId}/offers`);


export const getProjectComponent = async (projectId: string, id: string) => getResource<Component>(`${apiUrl}/api/projects/${projectId}/components/${id}`);

export const getProjectComponents = async (projectId: string) => getResource<Array<Component>>(`${apiUrl}/api/projects/${projectId}/components`);

export const createProjectComponent = async (projectId: string, request: ComponentRequest) => createResource(`${apiUrl}/api/projects/${projectId}/components`, request);

export const updateProjectComponent = async (projectId: string, component: Component) => updateResource<Component>(`${apiUrl}/api/projects/${projectId}/components/${component.id}`, component);

export const deleteProjectComponent = async (projectId: string, id: string) => deleteResource<Component>(`${apiUrl}/api/projects/${projectId}/components/${id}`);

export const getResource = async <T>(url: string): Promise<ErrorResult | DataResult<T>> => {

    let retry = false;

    while (true) {

        if (logRequests) console.log('==> GET ' + url);

        let response: Response = await fetch(url, {
            method: 'GET',
            mode: 'cors',
            credentials: 'include',
            headers: {
                'Authorization': 'Bearer ' + (await auth.getToken(scope))?.token,
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
                'Authorization': 'Bearer ' + (await auth.getToken(scope))?.token,
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
                'Authorization': 'Bearer ' + (await auth.getToken(scope))?.token,
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
                'Authorization': 'Bearer ' + (await auth.getToken(scope))?.token,
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
