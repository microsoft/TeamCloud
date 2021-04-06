// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export interface Message {
    action: string;
    ts: number;
    items: MessageItem[];
}

export interface MessageItem {
    id: string;
    ts: number;
    etag: string;
    slug?: string;
    type: string;
    organization: string;
    project: string;
    component?: string;
}
