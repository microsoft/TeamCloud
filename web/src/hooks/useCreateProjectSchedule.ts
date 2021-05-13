// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { useHistory, useParams } from 'react-router-dom';
import { ErrorResult, ScheduleDefinition } from 'teamcloud';
import { api } from '../API';
import { useOrg, useProject, useProjectSchedules } from '.';

export const useCreateProjectSchedule = () => {

    const history = useHistory();

    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    const { data: schedules } = useProjectSchedules();

    const queryClient = useQueryClient();

    return useMutation(async (scheduletDef: ScheduleDefinition) => {
        if (!project) throw Error('No project')

        const { data, code, _response } = await api.createSchedule(project.organization, project.id, { body: scheduletDef });

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'schedule', data.id], data)
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'schedule'], schedules ? [...schedules, data] : [data])

                history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/settings/schedules`);
            }
        }
    }).mutateAsync
}
