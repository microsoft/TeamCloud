// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { ListPeoplePicker, IPersonaProps, IBasePickerSuggestionsProps } from '@fluentui/react';
import { searchGraphUsers } from '../MSGraph';
import { Project, GraphUser } from '../model';

export interface IProjectMemberPickerProps {
    project?: Project;
    formEnabled: boolean;
    onChange: (users?: GraphUser[]) => void;
}

export interface IGraphUserPersonaProps extends IPersonaProps {
    graphUser?: GraphUser;
}

export const ProjectMemberPicker: React.FunctionComponent<IProjectMemberPickerProps> = (props) => {

    const _onResolveSuggestions = async (filter: string, selectedItems?: IGraphUserPersonaProps[], limitResults?: number): Promise<IGraphUserPersonaProps[]> => {
        if (!filter || !filter.length || filter.length === 0) return [];
        let graphUsers = await searchGraphUsers(filter);
        if (props.project?.users) {
            let projectUserIds = props.project.users.map(u => u.id);
            graphUsers = graphUsers.filter(gu => !projectUserIds.find(i => i === gu.id));
        }
        let personaProps = graphUsers.map(gu => ({ text: gu.displayName, secondaryText: gu.jobTitle, imageUrl: gu.imageUrl, graphUser: gu } as IGraphUserPersonaProps));
        return personaProps.filter(p => !_containsPersona(p, selectedItems))
    }

    const _containsPersona = (persona: IGraphUserPersonaProps, selectedPersonas?: IGraphUserPersonaProps[]) => {
        if (!selectedPersonas || !selectedPersonas.length || selectedPersonas.length === 0)
            return false;
        return selectedPersonas.filter(item => item.text === persona.text).length > 0;
    }

    const _onItemsChanged = (items?: IGraphUserPersonaProps[]): void => {
        let graphUsers = items?.filter(i => i.graphUser).map(i => i.graphUser as GraphUser);
        props.onChange(graphUsers);
    }

    const _getTextForItem = (persona: IPersonaProps): string => persona.text!

    const _suggestionProps: IBasePickerSuggestionsProps = {
        noResultsFoundText: 'No results found',
        loadingText: 'Loading...',
        showRemoveButtons: false,
        suggestionsContainerAriaLabel: 'Suggested users'
    }

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
