// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from 'react-query'
import { DeploymentScopeDefinition, OrganizationDefinition, ProjectTemplateDefinition } from 'teamcloud';
import { api, onResponse } from '../API';

export const useCreateOrg = () => {

    const navigate = useNavigate();
    const queryClient = useQueryClient();

    return useMutation(async (def: { orgDef: OrganizationDefinition, scopeDef?: DeploymentScopeDefinition, templateDef?: ProjectTemplateDefinition }) => {

        console.debug(`- createOrg`);
        const orgResponse = await api.createOrganization({
            body: def.orgDef,
            onResponse: onResponse
        });
        const newOrg = orgResponse.data;
        console.debug(`+ createOrg`);

        let scope, template;

        if (newOrg?.id) {
            if (def.scopeDef) {
                console.debug(`- createDeploymentScope`);
                const scopeResponse = await api.createDeploymentScope(newOrg.id, {
                    body: def.scopeDef,
                    onResponse: onResponse
                });
                scope = scopeResponse.data;
                console.debug(`+ createDeploymentScope`);
            }
            if (def.templateDef) {
                console.debug(`- createProjectTemplate`);
                const templateResponse = await api.createProjectTemplate(newOrg.id, {
                    body: def.templateDef,
                    onResponse: onResponse
                });
                template = templateResponse.data;
                console.debug(`+ createProjectTemplate`);
            }
        }

        return { org: newOrg, scope: scope, template: template }

    }, {
        onSuccess: data => {
            if (data.org) {

                queryClient.invalidateQueries('orgs');

                if (data.scope) {
                    console.debug(`+ setDeploymentScopes (${data.org.slug})`);
                    queryClient.setQueryData(['org', data.org.id, 'scopes'], [data.scope])
                }

                if (data.template) {
                    console.debug(`+ setProjectTemplates (${data.org.slug})`);
                    queryClient.setQueryData(['org', data.org.id, 'templates'], [data.template])
                }

                console.debug(`+ setOrg (${data.org.slug})`);
                queryClient.setQueryData(['org', data.org.slug], data.org)

                navigate(`/orgs/${data.org.slug}`);
            }
        }
    }).mutateAsync
}
