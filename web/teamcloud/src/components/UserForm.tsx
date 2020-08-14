import React from "react";
import { Stack, Dropdown, IDropdownOption, ListPeoplePicker, IPersonaProps, Label, IBasePickerSuggestionsProps } from "@fluentui/react";
import { ProjectUserRole } from "../model";
import { searchGraphUsers, GraphUser } from "../MSGraph";

export interface IUserFormProps {
    fieldsEnabled: boolean;
    onFormSubmit: () => void;
    onUserIdentifiersChange: (val?: string[]) => void;
    onUserRoleChange: (val?: ProjectUserRole) => void;
}

export interface IGraphUserPersonaProps extends IPersonaProps {
    graphUser?: GraphUser;
}

export const UserForm: React.FunctionComponent<IUserFormProps> = (props) => {

    const _projectRoleOptions = (): IDropdownOption[] => {
        return [ProjectUserRole.Member, ProjectUserRole.Owner].map(r => ({ key: r, text: r } as IDropdownOption));
    };

    const _onUserRoleDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        if (!option)
            props.onUserRoleChange(undefined);
        else
            props.onUserRoleChange(option.key as ProjectUserRole)
    };

    const _onResolveSuggestions = async (filter: string, selectedItems?: IGraphUserPersonaProps[], limitResults?: number): Promise<IGraphUserPersonaProps[]> => {
        if (!filter || !filter.length || filter.length === 0) return [];
        let graphUsers = await searchGraphUsers(filter);
        let personaProps = graphUsers.map(gu => ({ text: gu.displayName, secondaryText: gu.jobTitle, imageUrl: gu.imageUrl, graphUser: gu } as IGraphUserPersonaProps));
        return personaProps.filter(p => !_containsPersona(p, selectedItems))
    }

    const _containsPersona = (persona: IGraphUserPersonaProps, selectedPersonas?: IGraphUserPersonaProps[]) => {
        if (!selectedPersonas || !selectedPersonas.length || selectedPersonas.length === 0)
            return false;
        return selectedPersonas.filter(item => item.text === persona.text).length > 0;
    }

    const _onItemsChanged = (items?: IGraphUserPersonaProps[] | undefined): void => {
        let graphUsers = items?.filter(i => i.graphUser).map(i => i.graphUser as GraphUser);
        // setSelectedUsers(graphUsers);
        props.onUserIdentifiersChange(graphUsers?.map(u => u.id));
    }

    const _getTextForItem = (persona: IPersonaProps): string => persona.text!

    const _suggestionProps: IBasePickerSuggestionsProps = {
        noResultsFoundText: 'No results found',
        loadingText: 'Loading...',
        showRemoveButtons: false,
        suggestionsContainerAriaLabel: 'Suggested users'
    }

    return (
        <Stack>
            <Dropdown
                required
                label='Role'
                placeHolder='Select a Role'
                disabled={props.fieldsEnabled}
                options={_projectRoleOptions()}
                onChange={_onUserRoleDropdownChange} />
            <Label required>Users</Label>
            <ListPeoplePicker
                resolveDelay={500}
                onResolveSuggestions={_onResolveSuggestions}
                getTextFromItem={_getTextForItem}
                onChange={_onItemsChanged}
                pickerSuggestionsProps={_suggestionProps}
                disabled={props.fieldsEnabled} />
        </Stack>
    );
}

