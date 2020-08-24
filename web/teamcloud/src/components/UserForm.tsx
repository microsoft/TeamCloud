// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from "react";
import { Stack, Dropdown, IDropdownOption, Label, Panel, PrimaryButton, DefaultButton, Spinner, Text } from "@fluentui/react";
import { ProjectUserRole, Project, UserDefinition, ErrorResult, StatusResult } from "../model";
import { GraphUser } from "../MSGraph";
import { createProjectUser } from "../API";
import { ProjectMemberPicker } from "./ProjectMemberPicker";

export interface IUserFormProps {
    project: Project;
    panelIsOpen: boolean;
    onFormClose: () => void;
}

export const UserForm: React.FunctionComponent<IUserFormProps> = (props) => {

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [userIdentifiers, setUserIdentifiers] = useState<string[]>();
    const [userRole, setUserRole] = useState<ProjectUserRole>();
    const [errorText, setErrorText] = useState<string>();

    const _submitForm = async () => {
        setFormEnabled(false);
        if (props.project && userRole && userIdentifiers?.length && userIdentifiers.length > 0) {
            const userDefinitions: UserDefinition[] = userIdentifiers!.map(i => ({
                identifier: i,
                role: userRole
            }));
            const results = await Promise
                .all(userDefinitions.map(async d => await createProjectUser(props.project.id, d)));

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

    const _projectRoleOptions = (): IDropdownOption[] => {
        return [ProjectUserRole.Member, ProjectUserRole.Owner].map(r => ({ key: r, text: r } as IDropdownOption));
    };

    const _onUserRoleDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption): void => {
        setUserRole(option ? option.key as ProjectUserRole : undefined);
    };

    const _onMembersChanged = (users?: GraphUser[]) => {
        setUserIdentifiers(users?.map(u => u.id))
    };

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton disabled={!formEnabled || !(userRole && userIdentifiers?.length && userIdentifiers.length > 0)} onClick={() => _submitForm()} styles={{ root: { marginRight: 8 } }}>
                Add users
            </PrimaryButton>
            <DefaultButton disabled={!formEnabled} onClick={() => _resetAndCloseForm()}>Cancel</DefaultButton>
            <Spinner styles={{ root: { visibility: formEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    return (
        <Panel
            headerText='Add Users'
            isOpen={props.panelIsOpen}
            onDismiss={() => _resetAndCloseForm()}
            onRenderFooterContent={_onRenderPanelFooterContent}>
            <Stack>
                <Dropdown
                    required
                    label='Role'
                    placeHolder='Select a Role'
                    disabled={!formEnabled}
                    options={_projectRoleOptions()}
                    onChange={_onUserRoleDropdownChange} />
                <Label required>Users</Label>
                <ProjectMemberPicker
                    project={props.project}
                    formEnabled={formEnabled}
                    onChange={_onMembersChanged} />
            </Stack>
            <Text>{errorText}</Text>
        </Panel>
    );
}

