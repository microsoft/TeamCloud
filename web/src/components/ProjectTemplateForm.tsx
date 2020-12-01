// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { DefaultButton, PrimaryButton, Stack, TextField } from '@fluentui/react';
import { ProjectTemplateDefinition } from 'teamcloud';
import { useHistory, useParams } from 'react-router-dom';

export interface IProjectTemplateFormProps {
    onCreateProjectTemplate: (template: ProjectTemplateDefinition) => Promise<void>;
}

export const ProjectTemplateForm: React.FC<IProjectTemplateFormProps> = (props) => {

    const history = useHistory();
    const { orgId } = useParams() as { orgId: string };

    // Project Template
    const [templateName, setTemplateName] = useState<string>();
    const [templateUrl, setTemplateUrl] = useState<string>();
    const [templateVersion, setTemplateVersion] = useState<string>();
    const [templateToken, setTemplateToken] = useState<string>();

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    // const [errorText, setErrorText] = useState<string>();

    const _templateComplete = () => templateName && templateUrl;

    const _submitForm = () => {
        if (orgId && _templateComplete()) {

            setFormEnabled(false);

            const templateDef = {
                displayName: templateName,
                repository: {
                    url: templateUrl,
                    version: templateVersion ?? null,
                    token: templateToken ?? null
                }
            } as ProjectTemplateDefinition;

            props.onCreateProjectTemplate(templateDef);
        }
    };

    const _resetAndCloseForm = () => {
        setFormEnabled(true);
        history.replace(`/orgs/${orgId}/settings/templates`);
    };

    return (
        <Stack tokens={{ childrenGap: '20px' }}>
            <Stack.Item>
                <TextField
                    required
                    label='Name'
                    description='Project template display name'
                    disabled={!formEnabled}
                    value={templateName}
                    onChange={(_ev, val) => setTemplateName(val)} />
            </Stack.Item>
            <Stack.Item>
                <TextField
                    required
                    label='Url'
                    description='Git repository https url'
                    disabled={!formEnabled}
                    value={templateUrl}
                    onChange={(_ev, val) => setTemplateUrl(val)} />
            </Stack.Item>
            <Stack.Item>
                <TextField
                    label='Version'
                    description='Branch/Tag/SHA'
                    disabled={!formEnabled}
                    value={templateVersion}
                    onChange={(_ev, val) => setTemplateVersion(val)} />
            </Stack.Item>
            <Stack.Item>
                <TextField
                    label='Token'
                    description='Personal access token (required for private repositories)'
                    disabled={!formEnabled}
                    value={templateToken}
                    onChange={(_ev, val) => setTemplateToken(val)} />
            </Stack.Item>
            <Stack.Item styles={{ root: { paddingTop: '24px' } }}>
                <PrimaryButton text='Create template' disabled={!formEnabled || !_templateComplete()} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
                <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            </Stack.Item>
            {/* <Text>{errorText}</Text> */}
        </Stack>
    );
}
