// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// import { auth } from './API'
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

const token = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImtnMkxZczJUMENUaklmajRydDZKSXluZW4zOCIsImtpZCI6ImtnMkxZczJUMENUaklmajRydDZKSXluZW4zOCJ9.eyJhdWQiOiJodHRwczovL21hbmFnZW1lbnQuYXp1cmUuY29tIiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvYzJiYzA3NTAtYWI2Yi00NjA2LWI3OTgtZDEwNjczMjA5MzVmLyIsImlhdCI6MTYwNTcyODIyNiwibmJmIjoxNjA1NzI4MjI2LCJleHAiOjE2MDU3MzIxMjYsImFjciI6IjEiLCJhaW8iOiJBVVFBdS84UkFBQUF6Sy9YMmpVU2tBQkJKdlZkL2x4b0pHa3NPSTYwcE82a3ZSb0Nnb0o1cVFjc0xNdmNrQm5ZU3N1ZG85a3lkRjhMNUpWbUprRy9MMk1OWkRaVDFpNFR6Zz09IiwiYWx0c2VjaWQiOiIxOmxpdmUuY29tOjAwMDM0MDAxMThFNjE1MUIiLCJhbXIiOlsicHdkIl0sImFwcGlkIjoiMDk3MTZiYWMtZDllNy00ZDU1LWIwYzgtMzA5ZWE1NDg5YmQyIiwiYXBwaWRhY3IiOiIwIiwiZW1haWwiOiJjb2xieWx3aWxsaWFtc0BvdXRsb29rLmNvbSIsImZhbWlseV9uYW1lIjoiV2lsbGlhbXMiLCJnaXZlbl9uYW1lIjoiQ29sYnkiLCJncm91cHMiOlsiNzI5YjY2MTItYmM4Ni00M2Q2LWIzOTctYjU5MjVlNDNmZmYxIl0sImlkcCI6ImxpdmUuY29tIiwiaXBhZGRyIjoiNzYuMjAuMjQzLjQ4IiwibmFtZSI6IkNvbGJ5IFdpbGxpYW1zIiwib2lkIjoiMTNhYTM4NzYtMzBiMC00MGIxLTg1MTgtY2Y1Nzc3YjVjMTMzIiwicHVpZCI6IjEwMDMyMDAwOTE2Q0M3OTciLCJyaCI6IjAuQUFBQVVBZTh3bXVyQmthM21ORUdjeUNUWDZ4cmNRbm4yVlZOc01nd25xVkltOUpGQUhnLiIsInNjcCI6InVzZXJfaW1wZXJzb25hdGlvbiIsInN1YiI6ImR3NG0ySXFrRzRsS1VWbmZSd2Ixak50NmdWaEhZXzZnUjF6UHlNTTQ1anciLCJ0aWQiOiJjMmJjMDc1MC1hYjZiLTQ2MDYtYjc5OC1kMTA2NzMyMDkzNWYiLCJ1bmlxdWVfbmFtZSI6ImxpdmUuY29tI2NvbGJ5bHdpbGxpYW1zQG91dGxvb2suY29tIiwidXRpIjoiUVgtNHBhdGZ1RUdMQ1JOeUNxWWlBQSIsInZlciI6IjEuMCIsInhtc190Y2R0IjoxNTc2MjkxNDUzfQ.jn9fuUwLO098AyIdQ1mkuZyMCiolOJ5t17mqNnY7DKEtdIs76TcqONHyEWWbwOR2vsiJYmgoP-ee2gNHTb5LcUKncs4uIBbVPFpbHJzfmZ6xuOu2CaJiXxzi6qpgDoK-TKifpmRNjvFwMzf0ElL5rINOP8zycKGmQLYOJCnUPqnb7XzD1eCPcbXFS6fS_3li02M30CmjbEyLq4vT9S4WHlfZxPoZOnRHZRg4Zgfpsm3vCAEBd6WO4kQHXbappYCVPnzh3pGQU6N5DIysJzHHxerEMYkndvimH0Rcdbvo0tp-XbXrd21LOBAKpnfWAZuKKGW259hxioi93bVxvENzKA';

export const getManagementGroups = async (): Promise<ManagementGroup[]> => {

    const url = 'https://management.azure.com/providers/Microsoft.Management/managementGroups?api-version=2020-02-01';

    console.log('==> GET ' + url);

    let response: Response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    console.log('<== GET ' + url);

    const json = await response.json();

    console.log('=== JSON (' + url + ')');
    console.log(JSON.stringify(json));

    const groups = json as ManagementGroups;

    return groups.value;
};


export const getManagementGroup = async (id: string): Promise<ManagementGroup> => {

    const url = `https://management.azure.com/providers/Microsoft.Management/managementGroups/${id}?api-version=2020-02-01&$expand=children`;

    console.log('==> GET ' + url);

    console.log(token)

    let response: Response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    console.log('<== GET ' + url);

    const json = await response.json();

    console.log('=== JSON (' + url + ')');
    console.log(JSON.stringify(json));

    const group = json as ManagementGroup;

    return group;
};


export const getSubscriptions = async (): Promise<Subscription[]> => {

    const url = `https://management.azure.com/subscriptions?api-version=2020-01-01`;

    console.log('==> GET ' + url);

    console.log(token)

    let response: Response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    console.log('<== GET ' + url);

    const json = await response.json();

    console.log('=== JSON (' + url + ')');
    console.log(JSON.stringify(json));

    const subscriptions = json as Subscriptions;

    return subscriptions.value;
};

export const getResourceGroups = async (subscription: string): Promise<ResourceGroup[]> => {

    const url = `https://management.azure.com/subscriptions/${subscription}/resourceGroups?api-version=2020-06-01`;

    console.log('==> GET ' + url);

    console.log(token)

    let response: Response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    console.log('<== GET ' + url);

    const json = await response.json();

    console.log('=== JSON (' + url + ')');
    console.log(JSON.stringify(json));

    const groups = json as ResourceGroups;

    return groups.value;
};
