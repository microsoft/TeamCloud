import React, { useState, useEffect } from 'react';
import { DefaultButton, Stack, Panel, Persona, PersonaSize, getTheme, IPersonaStyles, IPanelStyles } from '@fluentui/react';
import { GraphUser, getGraphUser } from '../Auth';

export interface IUserInfoProps {
    onSignOut: () => void;
}

export const UserInfo: React.FunctionComponent<IUserInfoProps> = (props) => {

    const [user, setUser] = useState<GraphUser>();
    const [panelOpen, setPanelOpen] = useState(false);

    useEffect(() => {
        if (user === undefined) {
            const _setUser = async () => {
                const result = await getGraphUser();
                setUser(result);
            };
            _setUser();
        }
    }, []);

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
        content: { paddingTop: '20px' },
        main: { height: 'fit-content' }
    };


    if (user) {
        return <>
            <Persona
                text={user.displayName}
                // secondaryText={this.state.tenant.displayName || this.props.tenantId}
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
                    <DefaultButton text="Sign out" onClick={() => props.onSignOut} />
                </Stack>
            </Panel>
        </>;
    } else {
        return <></>;
    }
}
