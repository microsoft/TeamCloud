// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


import { Auth } from './Auth';
import { TeamCloud } from 'teamcloud';

const _getApiUrl = () => {
    if (!process.env.REACT_APP_TC_API_URL) throw new Error('Must set env variable $REACT_APP_TC_API_URL');
    return process.env.REACT_APP_TC_API_URL;
};

const apiUrl = _getApiUrl();

export const auth = new Auth();
export const api = new TeamCloud(auth, apiUrl, { credentialScopes: [] });
