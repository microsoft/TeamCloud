import { useQueryClient, useMutation } from "react-query";
import { useNavigate, useParams } from "react-router-dom";
import { ComponentTask } from "teamcloud";
import { useOrg, useProject, useProjectComponent, useProjectComponentTasks } from ".";
import { api } from "../API";



export const useCancelProjectComponentTask = () => {

    const navigate = useNavigate();

    const { orgId, projectId, itemId } = useParams() as { orgId: string, projectId: string, itemId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    const { data: component } = useProjectComponent();
    const { data: componentTasks } = useProjectComponentTasks();

    const queryClient = useQueryClient();

    return useMutation(async (componentTask: ComponentTask) => {

        const { data } = await api.cancelComponentTask(componentTask?.organization, componentTask.projectId, componentTask.componentId, componentTask.id, {
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
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

