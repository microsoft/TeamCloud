// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Text, Stack, StackItem, PrimaryButton } from '@fluentui/react';
import React from 'react';
import { useHistory, useLocation } from 'react-router-dom';
import NotFound from '../img/notfound.png';

export const Error404: React.FC = () => {

    //eslint-disable-next-line
    const projectsExpression = /.*\/projects\/[^\/]*/i;

    //eslint-disable-next-line
    const organizationsExpression = /.*\/orgs\/[^\/]*/i;

    const history = useHistory();
    const location = useLocation();

    const navigateHome = () => {
        [projectsExpression, organizationsExpression].forEach(expression => {
            let match = expression.exec(location.pathname);
            if (match) {
                history.push(match[0]);
                return;
            }
        });
        history.push('');
    }

    return (
        <div style={{ backgroundColor: 'white', height: '100%' }}>
            <Stack horizontal style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
                <StackItem>
                    <img src={NotFound} alt='Not found' style={{ width: 500 }} />
                </StackItem>
                <StackItem >
                    <h1>404 - Page not found</h1>
                    <Text block>Looks like this page doesn’t exist or can’t be found. Make sure the project name and URL are correct.</Text>
                    <PrimaryButton text="Go back home" onClick={() => navigateHome()} style={{ marginTop: 50 }} />
                </StackItem>
            </Stack>
        </div>
    );
}
