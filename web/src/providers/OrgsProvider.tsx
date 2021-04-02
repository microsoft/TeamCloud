// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useHistory } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { DeploymentScopeDefinition, OrganizationDefinition, ProjectTemplateDefinition } from 'teamcloud';
import { OrgsContext } from '../Context';
import { api } from '../API';

export const OrgsProvider = (props: any) => {

    const history = useHistory();

    const isAuthenticated = useIsAuthenticated();

    const queryClient = useQueryClient();

    const createOrg = useMutation(async (def: { orgDef: OrganizationDefinition, scopeDef?: DeploymentScopeDefinition, templateDef?: ProjectTemplateDefinition }) => {

        console.log(`- createOrg`);
        const orgResponse = await api.createOrganization({ body: def.orgDef });
        const newOrg = orgResponse.data;
        console.log(`+ createOrg`);

        let scope, template;

        if (newOrg?.id) {
            if (def.scopeDef) {
                console.log(`- createDeploymentScope`);
                const scopeResponse = await api.createDeploymentScope(newOrg.id, { body: def.scopeDef });
                scope = scopeResponse.data;
                console.log(`+ createDeploymentScope`);
            }
            if (def.templateDef) {
                console.log(`- createProjectTemplate`);
                const templateResponse = await api.createProjectTemplate(newOrg.id, { body: def.templateDef });
                template = templateResponse.data;
                console.log(`+ createProjectTemplate`);
            }
        }

        return { org: newOrg, scope: scope, template: template }

    }, {
        onSuccess: data => {
            if (data.org) {

                queryClient.invalidateQueries('orgs');

                if (data.scope) {
                    console.log(`+ setDeploymentScopes (${data.org.slug})`);
                    queryClient.setQueryData(['org', data.org.id, 'scopes'], [data.scope])
                }

                if (data.template) {
                    console.log(`+ setProjectTemplates (${data.org.slug})`);
                    queryClient.setQueryData(['org', data.org.id, 'templates'], [data.template])
                }

                console.log(`+ setOrg (${data.org.slug})`);
                queryClient.setQueryData(['org', data.org.slug], data.org)

                history.push(`/orgs/${data.org.slug}`);
            }
        }
    })

    const { data: orgs } = useQuery('orgs', async () => {
        console.log('- setOrgs');
        const response = await api.getOrganizations();
        console.log('+ setOrgs');
        return response.data;
    }, {
        enabled: isAuthenticated
    });


    return <OrgsContext.Provider value={{
        orgs: orgs,
        createOrg: createOrg.mutateAsync,
    }} {...props} />
}
