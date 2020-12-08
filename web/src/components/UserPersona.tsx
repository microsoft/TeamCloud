// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { MouseEventHandler } from 'react';
import { IPersonaStyleProps, IPersonaStyles, IStyleFunctionOrObject, Persona, PersonaSize } from '@fluentui/react';
import { GraphUser, Member } from '../model';

export interface IUserPersonaProps {
    size?: PersonaSize;
    user?: GraphUser;
    large?: boolean;
    showSecondaryText?: boolean;
    hidePersonaDetails?: boolean;
    onClick?: () => void;
    styles?: IStyleFunctionOrObject<IPersonaStyleProps, IPersonaStyles>;
}

export const UserPersona: React.FunctionComponent<IUserPersonaProps> = (props) => {

    const text = props.user?.displayName;

    const mail = props.user?.mail
        ?? ((props.user?.otherMails?.length ?? 0 > 0) ? props.user!.otherMails![0] : props.user?.userPrincipalName);

    const secondaryText = props.large ? mail : undefined;

    const tertiaryText = props.large ? props.user?.department : undefined;
    // const tertiaryText = props.user?.department;

    const imageUrl = props.user?.imageUrl;

    return (
        <Persona
            hidePersonaDetails={props.hidePersonaDetails}
            showSecondaryText={props.showSecondaryText}
            text={text}
            secondaryText={secondaryText}
            tertiaryText={tertiaryText}
            imageUrl={imageUrl}
            styles={props.styles}
            size={props.size ?? props.large ? PersonaSize.size72 : PersonaSize.size32} />
    );
}
