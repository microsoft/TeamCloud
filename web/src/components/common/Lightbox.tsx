// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from "react";
import { getTheme, IconButton, IFontStyles, Modal, Stack, Text } from "@fluentui/react";

export interface ILightboxProps {
	title?: string;
	titleSize?: keyof IFontStyles;
	isBlocking?: boolean;
	isOpen?: boolean;
	onDismiss?: (ev?: React.MouseEvent<HTMLButtonElement | HTMLElement>) => any;
	onDismissed?: () => any;
	onRenderHeader?: () => JSX.Element;
	onRenderFooter?: () => JSX.Element;
}

export const Lightbox: React.FC<ILightboxProps> = (props) => {

	const theme = getTheme();

	const _renderHeader = (): JSX.Element => {
		if (props.onRenderHeader)
			return props.onRenderHeader();
		return (<><Text variant={props.titleSize}>{props.title}</Text></>);
	};

	const _renderFooter = (): JSX.Element => {
		if (props.onRenderFooter)
			return (<Stack.Item>
				<Stack horizontal horizontalAlign='end'
					tokens={{ childrenGap: '10px' }}
					style={{ paddingTop: '32px', borderTop: '1px lightgray solid', position: 'absolute', left: '50px', bottom: '50px', width: 'calc(100% - 100px)' }}>
					{props.onRenderFooter()}
				</Stack>
			</Stack.Item>);
		return (<></>);
	};

	return (

		<Modal
			theme={theme}
			styles={{ main: { margin: 'auto 100px', minHeight: 'calc(100% - 32px)', minWidth: 'calc(100% - 32px)' }, scrollableContent: { padding: '50px' } }}
			isBlocking={props.isBlocking ?? false}
			isOpen={props.isOpen ?? false}
			onDismiss={props.onDismiss}
			onDismissed={props.onDismissed}>

			<Stack tokens={{ childrenGap: '12px' }} style={{ height: 'calc(100vh - 132px)' }}>
				<Stack.Item>
					<Stack horizontal horizontalAlign='space-between'
						tokens={{ childrenGap: '50px' }}
						style={{ paddingBottom: '32px', borderBottom: '1px lightgray solid' }}>
						<Stack.Item>
							{_renderHeader()}
						</Stack.Item>
						<Stack.Item>
							<IconButton
								hidden={!props.onDismiss}
								iconProps={{ iconName: 'ChromeClose' }}
								onClick={() => { if (props.onDismiss) props.onDismiss() }} />
						</Stack.Item>
					</Stack>
				</Stack.Item>
				<Stack.Item >
					{props.children}
				</Stack.Item>
				{_renderFooter()}
			</Stack>

		</Modal>
	);

}
