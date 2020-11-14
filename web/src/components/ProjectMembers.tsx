// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { ProjectMember } from '../model'
import { Project, ErrorResult, User } from 'teamcloud';
import { Stack, Facepile, IFacepilePersona, PersonaSize, IRenderFunction, HoverCard, HoverCardType, Persona, Shimmer, ShimmerElementsGroup, ShimmerElementType, CommandBar, ICommandBarItemProps, Separator, Label, Text } from '@fluentui/react';
import { getGraphUser, getGraphDirectoryObject } from '../MSGraph';
import { ProjectDetailCard, ProjectMembersForm } from '.';
import AppInsights from '../img/appinsights.svg';
import DevOps from '../img/devops.svg';
import DevTestLabs from '../img/devtestlabs.svg';
import GitHub from '../img/github.svg';
import { api } from '../API';


export interface IProjectMembersProps {
    user?: User;
    project: Project;
    onEditMember: (member?: ProjectMember) => void;
}

export const ProjectMembers: React.FunctionComponent<IProjectMembersProps> = (props) => {
    // return (<></>);

    const [members, setMembers] = useState<ProjectMember[]>();
    const [addMembersPanelOpen, setAddMembersPanelOpen] = useState(false);

    useEffect(() => {
        if (props.project) {
            const _setMembers = async () => {
                let _users = await api.getProjectUsers(props.project.organization, props.project.id);
                if (_users.data) {
                    console.warn('foo')
                    let _members = await Promise.all(_users.data.map(async u => ({
                        user: u,
                        graphUser: u.userType === 'User' ? await getGraphUser(u.id) : u.userType === 'Provider' ? await getGraphDirectoryObject(u.id) : undefined,
                        projectMembership: u.projectMemberships!.find(m => m.projectId === props.project.id)!
                    })));
                    setMembers(_members);
                }
            };
            _setMembers();
        }
    }, [props.project]);

    const _removeMemberFromProject = async (member: ProjectMember) => {
        let result = await api.deleteProjectUser(member.user.id, props.project.organization, props.project.id);
        if (result.code !== 202 && (result as ErrorResult).errors) {
            console.log(result as ErrorResult);
        }
    };

    const _findKnownProviderImage = (member: ProjectMember) => {
        if (member.graphUser?.displayName) {
            if (member.graphUser.displayName.startsWith('appinsights')) return AppInsights
            if (member.graphUser.displayName.startsWith('devops')) return DevOps
            if (member.graphUser.displayName.startsWith('devtestlabs')) return DevTestLabs
            if (member.graphUser.displayName.startsWith('github')) return GitHub
        }
        return undefined;
    };

    const _removeButtonDisabled = (member: ProjectMember) => {
        return members && member.projectMembership.role === 'Owner'
            && members.filter(m => m.user.userType === 'User'
                && m.user.projectMemberships
                && m.user.projectMemberships!.find(pm => pm.projectId === props.project.id && pm.role === 'Owner')).length === 1
    };

    const _userIsProjectOwner = () =>
        props.user?.projectMemberships?.find(m => m.projectId === props.project.id)?.role === 'Owner';

    const _getCommandBarItems = (): ICommandBarItemProps[] => [
        { key: 'addUser', text: 'Add', iconProps: { iconName: 'PeopleAdd' }, onClick: () => { setAddMembersPanelOpen(true) }, disabled: !_userIsProjectOwner() },
    ];

    const _getMemberCommandBarItems = (member: ProjectMember): ICommandBarItemProps[] => [
        { key: 'edit', text: 'Edit', iconProps: { iconName: 'EditContact' }, onClick: () => props.onEditMember(member) },
        { key: 'remove', text: 'Remove', iconProps: { iconName: 'UserRemove' }, disabled: _removeButtonDisabled(member), onClick: () => { _removeMemberFromProject(member) } },
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

    const _facepilePersonas = (): IFacepilePersona[] => members?.map(m => ({
        personaName: m.graphUser?.displayName,
        imageUrl: m.graphUser?.imageUrl ?? _findKnownProviderImage(m),
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
            <ProjectDetailCard
                title='Members'
                callout={members?.length.toString()}
                commandBarItems={_getCommandBarItems()}>
                <Shimmer
                    customElementsGroup={_getShimmerElements()}
                    isDataLoaded={members !== undefined}
                    width={152} >
                    <Facepile
                        styles={{ itemButton: _personaCoinStyles }}
                        personas={_facepilePersonas()}
                        personaSize={PersonaSize.size48}
                        maxDisplayablePersonas={20}
                        onRenderPersonaCoin={_onRenderPersonaCoin} />
                </Shimmer>
            </ProjectDetailCard>
            <ProjectMembersForm
                project={props.project}
                panelIsOpen={addMembersPanelOpen}
                onFormClose={() => setAddMembersPanelOpen(false)} />
        </>
    );
}
