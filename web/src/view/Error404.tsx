// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useLocation } from 'react-router-dom';

export interface IError404Props {

}

export const Error404: React.FunctionComponent<IError404Props> = (props) => {

    let location = useLocation();

    return (
        <div>
            <h3>
                No match for <code>{location.pathname}</code>
            </h3>
        </div>
    );
}
