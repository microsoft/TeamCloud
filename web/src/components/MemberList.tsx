// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { IColumn } from '@fluentui/react';
import { Organization, Project, UserDefinition } from 'teamcloud';
import { Member, ProjectMember } from '../model';
import { ContentList, MembersForm, UserPersona } from '.';

export interface IMemberListProps {
    org?: Organization;
    project?: Project;
    members?: Member[];
    addMembers: (users: UserDefinition[]) => Promise<any>;
}

export const MemberList: React.FC<IMemberListProps> = (props) => {

    const [addMembersPanelOpen, setAddMembersPanelOpen] = useState(false);

    const onRenderMemberColumn = (member?: Member, index?: number, column?: IColumn) => (
        <UserPersona principal={member?.graphPrincipal} showSecondaryText />
    );

    const onRenderRoleColumn = (member?: Member, index?: number, column?: IColumn) => {
        if (props.project) {
            return (member as ProjectMember)?.projectMembership.role;
        } else {
            const role = member?.user.role;
            return role?.toLowerCase() === 'none' ? 'Member' : role;
        }
    }

    const columns: IColumn[] = [
        { key: 'member', name: 'Member', minWidth: 240, isResizable: false, onRender: onRenderMemberColumn },
        { key: 'role', name: 'Role', minWidth: 240, maxWidth: 240, onRender: onRenderRoleColumn },
        { key: 'type', name: 'Type', minWidth: 240, maxWidth: 240, onRender: (m: Member) => m.user.userType }
    ];

    const _applyFilter = (member: Member, filter: string): boolean =>
        filter ? member.graphPrincipal?.displayName?.toUpperCase().includes(filter.toUpperCase()) ?? false : true;

    const _onItemInvoked = (member: Member): void => {
        console.log(member);
    };

    return (
        <>
            <ContentList
                columns={columns}
                items={props.members?.filter(m => m.graphPrincipal)}
                applyFilter={_applyFilter}
                onItemInvoked={_onItemInvoked}
                filterPlaceholder='Filter members'
                buttonText='Add members'
                buttonIcon='Add'
                onButtonClick={() => setAddMembersPanelOpen(true)}
            />
            <MembersForm
                members={props.members}
                panelIsOpen={addMembersPanelOpen}
                onFormClose={() => setAddMembersPanelOpen(false)}
                addMembers={props.addMembers} />
        </>
    );
}
