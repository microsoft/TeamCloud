// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { IPersonaStyleProps, IPersonaStyles, IStyleFunctionOrObject, Persona, PersonaSize } from '@fluentui/react';
import { GraphPrincipal } from '../model/GraphPrincipal';
import { isPrincipalGroup, isPrincipalServicePrincipal, isPrincipalUser } from '../MSGraph';

export interface IUserPersonaProps {
    size?: PersonaSize;
    // user?: GraphUser;
    principal?: GraphPrincipal;
    large?: boolean;
    showSecondaryText?: boolean;
    hidePersonaDetails?: boolean;
    onClick?: () => void;
    styles?: IStyleFunctionOrObject<IPersonaStyleProps, IPersonaStyles>;
}

export const UserPersona: React.FunctionComponent<IUserPersonaProps> = (props) => {

    const text = props.principal?.displayName;

    const secondaryText = 
        isPrincipalUser(props.principal) ? (props.large ? props.principal?.jobTitle : props.principal?.mail ?? (props.principal?.otherMails?.length ?? 0) > 0 ? props.principal!.otherMails![0] : props.principal?.userPrincipalName) :
        isPrincipalGroup(props.principal) ? undefined :
        isPrincipalServicePrincipal(props.principal) ? undefined :
        undefined;

    const tertiaryText = 
        isPrincipalUser(props.principal) ? (props.large ? props.principal?.department : undefined) :
        isPrincipalGroup(props.principal) ? undefined :
        isPrincipalServicePrincipal(props.principal) ? undefined :
        undefined;

    const imageUrl = 
        isPrincipalUser(props.principal) ? props.principal?.imageUrl :
        isPrincipalGroup(props.principal) ? undefined :
        isPrincipalServicePrincipal(props.principal) ? undefined :
        undefined;

    // console.log(JSON.stringify({
    //     text: text,
    //     secondaryText: secondaryText,
    //     tertiaryText: tertiaryText,
    //     imageUrl: imageUrl
    // }));

    return <Persona
            hidePersonaDetails={props.hidePersonaDetails}
            showSecondaryText={props.showSecondaryText}
            text={text}
            secondaryText={secondaryText}
            tertiaryText={tertiaryText}
            imageUrl={imageUrl}
            styles={props.styles}
            onClick={props.onClick}
            size={props.size ? props.size : (props.large ? PersonaSize.size72 : PersonaSize.size32)} />;
}
