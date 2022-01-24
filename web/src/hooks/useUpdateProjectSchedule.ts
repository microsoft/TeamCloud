// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { useNavigate, useParams } from 'react-router-dom';
import { Schedule } from 'teamcloud';
import { api } from '../API';
import { useOrg, useProject } from '.';

export const useUpdateProjectSchedule = () => {

    const navigate = useNavigate();

    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    // const { data: schedules } = useProjectSchedules();

    const queryClient = useQueryClient();

    return useMutation(async (schedule: Schedule) => {
        if (!project) throw Error('No project')

        const { data } = await api.updateSchedule(schedule.id, project.organization, project.id, {
            body: schedule,
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
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
