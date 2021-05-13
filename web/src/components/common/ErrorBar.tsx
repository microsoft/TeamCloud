// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { MessageBar, MessageBarType, Stack, Text } from '@fluentui/react';
import { ErrorResult } from 'teamcloud';

export interface IErrorBarProps {
    stackItem?: boolean;
    error?: any;
}

export const ErrorBar: React.FC<IErrorBarProps> = (props) => {

    const { error, stackItem } = props

    const isErrorResult = error && ('code' in error) && ('status' in error) && ('errors' in error)

    const errorTitle = isErrorResult ? `Error: ${error?.status} (${error?.code})` : 'Error';

    const errorMessages = () => {

        if (!isErrorResult)
            return (<Text>{JSON.stringify(error)}</Text>)

        const elements = [];

        const errors = (error as ErrorResult).errors;

        if (errors) {

            for (let i = 0; i < errors.length; i++) {
                const e = errors[i];

                elements.push(<br />);
                elements.push(<Text>- {e.message}</Text>);

                const suberrors = e.errors;
                if (suberrors) {

                    for (let ii = 0; ii < suberrors.length; ii++) {
                        const se = suberrors[ii];
                        elements.push(<br />);
                        elements.push(<Text>-- {se.message}</Text>);
                    }
                }
            }
        } else {
            elements.push(<></>);
        }

        return elements;
    }

    const getMessageBar = () => {
        return (<MessageBar
            messageBarType={MessageBarType.error}>
            <Text>{errorTitle}</Text>
            {errorMessages()}
        </MessageBar>);
    }

    return error ? stackItem ? (<Stack.Item>{getMessageBar()}</Stack.Item>) : getMessageBar() : <></>;
}
