// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Stack, Dropdown, IDropdownOption, Label, Panel, PrimaryButton, DefaultButton, Spinner, Text } from '@fluentui/react';
import { ProjectMembershipRole, UserDefinition, ErrorResult, StatusResult } from 'teamcloud';
import { GraphUser, Member } from '../model'
import { api } from '../API';
import { MemberPicker } from '.';
import { useParams } from 'react-router-dom';

export interface IMembersFormProps {
    members?: Member[];
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const MembersForm: React.FC<IMembersFormProps> = (props) => {

    let { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [userIdentifiers, setUserIdentifiers] = useState<string[]>();
    const [userRole, setUserRole] = useState<ProjectMembershipRole>();
    const [errorText, setErrorText] = useState<string>();

    const _submitForm = async () => {
        setFormEnabled(false);

        if (orgId && userRole && userIdentifiers?.length && userIdentifiers.length > 0) {

            const userDefinitions: UserDefinition[] = userIdentifiers!.map(i => ({
                identifier: i,
                role: userRole
            }));

            const results = await Promise
                .all(userDefinitions.map(async d => projectId ? await api.createProjectUser(orgId, projectId, { body: d }) : await api.createOrganizationUser(orgId, { body: d })));

            let errors: ErrorResult[] = [];
            results.forEach(r => {
                if ((r as StatusResult).code !== 202 && (r as ErrorResult).errors)
                    errors.push((r as ErrorResult));
            });
            if (errors.length > 0) {
                errors.forEach(e => console.log(e));
                setErrorText(`The following errors occured: ${errors.map(e => e.status).join()}`);
            } else {
                _resetAndCloseForm();
            }
        }
    };

    const _resetAndCloseForm = () => {
        setUserRole(undefined);
        setUserIdentifiers(undefined);
        setFormEnabled(true);
        props.onFormClose();
    };

    const _projectRoleOptions = (): IDropdownOption[] => projectId
        ? ['Member', 'Owner'].map(r => ({ key: r, text: r } as IDropdownOption))
        : ['Creator', 'Owner'].map(r => ({ key: r, text: r } as IDropdownOption));

    const _onUserRoleDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption): void => {
        setUserRole(option ? option.key as ProjectMembershipRole : undefined);
    };

    const _onMembersChanged = (users?: GraphUser[]) => {
        setUserIdentifiers(users?.map(u => u.id))
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton text='Add members' disabled={!formEnabled || !(userRole && userIdentifiers?.length && userIdentifiers.length > 0)} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }} />
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
            <Text>{errorText}</Text>
        </Panel>
    );
}

