// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { CSSProperties, useEffect, useState } from 'react';
import { ContentList, ContentProgress } from '.';

import collaboration from '../img/MSC17_collaboration_010_noBG.png'
import { useAuditCommands } from '../hooks/useAudit';
import { Dropdown, getTheme, IColumn, Icon, IconButton, IDropdownOption, IDropdownStyles, IIconProps, Modal, Pivot, PivotItem, Stack, Text } from '@fluentui/react';
import { CommandAuditEntity, ErrorResult } from 'teamcloud';
import { api } from '../API';
import { useParams } from 'react-router-dom';
import JSONPretty from 'react-json-pretty';

export const AuditList: React.FC = () => {

    const theme = getTheme();

    const { orgId } = useParams() as { orgId: string };

    const { data: auditCommands } = useAuditCommands();

    const _getTimeRangeOptions = (): IDropdownOption[] => [
        { key: '00:05:00', text: 'last 5 minutes', selected: true },
        { key: '00:30:00', text: 'last 30 minutes' },
        { key: '01:00:00', text: 'last hour' },
        { key: '12:00:00', text: 'last 12 hours' },
        { key: '1.00:00:00', text: 'last 24 hours' },
        { key: '00:00:00', text: 'ALL' },        
    ];

    const [selectedTimeRange, setSelectedTimeRange] = useState<string>(_getTimeRangeOptions().find(o => (o.selected ?? false))?.key as string);
    const [selectedCommands, setSelectedCommands] = useState<string[]>([]);

    const [auditEntity, setAuditEntity] = useState<CommandAuditEntity>();
    const [auditEntries, setAuditEntries] = useState<CommandAuditEntity[]>();
    const [auditEntriesLoading, setAuditEntriesLoading] = useState<boolean>();
    const [refreshKey, setRefreshKey] = useState<number>(0);
    const [pivotKey, setPivotKey] = useState<string>('Details');

    useEffect(() => {
        (async () => {

            setAuditEntriesLoading(true);

            const { data, code, _response } = await api.getAuditEntries(orgId, {
                timeRange: selectedTimeRange,
                commands: selectedCommands
            }).finally(() => setAuditEntriesLoading(false));
    
            if (code && code >= 400) {
                const error = JSON.parse(_response.bodyAsText) as ErrorResult;
                throw error;
            }
    
            var auditEntriesUpdate = data ?? [];

            console.log(JSON.stringify({
                selectedTimeRange: selectedTimeRange,
                selectedCommands: selectedCommands,
                resultCount: auditEntriesUpdate.length
            }));

            setAuditEntries(auditEntriesUpdate);

        })();
    }, [orgId, selectedTimeRange, selectedCommands, refreshKey]);

    useEffect(()=> {
        if (auditEntity) {

            (async () => {
                setAuditEntriesLoading(true);

                const { data, code, _response } = await api.getAuditEntry(auditEntity.commandId!, auditEntity.organizationId!, {
                    expand: true
                }).finally(() => setAuditEntriesLoading(false));;

                if (code && code >= 400) {
                    const error = JSON.parse(_response.bodyAsText) as ErrorResult;
                    throw error;
                }

                auditEntity.commandJson = auditEntity.commandJson ?? data?.commandJson;
                auditEntity.resultJson = auditEntity.resultJson ?? data?.resultJson;
            })();

        } 
    }, [auditEntity])

    const onRenderDate = (date?: Date | null) =>
        <Text>{date?.toDateTimeDisplayString(false)}</Text>;

    const columns: IColumn[] = [
        { key: 'commandId', name: 'Command ID', minWidth: 300, maxWidth: 300, fieldName: 'commandId' },
        { key: 'command', name: 'Command', minWidth: 300, maxWidth: 300, fieldName: 'command' },
        { key: 'runtimeStatus', name: 'Runtime Status', minWidth: 100, maxWidth: 100, fieldName: 'runtimeStatus' },
        { key: 'customStatus', name: 'Custom Status', minWidth: 100, maxWidth: 100, fieldName: 'customStatus' },
        { key: 'created', name: 'Created', minWidth: 200, onRender: (a: CommandAuditEntity) => onRenderDate(a?.created) },
        { key: 'updated', name: 'Updated', minWidth: 200, onRender: (a: CommandAuditEntity) => onRenderDate(a?.updated) },
    ];

    const iconContainerStyle: CSSProperties = {
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        flexShrink: 0,
        fontSize: '16px',
        width: '32px',
        textAlign: 'center',
        color: 'rgb(161, 159, 157)',
        cursor: 'text',
        transition: 'width 0.167s ease 0s'
    };

    const dropdownStyles: Partial<IDropdownStyles> = {
        dropdown: { minWidth: 250 },
    };

    const refreshIcon: IIconProps = { iconName: 'Refresh' };

    const _onRefresh = () => {
        setRefreshKey(refreshKey + 1);
    };

    const _getCommandOptions = () : IDropdownOption[] => auditCommands
        ? auditCommands.map(ac => ({ text: ac } as IDropdownOption))
        : [];

    const _applyFilter = (audit: CommandAuditEntity, filter: string): boolean => {
        if (filter) {
            return ((audit?.command?.toUpperCase().includes(filter.toUpperCase()) ?? false)
                || (audit?.commandId?.toUpperCase().includes(filter.toUpperCase()) ?? false)
                || (audit?.componentTask?.toUpperCase().includes(filter.toUpperCase()) ?? false)
                || (audit?.customStatus?.toUpperCase().includes(filter.toUpperCase()) ?? false)
                || (audit?.errors?.toUpperCase().includes(filter.toUpperCase()) ?? false)
                || (audit?.parentId?.toUpperCase().includes(filter.toUpperCase()) ?? false)
                || (audit?.projectId?.toUpperCase().includes(filter.toUpperCase()) ?? false)
                || (audit?.runtimeStatus?.toUpperCase().includes(filter.toUpperCase()) ?? false));
        }
        return true;
    }

    const _onItemInvoked = (audit: CommandAuditEntity): void => {
        setAuditEntity(audit);
    };

    const _onRenderBeforeSearchBox = () => (
        <Stack horizontal tokens={{ childrenGap: '20px' }} style={{ marginLeft: '10px' }}>
            <Stack horizontal tokens={{ childrenGap: '10px' }}>
                <Stack
                    className='ms-SearchBox-iconContainer'
                    theme={theme}
                    style={iconContainerStyle}>
                    <Icon
                        theme={theme}
                        iconName='Calendar'
                        className='ms-SearchBox-icon'
                    />
                </Stack>
                <Dropdown
                    theme={theme}
                    title="Time range"
                    options={_getTimeRangeOptions()}
                    onChange={(e, o) => setSelectedTimeRange(o?.key as string)}
                    styles={dropdownStyles}
                />
            </Stack>
            <Stack horizontal tokens={{ childrenGap: '10px' }} >
                <Stack
                    className='ms-SearchBox-iconContainer'
                    theme={theme}
                    style={iconContainerStyle}>
                    <Icon
                        theme={theme}
                        iconName='ReturnKey'
                        className='ms-SearchBox-icon'
                    />
                </Stack>
                <Dropdown
                    theme={theme}
                    title="Command"
                    options={_getCommandOptions()}
                    multiSelect={true}
                    onChange={(e, o) => setSelectedCommands((o?.selected ?? false) ? [...selectedCommands, (o?.key ?? o?.text) as string] : selectedCommands.filter(c => c !== ((o?.key ?? o?.text) as string)))}
                    styles={dropdownStyles}
                />
            </Stack>
        </Stack>
    );

    const _onRenderAfterSearchBox = () => (
        <IconButton
            theme={theme}
            iconProps={refreshIcon}
            onClick={_onRefresh} />
    );

    return (
        <>
            <ContentProgress progressHidden={!auditEntriesLoading} />
            <ContentList
                columns={columns}
                items={auditEntries ?? undefined}
                onRenderBeforeSearchBox={_onRenderBeforeSearchBox}
                onRenderAfterSearchBox={_onRenderAfterSearchBox}
                applyFilter={_applyFilter}
                onItemInvoked={_onItemInvoked}
                filterPlaceholder='Filter audit records'
                noCheck={true}
                noDataTitle='You do not have any audit records yet'
                noDataImage={collaboration}
            />
            <Modal
                theme={theme}
                styles={{ main: { margin: 'auto 100px', minHeight:'calc(100% - 32px)', minWidth:'calc(100% - 32px)' }, scrollableContent: { padding: '50px' } }}
                isBlocking={false}
                isOpen={(auditEntity ? true : false)}
                onDismiss={() => setAuditEntity(undefined)}>
                <Stack tokens={{ childrenGap: '12px' }}>
                    <Stack.Item>
                        <Stack horizontal horizontalAlign='space-between' 
                            tokens={{ childrenGap: '50px' }} 
                            style={{ paddingBottom: '32px', borderBottom: '1px lightgray solid' }}>
                            <Stack.Item>
                                <Text variant='xxLargePlus'>{`Command: ${auditEntity?.command} (${auditEntity?.commandId})`}</Text>
                            </Stack.Item>
                            <Stack.Item>
                                <IconButton iconProps={{ iconName: 'ChromeClose' }}
                                    onClick={() => setAuditEntity(undefined)} />
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
                    <Stack.Item>
                        <Pivot selectedKey={pivotKey} onLinkClick={(i, e) => setPivotKey(i?.props.itemKey ?? 'Details')} styles={{ root: { height: '100%', marginBottom: '12px' } }}>
                            <PivotItem headerText='Details' itemKey='Details'>
                                <JSONPretty data={auditEntity} />
                            </PivotItem>
                            <PivotItem headerText='Command' itemKey='Command'>
                                <JSONPretty data={auditEntity?.commandJson} />
                            </PivotItem>
                            <PivotItem headerText='Result' itemKey='Result'>
                                <JSONPretty data={auditEntity?.resultJson} />
                            </PivotItem>
                        </Pivot>
                    </Stack.Item>
                </Stack>
            </Modal>
        </>
    );
}
