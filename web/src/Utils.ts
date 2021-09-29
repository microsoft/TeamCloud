// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { KnownComponentTaskState, Project } from "teamcloud";

export const matchesLowerCase = (id: string, param: string) =>
    (id && param) ? id.toLowerCase() === param.toLowerCase() : id === param;

export const matchesAnyLowerCase = (id: string, ...params: string[]) =>
    params.some(p => matchesLowerCase(id, p));

export const matchesRouteParam = (obj: { id: string, slug: string }, param: string) =>
    matchesLowerCase(obj.id, param) || matchesLowerCase(obj.slug, param);

export const endsWithLowerCase = (path: string, check: string) =>
    (path && check) ? path.toLowerCase().endsWith(check.toLowerCase()) : false;

export const endsWithAnyLowerCase = (path: string, ...checks: string[]) =>
    checks.some(c => endsWithLowerCase(path, c));

export const includesLowerCase = (path: string, check: string) =>
    (path && check) ? path.toLowerCase().includes(check.toLowerCase()) : false;

export const matchesParent = (list: [{ id: string }], parent: { id: string }) =>
    list && !list.some(i => i.id !== parent.id);

export const matchesProject = (list: [{ projectId: string }], project: Project) =>
    list && !list.some(i => i.projectId !== project.id);

export const undefinedOrWrongParent = (list: [{ id: string }], parent: { id: string }) =>
    list === undefined || list.some(i => i.id !== parent.id);

export const prettyPrintCamlCaseString = (value: string) =>
    value.charAt(0).toUpperCase() + value.slice(1).replace(/\B[A-Z]/g, " $&");

export const isActiveComponentTaskState = (value: KnownComponentTaskState | undefined) : boolean => 
    (value && [ KnownComponentTaskState.Pending, KnownComponentTaskState.Initializing, KnownComponentTaskState.Processing ].includes(value)) as boolean; 

export const isFinalComponentTaskState = (value: KnownComponentTaskState | undefined) : boolean =>
    (value && [ KnownComponentTaskState.Succeeded, KnownComponentTaskState.Failed, KnownComponentTaskState.Canceled ].includes(value)) as boolean;