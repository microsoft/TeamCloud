// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQueryClient, useMutation } from "react-query";
import { useNavigate } from "react-router-dom";
import { ComponentTask } from "teamcloud";
import { useOrg, useProject, useProjectComponent, useProjectComponentTasks, useUrl } from ".";
import { api, onResponse } from "../API";

export const useCancelProjectComponentTask = () => {

    const navigate = useNavigate();

    const { orgId, projectId, itemId } = useUrl() as { orgId: string, projectId: string, itemId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    const { data: component } = useProjectComponent();
    const { data: componentTasks } = useProjectComponentTasks();

    const queryClient = useQueryClient();

    return useMutation(async (componentTask: ComponentTask) => {

        const { data } = await api.cancelComponentTask(componentTask?.organization, componentTask.projectId, componentTask.componentId, componentTask.id, {
            onResponse: onResponse
        });

        return data;
    }, {
        onSuccess: data => {
            if (data) {

                queryClient.setQueryData(['org', data.organization, 'project', data.projectId, 'component', data.componentId, 'componenttask'], componentTasks?.map(ct => ct.id === data.id ? data : ct))
                queryClient.setQueryData(['org', data.organization, 'project', data.projectId, 'component', data.componentId, 'componenttask', data.id], data)

                navigate(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug ?? itemId}/tasks/${data.id}`);
            }
        }
    }).mutateAsync
}

