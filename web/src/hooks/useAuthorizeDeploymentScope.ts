import { useQueryClient, useMutation } from "react-query";
import { DeploymentScope } from "teamcloud";
import { useDeploymentScopes } from ".";
import { api } from "../API";



export const useAuthorizeDeployemntScope = () => {

	const { data: deploymentScopes } = useDeploymentScopes();

    const queryClient = useQueryClient();

    return useMutation(async (deploymentScope: DeploymentScope) => {

        const { data } = await api.initializeAuthorization(deploymentScope.organization, deploymentScope.id, {
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });

        return data;
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.setQueryData(['org', data.organization, 'deploymentscope'], deploymentScopes?.map(ds => ds.id === data.id ? data : ds));
                queryClient.setQueryData(['org', data.organization, 'deploymentscope', data.id], data);
            }
        }
    }).mutateAsync
}

