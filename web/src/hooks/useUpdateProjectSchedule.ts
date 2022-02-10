// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { useNavigate } from 'react-router-dom';
import { Schedule } from 'teamcloud';
import { api, onResponse } from '../API';
import { useOrg, useProject, useUrl } from '.';

export const useUpdateProjectSchedule = () => {

    const navigate = useNavigate();

    const { orgId, projectId } = useUrl() as { orgId: string, projectId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    // const { data: schedules } = useProjectSchedules();

    const queryClient = useQueryClient();

    return useMutation(async (schedule: Schedule) => {
        if (!project) throw Error('No project')

        const { data } = await api.updateSchedule(schedule.id, project.organization, project.id, {
            body: schedule,
            onResponse: onResponse
        });

        return data;
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.invalidateQueries(['org', project?.organization, 'project', project?.id, 'schedule'])
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'schedule', data.id], data)

                navigate(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/settings/schedules`);
            }
        }
    }).mutateAsync
}
