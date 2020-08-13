import React from 'react';
import { User, ProjectMembership, UserType } from '../model';
import { Stack, Facepile, IFacepilePersona, PersonaSize, IRenderFunction, HoverCard, HoverCardType, Persona, Shimmer, ShimmerElementsGroup, ShimmerElementType, CommandBar, ICommandBarItemProps, Separator, Label, Text } from '@fluentui/react';
import { GraphUser } from '../MSGraph';
import { ProjectDetailCard } from './ProjectDetailCard';
import AppInsights from '../img/appinsights.svg';
import DevOps from '../img/devops.svg';
import DevTestLabs from '../img/devtestlabs.svg';
import GitHub from '../img/github.svg';



export interface Member {
    user: User;
    graphUser?: GraphUser;
    projectMembership?: ProjectMembership;
}

export interface IProjectMembersProps {
    members?: Member[];
    removeMember: (member: Member) => void;
}

export const ProjectMembers: React.FunctionComponent<IProjectMembersProps> = (props) => {

    const _findKnownProviderImage = (member: Member) => {
        if (member.graphUser?.displayName) {
            if (member.graphUser.displayName.startsWith('appinsights'))
                return AppInsights
            if (member.graphUser.displayName.startsWith('devops'))
                return DevOps
            if (member.graphUser.displayName.startsWith('devtestlabs'))
                return DevTestLabs
            if (member.graphUser.displayName.startsWith('github'))
                return GitHub
        }
        return undefined;
    }

    const _facepilePersonas = (): IFacepilePersona[] => props.members?.map(m => ({
        personaName: m.graphUser?.displayName,
        imageUrl: m.graphUser?.imageUrl ?? _findKnownProviderImage(m),
        data: m,
    })) ?? [];

    const _getCommandBarItems = (member: Member): ICommandBarItemProps[] => [
        { key: 'edit', text: 'Edit', iconProps: { iconName: 'EditContact' } },
        { key: 'remove', text: 'Remove', iconProps: { iconName: 'UserRemove' }, onClick: () => { props.removeMember(member) } },
    ];

    const _onRenderPersonaCoin: IRenderFunction<IFacepilePersona> = (props?: IFacepilePersona, defaultRender?: (props?: IFacepilePersona) => JSX.Element | null): JSX.Element | null => {
        if (defaultRender && props?.data) {
            let _onRenderPlainCard = (): JSX.Element | null => {
                let member: Member = props.data;
                let _isUserType = member.user.userType === UserType.User;
                let _getCommandBar = _isUserType ?
                    (<>
                        <Stack.Item>
                            <Separator />
                        </Stack.Item>
                        <Stack.Item align='end'>
                            <CommandBar
                                styles={{ root: { minWidth: '160px' } }}
                                items={_getCommandBarItems(member)} />
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

    const _personaCoinStyles = {
        cursor: 'pointer',
        selectors: {
            ':hover': {
                cursor: 'pointer'
            }
        }
    }

    return (
        <ProjectDetailCard title='Members'>
            <Shimmer
                customElementsGroup={_getShimmerElements()}
                isDataLoaded={props.members ? props.members.length > 0 : false}
                width={152} >
                <Facepile
                    styles={{ itemButton: _personaCoinStyles }}
                    personas={_facepilePersonas()}
                    personaSize={PersonaSize.size48}
                    onRenderPersonaCoin={_onRenderPersonaCoin} />
            </Shimmer>
        </ProjectDetailCard>
    );
}
