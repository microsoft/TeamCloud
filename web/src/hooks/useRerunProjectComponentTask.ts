import { useQueryClient, useMutation } from "react-query";
import { useNavigate } from "react-router";
import { ComponentTask } from "teamcloud";
import { useOrg, useProject, useProjectComponent, useProjectComponentTasks, useUrl } from ".";
import { api, onResponse } from "../API";


export const useRerunProjectComponentTask = () => {

    const navigate = useNavigate();

    const { orgId, projectId, itemId } = useUrl() as { orgId: string, projectId: string, itemId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    const { data: component } = useProjectComponent();
    const { data: componentTasks } = useProjectComponentTasks();

    const queryClient = useQueryClient();

    return useMutation(async (componentTask: ComponentTask) => {

        const { data } = await api.reRunComponentTask(componentTask?.organization, componentTask.projectId, componentTask.componentId, componentTask.id, {
            onResponse: onResponse
        });

        return data;
    }, {
        onSuccess: data => {
            if (data) {

                var componentTasksSanitized = componentTasks?.map(ct => ct.id === data.id ? data : ct);

                queryClient.setQueryData(['org', org?.id, 'project', project?.id, 'component', component?.id, 'componenttask', data.id], data)
                queryClient.setQueryData(['org', org?.id, 'project', project?.id, 'component', component?.id, 'componenttask', data.id, 'componenttask'], componentTasksSanitized)

                navigate(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug ?? itemId}/tasks/${data.id}`);
            }
        }
    }).mutateAsync
}