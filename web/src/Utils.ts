// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
