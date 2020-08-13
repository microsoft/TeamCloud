import React from 'react';
import { User, ProjectMembership } from '../model';
import { Stack, Facepile, IFacepilePersona, PersonaSize, IRenderFunction, HoverCard, HoverCardType, Persona, Shimmer, ShimmerElementsGroup, ShimmerElementType } from '@fluentui/react';
import { GraphUser } from '../MSGraph';
import { ProjectDetailCard } from './ProjectDetailCard';

export interface Member {
    user: User;
    graphUser?: GraphUser;
    projectMembership?: ProjectMembership;
}

export interface IProjectLinksProps {
    members?: Member[];
}

export const ProjectLinks: React.FunctionComponent<IProjectLinksProps> = (props) => {

    const facepilePersonas = (): IFacepilePersona[] => props.members?.map(m => ({
        personaName: m.graphUser?.displayName,
        imageUrl: m.graphUser?.imageUrl,
        data: m,
    })) ?? [];

    const _onRenderPersonaCoin: IRenderFunction<IFacepilePersona> = (props?: IFacepilePersona, defaultRender?: (props?: IFacepilePersona) => JSX.Element | null): JSX.Element | null => {
        if (defaultRender) {
            let element = defaultRender(props);
            let _onRenderPlainCard = (): JSX.Element | null => {
                if (!props?.data) return null;
                let member: Member = props.data
                console.log(member);
                return (
                    <Stack
                        tokens={{ padding: 20 }}>
                        <Persona
                            text={member.graphUser?.displayName ?? member.user.id}
                            secondaryText={`Role: ${member.projectMembership?.role}`}
                            tertiaryText={`Type: ${member.user.userType}`}
                            imageUrl={member.graphUser?.imageUrl}
                            size={PersonaSize.size72}
                        // styles={personaStyles}
                        // onClick={() => setPanelOpen(true)}
                        />
                    </Stack>
                );
            };

            return (
                <HoverCard
                    instantOpenOnClick
                    type={HoverCardType.plain}
                    plainCardProps={{ onRenderPlainCard: _onRenderPlainCard }}>
                    {element}
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

    return (
        <ProjectDetailCard title='Members'>
            <Shimmer
                customElementsGroup={_getShimmerElements()}
                isDataLoaded={props.members ? props.members.length > 0 : false}
                width={152} >
                <Facepile
                    personas={facepilePersonas()}
                    personaSize={PersonaSize.size48}
                    onRenderPersonaCoin={_onRenderPersonaCoin}
                ></Facepile>
            </Shimmer>
        </ProjectDetailCard>
    );
}
