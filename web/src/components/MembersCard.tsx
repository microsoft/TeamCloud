// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Member, ProjectMember } from '../model'
import { Project, ErrorResult, User } from 'teamcloud';
import { Stack, Facepile, IFacepilePersona, PersonaSize, IRenderFunction, HoverCard, HoverCardType, Persona, Shimmer, ShimmerElementsGroup, ShimmerElementType, CommandBar, ICommandBarItemProps, Separator, Label, Text } from '@fluentui/react';
import { DetailCard, MembersForm } from '.';
import { api } from '../API';
import { useParams } from 'react-router-dom';


export interface IMembersCardProps {
    user?: User;
    project?: Project;
    members?: Member[];
    onEditMember: (member?: Member) => void;
}

export const MembersCard: React.FC<IMembersCardProps> = (props) => {

    const { orgId } = useParams() as { orgId: string };

    const [addMembersPanelOpen, setAddMembersPanelOpen] = useState(false);

    const _removeMember = async (member: Member) => {
        if (props.project && (member as ProjectMember)?.projectMembership !== undefined) {
            const result = await api.deleteProjectUser(member.user.id, props.project.organization, props.project.id);
            if (result.code !== 202 && (result as ErrorResult).errors) {
                console.log(result as ErrorResult);
            }
        } else if (orgId) {
            const result = await api.deleteOrganizationUser(member.user.id, orgId);
            if (result.code !== 202 && (result as ErrorResult).errors) {
                console.log(result as ErrorResult);
            }
        }
    };

    const _removeButtonDisabled = (member: ProjectMember) => {
        return props.project && props.members && member.projectMembership.role === 'Owner'
            && props.members.filter(m => m.user.userType === 'User'
                && m.user.projectMemberships
                && m.user.projectMemberships!.find(pm => pm.projectId === props.project!.id && pm.role === 'Owner')).length === 1
    };

    const _userIsProjectOwner = () =>
        props.project && props.user?.projectMemberships?.find(m => m.projectId === props.project!.id)?.role === 'Owner';

    const _getCommandBarItems = (): ICommandBarItemProps[] => [
        { key: 'addUser', text: 'Add', iconProps: { iconName: 'PeopleAdd' }, onClick: () => setAddMembersPanelOpen(true), disabled: !_userIsProjectOwner() },
    ];

    const _getMemberCommandBarItems = (member: ProjectMember): ICommandBarItemProps[] => [
        { key: 'edit', text: 'Edit', iconProps: { iconName: 'EditContact' }, onClick: () => props.onEditMember(member) },
        { key: 'remove', text: 'Remove', iconProps: { iconName: 'UserRemove' }, disabled: _removeButtonDisabled(member), onClick: () => { _removeMember(member) } },
    ];

    const _getShimmerElements = (): JSX.Element => (
        <ShimmerElementsGroup
            shimmerElements={[
                { type: ShimmerElementType.circle, height: 48 },
                { type: ShimmerElementType.gap, width: 4 },
                { type: ShimmerElementType.circle, height: 48 },
                { type: ShimmerElementType.gap, width: 4 },
                { type: ShimmerElementType.circle, height: 48 }
            ]} />
    );

    const _facepilePersonas = (): IFacepilePersona[] => props.members?.map(m => ({
        personaName: m.graphUser?.displayName,
        imageUrl: m.graphUser?.imageUrl,
        data: m,
    })) ?? [];

    const _onRenderPersonaCoin: IRenderFunction<IFacepilePersona> = (props?: IFacepilePersona, defaultRender?: (props?: IFacepilePersona) => JSX.Element | null): JSX.Element | null => {
        if (defaultRender && props?.data) {
            let _onRenderPlainCard = (): JSX.Element | null => {
                let member: ProjectMember = props.data;
                let _isUserType = member.user.userType === 'User';
                let _getCommandBar = _isUserType ?
                    (<>
                        <Stack.Item>
                            <Separator />
                        </Stack.Item>
                        <Stack.Item align='end'>
                            <CommandBar
                                styles={{ root: { minWidth: '160px' } }}
                                items={_getMemberCommandBarItems(member)}
                                ariaLabel='Use left and right arrow keys to navigate between commands' />
                        </Stack.Item>
                    </>) : null;
                return (
                    <Stack
                        tokens={{ padding: _isUserType ? '20px 20px 0 20px' : '20px' }}>
                        <Stack.Item>
                            <Persona
                                text={member.graphUser?.displayName ?? member.user.id}
                                secondaryText={member.graphUser?.jobTitle ?? member.user.userType}
                                tertiaryText={member.graphUser?.department}
                                imageUrl={member.graphUser?.imageUrl}
                                size={PersonaSize.size72} />
                        </Stack.Item>
                        <Stack.Item>
                            <Separator />
                        </Stack.Item>
                        <Stack.Item>
                            <Stack tokens={{ childrenGap: 0 }}>
                                <Stack horizontal verticalAlign='baseline' tokens={{ childrenGap: 6 }}>
                                    <Label>Type:</Label>
                                    <Text>{member.user.userType}</Text>
                                </Stack>
                                <Stack horizontal verticalAlign='baseline' tokens={{ childrenGap: 6 }}>
                                    <Label>Role:</Label>
                                    <Text>{member.projectMembership?.role ?? 'Unknown'}</Text>
                                </Stack>
                            </Stack>
                        </Stack.Item>
                        {_getCommandBar}
                    </Stack>
                );
            };

            return (
                <HoverCard
                    instantOpenOnClick
                    type={HoverCardType.plain}
                    cardOpenDelay={1000}
                    plainCardProps={{ onRenderPlainCard: _onRenderPlainCard }}>
                    {defaultRender(props)}
                </HoverCard>
            );
        }

        return null;
    };

    const _personaCoinStyles = {
        cursor: 'pointer',
        selectors: {
            ':hover': {
                cursor: 'pointer'
            }
        }
    }

    return (
        <>
            <DetailCard
                title='Members'
                callout={props.members?.length.toString()}
                commandBarItems={_getCommandBarItems()}>
                <Shimmer
                    customElementsGroup={_getShimmerElements()}
                    isDataLoaded={props.members !== undefined}
                    width={152} >
                    <Facepile
                        styles={{ itemButton: _personaCoinStyles }}
                        personas={_facepilePersonas()}
                        personaSize={PersonaSize.size48}
                        maxDisplayablePersonas={20}
                        onRenderPersonaCoin={_onRenderPersonaCoin} />
                </Shimmer>
            </DetailCard>
            <MembersForm
                members={props.members}
                panelIsOpen={addMembersPanelOpen}
                onFormClose={() => setAddMembersPanelOpen(false)} />
        </>
    );
}
