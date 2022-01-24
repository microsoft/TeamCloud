import { useMutation, useQueryClient } from "react-query";
import { ErrorResult, Project } from "teamcloud";
import { api } from "../API";

export const useDeleteProject = () => {

    const queryClient = useQueryClient();

    return useMutation(async (project: Project) => {
        const result = await api.deleteProject(project.id, project.organization);
        if (result.code !== 202 && (result as ErrorResult).errors) {
            console.log(result as ErrorResult);
        }
        return undefined;
    }, {
        onSuccess: (data, project) => {
            queryClient.setQueryData(['org', project?.organization, 'project', project?.id], data)

            // navigate(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug}`);
        }
    }).mutateAsync
}