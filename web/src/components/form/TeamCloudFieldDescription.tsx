// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from "react";

import "./TeamCloudFieldDescription.css"

export interface TeamCloudFieldDescriptionProps {
	description: string
}

export const TeamCloudFieldDescription: React.FC<TeamCloudFieldDescriptionProps> = (props) => {

  return (	  
      props && props.description 
        ? <span><span className="ms-TextField-description teamCloudSelectWidgetDescription">{props.description}</span></span> 
        : <></>
  );
};
