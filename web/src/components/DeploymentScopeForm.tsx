// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useState } from 'react';
import { ComboBox, DefaultButton, IComboBoxOption, PrimaryButton, Stack, TextField } from '@fluentui/react';
import { useHistory, useParams } from 'react-router-dom';
import { FuiForm } from '@rjsf/fluent-ui'
import { IChangeEvent, ISubmitEvent } from '@rjsf/core';
import { DeploymentScopeDefinition } from 'teamcloud';
import { useAdapters, useCreateDeploymentScope } from '../hooks';
import { ContentSeparator, TCFieldTemplate } from '.';

export interface IDeploymentScopeFormProps {
    embedded?: boolean,
    onScopeChange?: (scope?: DeploymentScopeDefinition) => void;
    createDeploymentScope?: (scope: DeploymentScopeDefinition) => Promise<void>;
}

export const DeploymentScopeForm: React.FC<IDeploymentScopeFormProps> = (props) => {

    const { onScopeChange } = props;
    const history = useHistory();
    const { orgId } = useParams() as { orgId: string };

    const { data: adapterInformation } = useAdapters();

    const [scopeName, setScopeName] = useState<string>();
    const [scopeType, setScopeType] = useState<string>();
    const [scopeTypeOptions, setScopeTypeOptions] = useState<IComboBoxOption[]>();
    const [scopeTypeSchema, setScopeTypeSchema] = useState<string>();
    const [scopeTypeForm, setScopeTypeForm] = useState<string>();
    const [scopeTypeData, setScopeTypeData] = useState<string>();

    const [formEnabled, setFormEnabled] = useState<boolean>(true);
    const [formCompleted, setFormCompleted] = useState<boolean>();

    const _createScope = useCreateDeploymentScope();

    const _createDefinition = useCallback((): DeploymentScopeDefinition => {
        return {
            displayName: scopeName,
            type: scopeType,
            inputData: scopeTypeData,
            managementGroupId: undefined,
            subscriptionIds: undefined,
            // isDefault: true
        } as DeploymentScopeDefinition;
    }, [scopeName, scopeType, scopeTypeData]);

    const _changeForm = async (e: IChangeEvent<any>) => {
        console.log("Form errors: " + e.errors.length);
        // console.log(JSON.stringify(e.formData))
        setScopeTypeData(e.errors.length === 0 ? JSON.stringify(e.formData) : undefined);
    };

    const _resetAndCloseForm = () => {
        setFormEnabled(true);
        history.push(`/orgs/${orgId}/settings/scopes`);
    };

    useEffect(() => {
        if (scopeTypeOptions === undefined) {
            // console.log('+ scopeTypeOptions');
            var options = adapterInformation?.map(info => ({ key: info.type?.toString(), text: info.displayName })) as IComboBoxOption[];
            setScopeType(options?.find(option => option !== undefined)?.key as string);
            setScopeTypeOptions(options);
        }
        if (scopeType && adapterInformation) {
            var scopeTypeInfo = adapterInformation?.find(info => info && info.type === scopeType)
            // console.log("ScopeTypeInfo = " + JSON.stringify(scopeTypeInfo));
            // console.log(JSON.stringify(JSON.parse(scopeTypeInfo!.inputDataSchema!)));
            // console.log(JSON.stringify(JSON.parse(scopeTypeInfo!.inputDataForm!)));
            setScopeTypeSchema(scopeTypeInfo?.inputDataSchema || undefined);
            setScopeTypeForm(scopeTypeInfo?.inputDataForm || undefined);
        } else {
            setScopeTypeSchema(undefined);
            setScopeTypeForm(undefined);
        }
    }, [scopeType, scopeTypeOptions, adapterInformation])

    useEffect(() => {
        if (scopeName && scopeType && scopeTypeData) {
            setFormCompleted(true);
        } else {
            setFormCompleted(false);
        }
        if (onScopeChange) {
            const scopeDef = _createDefinition();
            onScopeChange(scopeDef);
        }
    }, [scopeName, scopeType, scopeTypeData, onScopeChange, _createDefinition]);


    const _submitForm = async (e: ISubmitEvent<any>) => {
        if (orgId && formCompleted) {

            setFormEnabled(false);

            const scopeDef = _createDefinition();

            if (props.createDeploymentScope) {
                props.createDeploymentScope(scopeDef);
            } else {
                _createScope(scopeDef);
                _resetAndCloseForm();
            }
        }
    };

    return (
        <Stack tokens={{ childrenGap: '20px' }} styles={{ root: props.embedded ? { padding: '24px 8px' } : undefined }}>
            <Stack.Item>
                <TextField
                    required
                    label='Name'
                    description='Deployment scope display name'
                    disabled={!formEnabled}
                    value={scopeName}
                    onChange={(_ev, val) => setScopeName(val)} />
            </Stack.Item>
            <Stack.Item>
                <ComboBox
                    required
                    label='Type'
                    disabled={!formEnabled}
                    selectedKey={scopeType}
                    options={scopeTypeOptions ?? []}
                    onChange={(_ev, val) => setScopeType(val ? val.key as string : undefined)} />
            </Stack.Item>
            <Stack.Item>
                <FuiForm
                    disabled={!formEnabled}
                    omitExtraData={true}
                    liveOmit={true}
                    onSubmit={_submitForm}
                    onChange={_changeForm}
                    FieldTemplate={TCFieldTemplate}
                    formData={JSON.parse(scopeTypeData ?? '{}')}
                    schema={JSON.parse(scopeTypeSchema ?? '{}')}
                    uiSchema={JSON.parse(scopeTypeForm ?? '{}')}>
                    {props.embedded ? <></> : <div><ContentSeparator />
                        <div style={{ paddingTop: '24px' }}>
                            <PrimaryButton text='Create scope' type='submit' hidden={props.embedded ? true : false} disabled={!formEnabled || !formCompleted} styles={{ root: { marginRight: 8 } }} />
                            <DefaultButton text='Cancel' hidden={props.embedded ? true : false} disabled={!formEnabled} onClick={() => setScopeTypeData(undefined)} />
                        </div></div>}
                </FuiForm>
            </Stack.Item>

        </Stack>
    );
}

