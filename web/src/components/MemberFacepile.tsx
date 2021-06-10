// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { CommandBar, Facepile, HoverCard, HoverCardType, ICommandBarItemProps, IFacepilePersona, IRenderFunction, PersonaSize, Separator, Shimmer, ShimmerElementsGroup, ShimmerElementType, Stack, Text } from '@fluentui/react';
import React from 'react';
import { UserPersona } from '.';
import { useGraphUser } from '../hooks';
import { Member, ProjectMember } from '../model';
import { isPrincipalUser } from '../MSGraph';

export interface IMemberFacepileProps {
    members?: Member[];
    onRemoveMember: (member: Member) => void;
}

export const MemberFacepile: React.FunctionComponent<IMemberFacepileProps> = (props) => {

    const { data: graphUser } = useGraphUser();

    const _isOwner = (member: ProjectMember) => {
        const role = (member as ProjectMember)?.projectMembership?.role ?? member.user.role;
        return role.toLowerCase() === 'owner';
    };

    const _isAdmin = () => {
        const member = graphUser && props.members?.find(m => m.user.id === graphUser.id);
        if (!member) return false;
        const role = (member as ProjectMember)?.projectMembership?.role ?? member.user.role;
        return role.toLowerCase() === 'owner' || role.toLowerCase() === 'admim';
    };


    const _getMemberCommandBarItems = (member: ProjectMember): ICommandBarItemProps[] => [
        // { key: 'edit', text: 'Edit', iconProps: { iconName: 'EditContact' }, onClick: () => props.onEditMember(member) },
        { key: 'edit', text: 'Edit', iconProps: { iconName: 'EditContact' }, onClick: () => { } },
        { key: 'remove', text: 'Remove', iconProps: { iconName: 'UserRemove' }, disabled: !_isAdmin() || _isOwner(member), onClick: () => props.onRemoveMember(member) },
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

    const _facepilePersonas = (): IFacepilePersona[] => props.members?.filter(m => m.graphPrincipal)?.map(m => ({
        personaName: m.graphPrincipal?.displayName,
        imageUrl: isPrincipalUser(m.graphPrincipal) ? m.graphPrincipal?.imageUrl : undefined,
        data: m,
    })) ?? [];

    const _onRenderPersonaCoin: IRenderFunction<IFacepilePersona> = (props?: IFacepilePersona, defaultRender?: (props?: IFacepilePersona) => JSX.Element | null): JSX.Element | null => {
        if (defaultRender && props?.data) {
            let _onRenderPlainCard = (): JSX.Element | null => {
                let member: ProjectMember = props.data;
                return (
                    <Stack
                        tokens={{ padding: '20px 20px 0 20px' }}>
                        <Stack.Item>
                            <UserPersona principal={member.graphPrincipal} large />
                        </Stack.Item>
                        <Stack.Item>
                            <Separator />
                        </Stack.Item>
                        <Stack.Item>
                            <Stack horizontal horizontalAlign='space-between' verticalAlign='center'>
                                <Stack.Item>
                                    <Text>{member.projectMembership?.role ?? 'Unknown'}</Text>
                                </Stack.Item>
                                <Stack.Item>
                                    <CommandBar
                                        styles={{ root: { minWidth: '160px' } }}
                                        items={_getMemberCommandBarItems(member)}
                                        ariaLabel='Use left and right arrow keys to navigate between commands' />
                                </Stack.Item>
                            </Stack>
                        </Stack.Item>
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
        <Shimmer
            customElementsGroup={_getShimmerElements()}
            isDataLoaded={props.members?.filter(m => m.graphPrincipal) !== undefined}
            width={152} >
            <Facepile
                styles={{ itemButton: _personaCoinStyles }}
                personas={_facepilePersonas()}
                personaSize={PersonaSize.size48}
                maxDisplayablePersonas={20}
                onRenderPersonaCoin={_onRenderPersonaCoin} />
        </Shimmer>

    );
}
