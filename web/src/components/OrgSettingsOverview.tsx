// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { DefaultButton, getTheme, Persona, PersonaSize, PrimaryButton, Separator, Stack, Text, TextField } from '@fluentui/react';
import React, { useEffect, useState } from 'react';
import { Organization, User } from 'teamcloud';
import { Member } from '../model';


export interface IOrgSettingsOverviewProps {
    user?: User;
    org?: Organization
    members?: Member[];
}

export const OrgSettingsOverview: React.FC<IOrgSettingsOverviewProps> = (props) => {

    const [owner, setOwner] = useState<Member>();

    const { members } = props;

    const theme = getTheme();

    useEffect(() => {
        if (members && owner === undefined) {
            const find = members?.find(m => m.user.role.toLowerCase() === 'admin');
            setOwner(find);
        }
    }, [members, owner])

    return props.org ? (
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
                                    defaultValue={props.org.displayName} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    readOnly
                                    label='Slug'
                                    // description='Organization slug'
                                    defaultValue={props.org.slug} />
                            </Stack.Item>
                            <Stack.Item>
                                <TextField
                                    readOnly
                                    label='Url'
                                    // description='Organization url'
                                    defaultValue={`${window.location.protocol}//${window.location.host}/orgs/${props.org.slug}`} />
                            </Stack.Item>
                            <Stack.Item>
                                {/* <Stack>
                                    <Label>Region</Label>
                                    <Text>{props.org.location ?? undefined}</Text>
                                </Stack> */}
                                <TextField
                                    readOnly
                                    label='Region'
                                    // description='Organization region'
                                    defaultValue={props.org.location ?? undefined} />
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
                    <Stack.Item styles={{ root: { paddingTop: '12px' } }}>
                        <Persona
                            hidePersonaDetails
                            text={props.org.displayName}
                            size={PersonaSize.size100}
                            styles={{ root: { paddingLeft: '100px' } }}
                            coinProps={{ styles: { initials: { borderRadius: '4px' } } }} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <Separator styles={{ root: { selectors: { '::before': { backgroundColor: theme.palette.neutralQuaternary } } } }} />
            </Stack.Item>
            <Stack.Item>
                <Stack tokens={{ childrenGap: '14px' }}>
                    <Stack.Item>
                        <Text variant='xLarge' >Organization owner</Text>
                    </Stack.Item>
                    <Stack.Item>
                        <Persona
                            text={owner?.graphUser?.displayName ?? owner?.user.id}
                            showSecondaryText
                            secondaryText={owner?.graphUser?.mail ?? (owner?.graphUser?.otherMails && owner.graphUser.otherMails.length > 0 ? owner.graphUser.otherMails[0] : undefined)}
                            imageUrl={owner?.graphUser?.imageUrl}
                            // styles={{ root: { paddingTop: '24px' } }}
                            size={PersonaSize.size32} />
                    </Stack.Item>
                    <Stack.Item>
                        <DefaultButton
                            disabled
                            text='Change owner' />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <Separator styles={{ root: { selectors: { '::before': { backgroundColor: theme.palette.neutralQuaternary } } } }} />
            </Stack.Item>
            <Stack.Item>
                <Stack tokens={{ childrenGap: '14px' }}>

                    <Stack.Item>
                        <TextField
                            readOnly
                            label='Subscription'
                            description='Organization subscription'
                            defaultValue={props.org.subscriptionId ?? undefined} />
                    </Stack.Item>
                    <Stack.Item>
                        <TextField
                            readOnly
                            label='Resource Group'
                            description='Organization resouce group ID'
                            defaultValue={props.org.resourceId ?? undefined} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <Separator styles={{ root: { selectors: { '::before': { backgroundColor: theme.palette.neutralQuaternary } } } }} />
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
                            // disabled
                            text='Delete'
                            styles={{
                                root: { backgroundColor: theme.palette.red, border: '1px solid transparent' },
                                rootHovered: { backgroundColor: theme.palette.redDark, border: '1px solid transparent' },
                                label: { fontWeight: 700 }
                            }} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <Separator styles={{ root: { selectors: { '::before': { backgroundColor: theme.palette.neutralQuaternary } } } }} />
            </Stack.Item>
        </Stack>
    ) : <></>;

}
