import { BaseButton, Button, DefaultButton, Dialog, DialogFooter, IButtonProps, PrimaryButton, Stack, StackItem, Text, TextField } from "office-ui-fabric-react";
import React, { useRef, useState } from "react";

export interface IConfirmationButtonProps extends IButtonProps {
	confirmationTitle: string;
	confirmationBody?: string;
	confirmationValue?: string;
}

export const ConfirmationButton: React.FunctionComponent<IConfirmationButtonProps> = (props: IConfirmationButtonProps) => {

	const buttonRef = useRef<PrimaryButton>(null);

	const [hiddenDialog, SetHiddenDialog] = useState<boolean>(true);
	const [confirmationValue, SetConfirmationValue] = useState<string>();

	const requestConfirmation = (evt: React.MouseEvent<HTMLAnchorElement | HTMLButtonElement | HTMLDivElement | BaseButton | Button | HTMLSpanElement, MouseEvent>) => {
		evt.stopPropagation();
		SetHiddenDialog(false)
	}

	const onConfirmed = (evt: React.MouseEvent<HTMLAnchorElement | HTMLButtonElement | HTMLDivElement | BaseButton | Button | HTMLSpanElement, MouseEvent>) => {
		SetHiddenDialog(true);
		props.onClick && props.onClick(evt);
		buttonRef.current?.forceUpdate();
	}

	const renderConfirmationBody = () => {
		if (props.confirmationBody) {
			return props.confirmationBody
				.split(/\r?\n/)
				.map((val, idx) => <StackItem key={`body_${idx}`}><Text>{val}</Text></StackItem>)
		}
		return <></>
	}

	const renderConfirmationValue = () => {
		if (props.confirmationValue) {
			return 	<StackItem>
				<Text block nowrap style={{ marginBottom: 10 }}>
					To confirm this action, please type "<Text style={{ fontWeight: "bold"}}>{props.confirmationValue}</Text>":
				</Text>
				<TextField
					placeholder={props.confirmationValue}
					onChange={(evt, val) => SetConfirmationValue(val)} />
			</StackItem>
		}
		return <></>
	}

	return (
		<>
			<PrimaryButton {...props} onClick={requestConfirmation} ref={buttonRef} />
			<Dialog
				hidden={hiddenDialog}
				onDismiss={() => SetHiddenDialog(true)}
				dialogContentProps={{ title: props.confirmationTitle }}
				modalProps={{ isBlocking: true }}
				maxWidth={500}>
				<Stack style={{ minHeight: 100, minWidth: 450 }} tokens={{ childrenGap: '20px' }}>
					{ renderConfirmationBody() }
					{ renderConfirmationValue() }
				</Stack>
				<DialogFooter>
					<PrimaryButton {...props} onClick={onConfirmed} disabled={confirmationValue !== (props.confirmationValue ?? confirmationValue)} />
					<DefaultButton onClick={() => SetHiddenDialog(true)} text="Cancel" />
				</DialogFooter>
			</Dialog>
		</>
	);
};