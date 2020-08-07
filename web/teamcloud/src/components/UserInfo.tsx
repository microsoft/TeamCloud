import React from 'react';
import { DefaultButton, Stack, Panel, Persona, PersonaSize } from '@fluentui/react';
import { GraphUser, getGraphUser } from '../Auth';
import './UserInfo.css';
// import { Tenant, fetchTenant } from "../models/Tenant";

export interface IUserInfoProps {
    onSignOut: () => void;
}

export interface IUserInfoState {
    initialized: boolean;
    user: GraphUser;
    // tenant: Tenant;
    panelVisible: boolean;
}

export class UserInfo extends React.Component<IUserInfoProps, IUserInfoState> {

    constructor(props: any) {
        super(props);
        this.state = this._getDefaultState();
    }

    private _getDefaultState(): IUserInfoState {
        return {
            initialized: false,
            user: {} as GraphUser,
            // tenant: {} as Tenant,
            panelVisible: false
        };
    }

    async componentDidMount() {

        var promises: any[] = [
            getGraphUser(),
            // fetchTenant(this.props.tenantId)
        ]

        var results = await Promise.all(promises);

        this.setState({
            initialized: true,
            user: results[0],
            // tenant: results[1]
        });
    }

    render() {

        if (this.state.initialized) {

            return <div className="user">
                <Persona
                    text={this.state.user.displayName}
                    // secondaryText={this.state.tenant.displayName || this.props.tenantId}
                    size={PersonaSize.size40}
                    onClick={this._showPanel}
                />
                <Panel isLightDismiss isOpen={this.state.panelVisible} onDismiss={this._hidePanel} className="userPanel">
                    <Stack>
                        <DefaultButton text="sign out" onClick={this._signOut} />
                    </Stack>
                </Panel>
            </div>;

        } else {

            return <></>;

        }
    }

    private _hidePanel = () => {
        this.setState({ panelVisible: false });
    }

    private _showPanel = () => {
        this.setState({ panelVisible: true });
    }

    private _signOut = () => {
        this.props.onSignOut();
    }
}
