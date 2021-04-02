// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { DefaultButton, getTheme, Link, Persona, PersonaSize, PrimaryButton, Stack, Text, TextField } from '@fluentui/react';
import React, { useEffect, useState } from 'react';
import { Member } from '../model';
import { ContentSeparator, UserPersona } from '.';
import { useOrg, useUser, useMembers } from '../hooks';

export const OrgSettingsOverview: React.FC = () => {

    const { data: org } = useOrg();
    const { data: user } = useUser();
    const { data: members } = useMembers();

    const [owner, setOwner] = useState<Member>();

    const theme = getTheme();

    useEffect(() => {
        if (org && members) {
            if (owner === undefined || owner.user.organization !== org.id) {
                const find = members.find(m => m.user.role.toLowerCase() === 'owner');
                console.log(`+ setOwner (${org.slug})`)
                setOwner(find);
            }
        } else if (owner) {
            console.log(`+ setOwner (undefined})`)
            setOwner(undefined);
        }
    }, [org, members, owner])

    return org ? (
        <Stack styles={{ root: { maxWidth: '600px' } }} tokens={{ childrenGap: '20px' }}>
            <Stack.Item>
                <Stack horizontal horizontalAlign='space-between'>
                    <Stack.Item grow>
                        <Stack tokens={{ childrenGap: '14px' }}>
                            <Stack.Item>
                                <TextField
                                    readOnly
                                    label='Name'
                                    description='Organization display name'
                                    defaultValue={org.displayName} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    readOnly label='Slug' defaultValue={org.slug}
                                    description='The slug can be used in place of the ID in URLs and API calls' />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    readOnly label='Url'
                                    defaultValue={`${window.location.protocol}//${window.location.host}/orgs/${org.slug}`} />
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
                            text={org.displayName}
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
                        <Text variant='xLarge' >Organization owner</Text>
                    </Stack.Item>
                    <Stack.Item>
                        <UserPersona user={owner?.graphUser} showSecondaryText />
                    </Stack.Item>
                    <Stack.Item>
                        <DefaultButton
                            disabled={!(owner && user && owner.user.id === user.id)}
                            text='Change owner'
                            onClick={() => console.log(owner)} />
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
                            label='Subscription'
                            description='Organization subscription'
                            defaultValue={org.subscriptionId ?? undefined} />
                    </Stack.Item>
                    <Stack.Item>
                        <TextField
                            readOnly
                            label='Resource Group'
                            description='Organization resouce group ID'
                            defaultValue={org.resourceId ?? undefined} />
                    </Stack.Item>
                    {org.resourceId && (
                        <Stack.Item>
                            <Link target='_blank' href={`https://portal.azure.com/#@${org.tenant}/resource${org.resourceId}`}>
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
                        <Text variant='xLarge' >Delete organization</Text>
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
