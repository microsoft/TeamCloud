import { useQueryClient, useMutation } from "react-query";
import { useHistory, useParams } from "react-router";
import { ComponentTask } from "teamcloud";
import { useOrg, useProject, useProjectComponent, useProjectComponentTasks } from ".";
import { api } from "../API";


export const useRerunProjectComponentTask = () => {

	const history = useHistory();

    const { orgId, projectId, itemId } = useParams() as { orgId: string, projectId: string, itemId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    const { data: component } = useProjectComponent();
    const { data: componentTasks } = useProjectComponentTasks();

    const queryClient = useQueryClient();

    return useMutation(async (componentTask: ComponentTask) => {

        const { data } = await api.reRunComponentTask(componentTask?.organization, componentTask.projectId, componentTask.componentId, componentTask.id, {
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });

        return data;
    }, {
        onSuccess: data => {
            if (data) {

                var componentTasksSanitized = componentTasks?.map(ct => ct.id === data.id ? data : ct);

                queryClient.setQueryData(['org', org?.id, 'project', project?.id, 'component', component?.id, 'componenttask', data.id], data)
                queryClient.setQueryData(['org', org?.id, 'project', project?.id, 'component', component?.id, 'componenttask', data.id, 'componenttask'], componentTasksSanitized)

                history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug ?? itemId}/tasks/${data.id}`);
            }
        }
    }).mutateAsync	
}