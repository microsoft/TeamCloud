// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { IColumn, Persona, PersonaSize } from '@fluentui/react';
import { Project } from 'teamcloud';
import { Member, ProjectMember } from '../model';
import { ContentList, MembersForm } from '.';

export interface IMemberListProps {
    project?: Project;
    members?: Member[];
}

export const MemberList: React.FC<IMemberListProps> = (props) => {

    const [addMembersPanelOpen, setAddMembersPanelOpen] = useState(false);

    const onRenderMemberColumn = (member?: Member, index?: number, column?: IColumn) => (
        <Persona
            text={member?.graphUser?.displayName ?? member?.user.id}
            showSecondaryText
            secondaryText={member?.graphUser?.mail ?? (member?.graphUser?.otherMails && member.graphUser.otherMails.length > 0 ? member.graphUser.otherMails[0] : undefined)}
            imageUrl={member?.graphUser?.imageUrl}
            size={PersonaSize.size32} />
    );

    const onRenderRoleColumn = (member?: Member, index?: number, column?: IColumn) => props.project ? (member as ProjectMember)?.projectMembership.role : member?.user.role;

    const columns: IColumn[] = [
        { key: 'member', name: 'Member', minWidth: 240, isResizable: false, onRender: onRenderMemberColumn },
        { key: 'role', name: 'Role', minWidth: 240, onRender: onRenderRoleColumn },
        { key: 'type', name: 'Type', minWidth: 240, onRender: (m: Member) => m.user.userType }
    ];

    const _applyFilter = (member: Member, filter: string): boolean =>
        filter ? member.graphUser?.displayName?.toUpperCase().includes(filter.toUpperCase()) ?? false : true;

    const _onItemInvoked = (member: Member): void => {
        console.log(member);
    };

    return (
        <>
            <ContentList
                columns={columns}
                items={props.members}
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
                onFormClose={() => setAddMembersPanelOpen(false)} />

        </>
    );
}
