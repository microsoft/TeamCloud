// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Stack, Dropdown, IDropdownOption, Label, Panel, PrimaryButton, DefaultButton, Spinner } from '@fluentui/react';
import { UserDefinition } from 'teamcloud';
import { GraphUser, Member } from '../model'
import { MemberPicker } from '.';
import { useParams } from 'react-router-dom';

export interface IMembersFormProps {
    members?: Member[];
    panelIsOpen: boolean;
    onFormClose: () => void;
    onAddUsers: (users: UserDefinition[]) => Promise<void>;
}

export const MembersForm: React.FC<IMembersFormProps> = (props) => {

    const { projectId } = useParams() as { projectId: string }

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [userIdentifiers, setUserIdentifiers] = useState<string[]>();
    const [userRole, setUserRole] = useState<string>();

    const _submitForm = async () => {
        if (userRole && userIdentifiers?.length && userIdentifiers.length > 0) {
            setFormEnabled(false);

            const userDefinitions: UserDefinition[] = userIdentifiers!.map(i => ({
                identifier: i,
                role: userRole
            }));

            await props.onAddUsers(userDefinitions);

            _resetAndCloseForm();
        }
    };

    const _resetAndCloseForm = () => {
        setUserRole(undefined);
        setUserIdentifiers(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

    const _projectRoleOptions = (): IDropdownOption[] =>
        ['Member', 'Admin'].map(r => ({ key: r, text: r } as IDropdownOption));

    const _onUserRoleDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption): void => {
        setUserRole(option ? option.key as string : undefined);
    };

    const _onMembersChanged = (users?: GraphUser[]) => {
        setUserIdentifiers(users?.map(u => u.id))
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton text='Add members' disabled={!formEnabled || !(userRole && userIdentifiers && userIdentifiers.length > 0)} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
            <DefaultButton text='Cancel' disabled={!formEnabled} onClick={() => _resetAndCloseForm()} />
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    return (
        <Panel
            headerText='Add Members'
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}
            onRenderFooterContent={_onRenderPanelFooterContent}>
            <Stack>
                <Dropdown
                    required
                    label='Role'
                    placeholder='Select a Role'
                    disabled={!formEnabled}
                    options={_projectRoleOptions()}
                    onChange={_onUserRoleDropdownChange} />
                <Label required>Users</Label>
                <MemberPicker
                    members={props.members}
                    formEnabled={formEnabled}
                    onChange={_onMembersChanged} />
            </Stack>
        </Panel>
    );
}

