// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { DefaultButton, getTheme, Link, Persona, PersonaSize, PrimaryButton, Stack, Text, TextField } from '@fluentui/react';
import { ProjectMember } from '../model';
import { ContentSeparator, UserPersona } from '.';
import { useOrg, useUser, useProject, useProjectMembers } from '../hooks';

export const ProjectSettingsOverview: React.FC = () => {

    const { data: org } = useOrg();
    const { data: user } = useUser();

    const { data: project } = useProject();
    const { data: members } = useProjectMembers();

    const [owner, setOwner] = useState<ProjectMember>();

    const theme = getTheme();

    useEffect(() => {
        if (project && members) {
            if (owner === undefined || owner.projectMembership.projectId !== project.id) {
                const find = members?.find(m => m.projectMembership.role.toLowerCase() === 'owner');
                // console.log(`+ setProjectOwner (${project.slug})`)
                setOwner(find);
            }
        } else if (owner) {
            // console.log(`+ setProjectOwner (undefined)`)
            setOwner(undefined);
        }
    }, [project, members, owner])


    return org && project ? (
        <Stack styles={{ root: { maxWidth: '600px' } }} tokens={{ childrenGap: '20px' }}>
            <Stack.Item>
                <Stack horizontal horizontalAlign='space-between'>
                    <Stack.Item grow>
                        <Stack tokens={{ childrenGap: '14px' }}>
                            <Stack.Item>
                                <TextField
                                    readOnly
                                    label='Name'
                                    description='Project display name'
                                    defaultValue={project.displayName} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    readOnly label='Slug' defaultValue={project.slug}
                                    description='The slug can be used in place of the ID in URLs and API calls' />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    readOnly label='Url'
                                    defaultValue={`${window.location.protocol}//${window.location.host}/orgs/${org.slug}/projects/${project.slug}`} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    readOnly label='Region' defaultValue={org.location ?? undefined} />
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
                    <Stack.Item styles={{ root: { paddingTop: '12px' } }}>
                        <Persona
                            hidePersonaDetails
                            text={project.displayName}
                            size={PersonaSize.size100}
                            styles={{ root: { paddingLeft: '100px' } }}
                            coinProps={{ styles: { initials: { borderRadius: '4px' } } }} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <ContentSeparator />
            </Stack.Item>
            <Stack.Item>
                <Stack tokens={{ childrenGap: '14px' }}>
                    <Stack.Item>
                        <Text variant='xLarge' >Project owner</Text>
                    </Stack.Item>
                    <Stack.Item>
                        <UserPersona user={owner?.graphUser} showSecondaryText />
                    </Stack.Item>
                    <Stack.Item>
                        <DefaultButton
                            disabled={!(owner && user && owner.user.id === user.id)}
                            text='Change owner' />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <ContentSeparator />
            </Stack.Item>
            <Stack.Item>
                <Stack tokens={{ childrenGap: '14px' }}>
                    <Stack.Item>
                        <TextField
                            readOnly
                            label='Resource Group'
                            description='Project resouce group ID'
                            defaultValue={project.resourceId ?? undefined} />
                    </Stack.Item>
                    {project.resourceId && (
                        <Stack.Item>
                            <Link target='_blank' href={`https://portal.azure.com/#@${org.tenant}/resource${project.resourceId}`}>
                                View in Azure Portal
                        </Link>
                        </Stack.Item>
                    )}
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <ContentSeparator />
            </Stack.Item>
            <Stack.Item>
                <Stack tokens={{ childrenGap: '6px' }}>
                    <Stack.Item>
                        <Text variant='xLarge' >Delete project</Text>
                    </Stack.Item>
                    <Stack.Item>
                        <Text>This will affect all contents and members of this organization.</Text>
                    </Stack.Item>
                    <Stack.Item styles={{ root: { paddingTop: '12px' } }}>
                        <PrimaryButton
                            disabled={!(owner && user && owner.user.id === user.id)}
                            text='Delete'
                            styles={{
                                root: { backgroundColor: theme.palette.red, border: '1px solid transparent' },
                                rootHovered: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                                rootPressed: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                                label: { fontWeight: 700 }
                            }} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <ContentSeparator />
            </Stack.Item>
        </Stack>
    ) : <></>;
}
