// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { GraphPrincipal } from "./GraphPrincipal";

export interface GraphUser extends GraphPrincipal {
    type: 'User';
    mail?: string;
    userPrincipalName: string;
    givenName?: string;
    surname?: string;
    otherMails?: string[];
    companyName?: string;
    jobTitle?: string;
    preferredLanguage?: string;
    userType?: string;
    department?: string;
    imageUrl?: string;
}
