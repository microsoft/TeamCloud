import React, { useState, useEffect } from "react";
import { Stack, Label, TextField, Dropdown, IDropdownOption, Spinner } from "@fluentui/react";
import { ProjectType, DataResult } from "../model";
import { getProjectTypes } from "../API";

export interface IProjectFormProps {
    // onProjectSelected?: (project: Project) => void;
}

export const ProjectForm: React.FunctionComponent<IProjectFormProps> = (props) => {

    const [name, setName] = useState<string>();
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
    }, []);


    const handleSubmit = () => {

    }

    if (projectTypes) {

        const projectTypeOptions: IDropdownOption[] = projectTypes.map(pt => {
            return { key: pt.id, text: pt.id } as IDropdownOption
        });

        return (
            <Stack>
                <form onSubmit={handleSubmit}>
                    <TextField label='Name' onChange={(ev, val) => setName(val)} />
                    <Dropdown
                        label='Project Type'
                        placeHolder='Select a Project Type'
                        options={projectTypeOptions}></Dropdown>
                </form>
            </Stack>
        );
    } else {
        return (<Stack verticalFill verticalAlign='center' horizontalAlign='center'><Spinner /></Stack>)
    }
}
