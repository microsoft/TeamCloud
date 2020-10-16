// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { DefaultButton, Stack, Panel, Persona, PersonaSize, getTheme, Separator, PrimaryButton } from '@fluentui/react';
import { GraphUser, User } from '../model';
import { UserForm } from './UserForm';

export interface IUserInfoProps {
    user?: User;
    graphUser?: GraphUser;
    onSignOut: () => void;
}

export const UserInfo: React.FunctionComponent<IUserInfoProps> = (props) => {

    const [panelOpen, setPanelOpen] = useState(false);
    const [editPanelOpen, setEditPanelOpen] = useState(false);

    const theme = getTheme();

    const personaStyles = {
        root: {
            minHeight: '56px',
            paddingLeft: '10px',
            selectors: {
                ':hover': {
                    cursor: 'pointer',
                    background: theme.palette.themeDark
                }
            }
        },
        primaryText: {
            color: theme.palette.white,
            selectors: {
                ':hover': {
                    cursor: 'pointer',
                    color: theme.palette.white,
                    background: theme.palette.themeDark
                }
            }
        }
    };

    const panelStyles = {
        root: { marginTop: '56px' },
        content: { paddingTop: '12px' },
        main: { height: 'fit-content' }
    };

    if (props.graphUser) {
        return <>
            <Persona
                text={props.graphUser.displayName}
                // secondaryText={this.state.tenant.displayName || this.props.tenantId}
                imageUrl={props.graphUser.imageUrl}
                size={PersonaSize.size40}
                styles={personaStyles}
                onClick={() => setPanelOpen(true)}
            />
            <Panel
                isLightDismiss
                styles={panelStyles}
                isOpen={panelOpen}
                hasCloseButton={false}
                onDismiss={() => setPanelOpen(false)} >
                <Stack>
                    <Persona
                        text={props.graphUser?.displayName}
                        secondaryText={props.graphUser?.jobTitle}
                        tertiaryText={props.graphUser?.department}
                        imageUrl={props.graphUser?.imageUrl}
                        size={PersonaSize.size72} />
                    <Separator />
                    <Stack tokens={{ childrenGap: '8px' }}>
                        <PrimaryButton text='Edit' onClick={() => setEditPanelOpen(true)} />
                        <DefaultButton text='Sign out' onClick={() => props.onSignOut()} />
                    </Stack>
                </Stack>
            </Panel>
            <UserForm
                me={true}
                user={props.user}
                graphUser={props.graphUser}
                panelIsOpen={editPanelOpen}
                onFormClose={() => setEditPanelOpen(false)} />
        </>;
    } else {
        return <></>;
    }
}
