// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export interface GraphUser {
    id: string;
    userPrincipalName: string;
    displayName?: string;
    givenName?: string;
    sirname?: string;
    mail?: string;
    otherMails?: string[];
    companyName?: string;
    jobTitle?: string;
    preferredLanguage?: string;
    userType?: string;
    department?: string;
    imageUrl?: string;
}
