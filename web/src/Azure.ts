// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { auth } from './API';
import { ManagementGroup, ManagementGroups, ResourceGroup, ResourceGroups, Subscription, Subscriptions } from './model';

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

export const getManagementGroups = async (): Promise<ManagementGroup[]> => {

    const url = 'https://management.azure.com/providers/Microsoft.Management/managementGroups?api-version=2020-02-01';

    const token = await auth.getManagementToken();

    console.log('==> GET ' + url);

    let response: Response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token?.token}`
        }
    });

    console.log('<== GET ' + url);

    const json = await response.json();

    // console.log('=== JSON (' + url + ')');
    // console.log(JSON.stringify(json));

    const groups = json as ManagementGroups;

    return groups.value;
};


export const getManagementGroup = async (id: string): Promise<ManagementGroup> => {

    const url = `https://management.azure.com/providers/Microsoft.Management/managementGroups/${id}?api-version=2020-02-01&$expand=children`;

    console.log('==> GET ' + url);

    const token = await auth.getManagementToken();

    let response: Response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token?.token}`
        }
    });

    console.log('<== GET ' + url);

    const json = await response.json();

    // console.log('=== JSON (' + url + ')');
    // console.log(JSON.stringify(json));

    const group = json as ManagementGroup;

    return group;
};


export const getSubscriptions = async (): Promise<Subscription[]> => {

    const url = `https://management.azure.com/subscriptions?api-version=2020-01-01`;

    console.log('==> GET ' + url);

    const token = await auth.getManagementToken();

    let response: Response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token?.token}`
        }
    });

    console.log('<== GET ' + url);

    const json = await response.json();

    // console.log('=== JSON (' + url + ')');
    // console.log(JSON.stringify(json));

    const subscriptions = json as Subscriptions;

    return subscriptions.value;
};

export const getResourceGroups = async (subscription: string): Promise<ResourceGroup[]> => {

    const url = `https://management.azure.com/subscriptions/${subscription}/resourceGroups?api-version=2020-06-01`;

    console.log('==> GET ' + url);

    const token = await auth.getManagementToken();

    let response: Response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token?.token}`
        }
    });

    console.log('<== GET ' + url);

    const json = await response.json();

    // console.log('=== JSON (' + url + ')');
    // console.log(JSON.stringify(json));

    const groups = json as ResourceGroups;

    return groups.value;
};
