import { IComboBoxOption } from "@fluentui/react";
import { WidgetProps } from "@rjsf/core";
import { ComboBox } from "office-ui-fabric-react";
import React, { useEffect, useState } from "react";
import { useAzureManagementGroups } from "../../hooks";

export const ManagementGroupWidget: React.FC<WidgetProps>  = (props) => {

    const { data: managementGroups } = useAzureManagementGroups();

	const [managementGroupSelected, setManagementGroup] = useState<string>();
    const [managementGroupOptions, setManagementGroupOptions] = useState<IComboBoxOption[]>();

	useEffect(() => {
        if (managementGroups && !managementGroupOptions) {
            console.log('ManagementGroupWidget: + managementGroupOptions')
            setManagementGroupOptions(managementGroups?.map(s => ({ key: s.id, text: s.properties.displayName })));
        }
    }, [managementGroups, managementGroupOptions]);

	useEffect(() => {
		props.onChange(managementGroupSelected);
	}, [props, managementGroupSelected]);

	return (
		<ComboBox
			// {...props}
			label='Management Group'
			disabled={!managementGroupOptions || managementGroupOptions.length === 0}
			selectedKey={managementGroupSelected ?? undefined}
			options={managementGroupOptions ?? []}
			onChange={(_ev, val) => setManagementGroup(val ? val.key as string : undefined)} />
	);
}

