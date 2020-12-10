// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, useState } from 'react';
import { ICommandBarItemProps } from '@fluentui/react';
import { ErrorResult, User, UserDefinition } from 'teamcloud';
import { api } from '../API';
import { GraphUserContext, ProjectContext } from '../Context';
import { Member, ProjectMember } from '../model'
import { DetailCard, MembersForm, MemberFacepile } from '.';


export interface IMembersCardProps {
    // onEditMember: (member?: Member) => void;
    members?: Member[];
    onAddUsers: (user: UserDefinition[]) => Promise<void>;
    onRemoveUsers: (user: User[]) => Promise<void>;
}

export const MembersCard: React.FC<IMembersCardProps> = (props) => {

    const [addMembersPanelOpen, setAddMembersPanelOpen] = useState(false);

    const { graphUser } = useContext(GraphUserContext);

    const { members, onAddUsers } = useContext(ProjectContext);

    const _removeMember = async (member: Member) => {
        const projectId = (member as ProjectMember)?.projectMembership?.projectId;
        if (projectId) {
            const result = await api.deleteProjectUser(member.user.id, member.user.organization, projectId);
            if (result.code !== 204 && (result as ErrorResult).errors) {
                console.error(result as ErrorResult);
            }
        } else {
            console.log('Deleting a user from a Org is not implemented yet.')
        }
    };

    const _isAdmin = () => {
        const member = graphUser && members?.find(m => m.user.id === graphUser.id);
        if (!member) return false;
        const role = (member as ProjectMember)?.projectMembership?.role ?? member.user.role;
        return role.toLowerCase() === 'owner' || role.toLowerCase() === 'admim';
    };

    const _getCommandBarItems = (): ICommandBarItemProps[] => [
        { key: 'addUser', text: 'Add', iconProps: { iconName: 'PeopleAdd' }, onClick: () => setAddMembersPanelOpen(true), disabled: !_isAdmin() },
    ];

    return (
        <>
            <DetailCard
                title='Members'
                callout={members?.length}
                commandBarItems={_getCommandBarItems()}>
                <MemberFacepile
                    members={members}
                    onRemoveMember={_removeMember} />
            </DetailCard>
            <MembersForm
                members={members}
                panelIsOpen={addMembersPanelOpen}
                onFormClose={() => setAddMembersPanelOpen(false)}
                onAddUsers={onAddUsers} />
        </>
    );
}
