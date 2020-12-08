// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { ListPeoplePicker, IPersonaProps, IBasePickerSuggestionsProps } from '@fluentui/react';
import { searchGraphUsers } from '../MSGraph';
import { GraphUser, Member } from '../model'

export interface IMemberPickerProps {
    members?: Member[];
    formEnabled: boolean;
    onChange: (users?: GraphUser[]) => void;
}

export interface IGraphUserPersonaProps extends IPersonaProps {
    graphUser?: GraphUser;
}

export const MemberPicker: React.FC<IMemberPickerProps> = (props) => {

    const _onResolveSuggestions = async (filter: string, selectedItems?: IGraphUserPersonaProps[], limitResults?: number): Promise<IGraphUserPersonaProps[]> => {
        if (!filter || !filter.length || filter.length === 0) return [];

        console.log(`searchGraphUsers (${filter})`);
        let graphUsers = await searchGraphUsers(filter);

        if (props.members)
            graphUsers = graphUsers.filter(gu => !props.members!.some(m => m.user.id === gu.id));

        let personaProps = graphUsers.map(gu => ({ text: gu.displayName, secondaryText: gu.jobTitle, imageUrl: gu.imageUrl, graphUser: gu } as IGraphUserPersonaProps));

        return personaProps.filter(p => !_containsPersona(p, selectedItems))
    };

    const _containsPersona = (persona: IGraphUserPersonaProps, selectedPersonas?: IGraphUserPersonaProps[]) => {
        if (!selectedPersonas || !selectedPersonas.length || selectedPersonas.length === 0)
            return false;
        return selectedPersonas.filter(item => item.text === persona.text).length > 0;
    };

    const _onItemsChanged = (items?: IGraphUserPersonaProps[]): void => {
        let graphUsers = items?.filter(i => i.graphUser).map(i => i.graphUser as GraphUser);
        props.onChange(graphUsers);
    };

    const _getTextForItem = (persona: IPersonaProps): string => persona.text!;

    const _suggestionProps: IBasePickerSuggestionsProps = {
        noResultsFoundText: 'No results found',
        loadingText: 'Loading...',
        showRemoveButtons: false,
        suggestionsContainerAriaLabel: 'Suggested users'
    };

    return (
        <ListPeoplePicker
            resolveDelay={500}
            onResolveSuggestions={_onResolveSuggestions}
            getTextFromItem={_getTextForItem}
            onChange={_onItemsChanged}
            pickerSuggestionsProps={_suggestionProps}
            disabled={!props.formEnabled} />
    );
}
