// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { FontIcon } from "@fluentui/react";
import React from "react";
import { Component } from "teamcloud";

export interface IComponentIconProps {
    component?: Component;
}

export const ComponentIcon: React.FunctionComponent<IComponentIconProps> = (props) => {

	const { component } = props;

	const _getTypeIcon = (component?: Component) => {
        if (component?.type) {
			switch (component.type.toLowerCase()) { 
				case 'environment': return 'AzureLogo';
				case 'repository': return 'OpenSource';
	        }
        	console.log(`Icon for component type '${component?.type}' not found`);
		}
        return undefined;
    };

	return component?.type 
		? ( <FontIcon iconName={_getTypeIcon(component)} className='component-type-icon' /> )
		: ( <></> );
}