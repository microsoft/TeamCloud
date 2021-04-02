// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { DefaultButton, Stack, Panel, getTheme, Separator, PrimaryButton } from '@fluentui/react';
import { auth } from '../API';
import { UserPersona } from '.';
import { useGraphUser } from '../hooks';

export const UserInfo: React.FC = () => {

    const [panelOpen, setPanelOpen] = useState(false);
    // const [editPanelOpen, setEditPanelOpen] = useState(false);

    const { data: graphUser } = useGraphUser();

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
                user={graphUser}
                styles={personaStyles}
                onClick={() => setPanelOpen(true)} />
            <Panel
                isLightDismiss
                styles={panelStyles}
                isOpen={panelOpen}
                hasCloseButton={false}
                onDismiss={() => setPanelOpen(false)} >
                <Stack>
                    <UserPersona user={graphUser} large />
                    <Separator />
                    <Stack tokens={{ childrenGap: '8px' }}>
                        <PrimaryButton text='Edit' onClick={() => { }} />
                        {/* <PrimaryButton text='Edit' onClick={() => setEditPanelOpen(true)} /> */}
                        <DefaultButton text='Sign out' onClick={() => auth.logout()} />
                        {/* <DefaultButton text='Sign out' onClick={() => props.onSignOut()} /> */}
                    </Stack>
                </Stack>
            </Panel>
            {/* <UserForm
                me={true}
                user={props.user}
                graphUser={graphUser}
                panelIsOpen={editPanelOpen}
                onFormClose={() => setEditPanelOpen(false)} /> */}
        </>
    ) : <></>;
}
