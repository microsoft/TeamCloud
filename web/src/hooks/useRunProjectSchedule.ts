// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { Schedule } from 'teamcloud';
import { api, onResponse } from '../API';

export const useRunProjectSchedule = () => {

    const queryClient = useQueryClient();

    return useMutation(async (schedule: Schedule) => {

        const { data } = await api.runSchedule(schedule.id, schedule.organization, schedule.projectId, {
            onResponse: onResponse
        });

        return data;
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.invalidateQueries(['org', data.organization, 'project', data.projectId, 'schedule'])
                queryClient.setQueryData(['org', data.organization, 'project', data.projectId, 'schedule', data.id], data)
            }
        }
    }).mutateAsync
}
