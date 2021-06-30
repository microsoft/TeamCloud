// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


export const DaysOfWeek = [0, 1, 2, 3, 4, 5, 6];

export const DaysOfWeekNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

export const getNames = (daysOfWeek: number[]): string[] => daysOfWeek.map(d => DaysOfWeekNames[d])

export const shiftToUtc = (daysOfWeek: number[] | string[], date: Date): { indices: number[], names: string[] } => {

    if (daysOfWeek.length < 1)
        return { indices: [], names: [] };

    const localDate = date.getDate();
    const utcDate = date.getUTCDate();

    const day = daysOfWeek[0];

    let days: number[] = (typeof day === 'string' ? (daysOfWeek as string[]).map(d => DaysOfWeekNames.indexOf(d)) : daysOfWeek) as number[];

    days.sort();

    if (localDate > utcDate) {
        days = days.map(d => d === 0 ? 6 : d - 1);
    } else if (localDate < utcDate) {
        days = days.map(d => d === 6 ? 0 : d + 1);
    }

    return { indices: days, names: days.map(d => DaysOfWeekNames[d]) };
}

export const shiftToLocal = (daysOfWeek: number[] | string[], date: Date): { indices: number[], names: string[] } => {

    if (daysOfWeek.length < 1)
        return { indices: [], names: [] };

    const localDate = date.getDate();
    const utcDate = date.getUTCDate();

    const day = daysOfWeek[0];

    let days: number[] = (typeof day === 'string' ? (daysOfWeek as string[]).map(d => DaysOfWeekNames.indexOf(d)) : daysOfWeek) as number[];

    days.sort();

    if (localDate < utcDate) {
        days = days.map(d => d === 0 ? 6 : d - 1);
    } else if (localDate > utcDate) {
        days = days.map(d => d === 6 ? 0 : d + 1);
    }

    return { indices: days, names: days.map(d => DaysOfWeekNames[d]) };
}
