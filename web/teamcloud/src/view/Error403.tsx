import React from 'react';
import { Stack, Text } from '@fluentui/react';

export interface IError403Props {
    error?: any;
}

export const Error403: React.FunctionComponent<IError403Props> = (props) => {
    // if (props.error) {
    //     console.log(props.error)
    // }
    return (
        <Stack verticalFill verticalAlign='center' horizontalAlign='center'>
            <Text as='h1'>Access Denied</Text>
            <Text as='h2'>{props.error?.name}</Text>
            <Text as='h3'>{props.error?.errorCode}</Text>
            <Text as='h4'>{props.error?.errorMessage}</Text>
        </Stack>
    );
}
