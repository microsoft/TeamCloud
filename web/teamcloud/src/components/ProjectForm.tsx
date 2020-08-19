// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from "react";
import { Stack, TextField, Dropdown, IDropdownOption, Spinner } from "@fluentui/react";
import { ProjectType, DataResult } from "../model";
import { getProjectTypes } from "../API";

export interface IProjectFormProps {
    fieldsEnabled: boolean;
    onFormSubmit: () => void;
    onNameChange: (val: string | undefined) => void;
    onProjectTypeChange: (val: ProjectType | undefined) => void;
}

export const ProjectForm: React.FunctionComponent<IProjectFormProps> = (props) => {

    const [projectTypes, setProjectTypes] = useState<ProjectType[]>();

    useEffect(() => {
        if (projectTypes === undefined) {
            const _setProjectTypes = async () => {
                const result = await getProjectTypes()
                const data = (result as DataResult<ProjectType[]>).data;
                setProjectTypes(data);
            };
            _setProjectTypes();
        }
    }, [projectTypes]);

    const projectTypeOptions = (): IDropdownOption[] => {
        if (!projectTypes) return [];
        return projectTypes.map(pt => ({ key: pt.id, text: pt.id } as IDropdownOption));
    };

    const onDropdownChange = (event: React.FormEvent<HTMLDivElement>, option?: IDropdownOption, index?: number): void => {
        if (!projectTypes || !option)
            props.onProjectTypeChange(undefined);
        else
            props.onProjectTypeChange(projectTypes.find(pt => pt.id === option.key))
    };

    if (projectTypes) {
        return (
            <Stack>
                <TextField
                    label='Name'
                    required
                    // errorMessage='Name is required.'
                    disabled={props.fieldsEnabled}
                    onChange={(ev, val) => props.onNameChange(val)} />
                <Dropdown
                    label='Project Type'
                    required
                    // errorMessage='Project Type is required.'
                    placeHolder='Select a Project Type'
                    disabled={props.fieldsEnabled}
                    options={projectTypeOptions()}
                    onChange={onDropdownChange} />
            </Stack>
        );
    } else {
        return (<Stack verticalFill verticalAlign='center' horizontalAlign='center'><Spinner /></Stack>)
    }
}
