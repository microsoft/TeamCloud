// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { DefaultButton, Stack, Panel, getTheme, Separator, PrimaryButton, Text } from '@fluentui/react';
import { auth } from '../API';
import { UserForm, UserPersona } from '.';
import { useGraphUser, useUser } from '../hooks';

export const UserInfo: React.FC = () => {

    const [panelOpen, setPanelOpen] = useState(false);
    const [editPanelOpen, setEditPanelOpen] = useState(false);

    const { data: graphUser } = useGraphUser();
    const { data: user } = useUser();

    const theme = getTheme();

    const personaStyles = {
        root: {
            minHeight: '48px',
            paddingLeft: '10px',
            paddingRight: '16px',
            selectors: {
                ':hover': {
                    cursor: 'pointer',
                    background: theme.palette.neutralLighter
                }
            }
        },
        primaryText: {
            color: theme.palette.themePrimary,
            selectors: {
                ':hover': {
                    cursor: 'pointer',
                    color: theme.palette.themePrimary,
                    background: theme.palette.neutralLighter
                }
            }
        }
    };

    const panelStyles = {
        root: { marginTop: '48px' },
        content: { paddingTop: '12px' },
        main: { height: 'fit-content' }
    };

    return graphUser ? (
        <>
            <UserPersona
                hidePersonaDetails
                principal={graphUser}
                styles={personaStyles}
                onClick={() => setPanelOpen(true)} />
            <Panel
                isLightDismiss
                styles={panelStyles}
                isOpen={panelOpen}
                hasCloseButton={false}
                onDismiss={() => setPanelOpen(false)} >
                <Stack>
                    <UserPersona principal={graphUser} large />
                    <Separator />
                    <Stack tokens={{ childrenGap: '8px' }}>
                        {/* <PrimaryButton text='Edit' onClick={() => { }} /> */}
                        <PrimaryButton text='Edit' onClick={() => setEditPanelOpen(true)} />
                        <DefaultButton text='Sign out' onClick={() => auth.logout()} />
                        {/* <DefaultButton text='Sign out' onClick={() => props.onSignOut()} /> */}
                    </Stack>
                    <Separator />
                    <Text styles={{ root: { color: theme.palette.neutralSecondary, padding: '0px', textAlign: 'center' } }}>{process.env.REACT_APP_VERSION}</Text>
                </Stack>
            </Panel>
            <UserForm
                me
                user={user}
                graphUser={graphUser}
                panelIsOpen={editPanelOpen}
                onFormClose={() => setEditPanelOpen(false)} />
        </>
    ) : <></>;
}
