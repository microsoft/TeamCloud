// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { CSSProperties, useEffect, useState } from 'react';
import { Dropdown, getTheme, IColumn, Icon, IconButton, IDropdownOption, IDropdownStyles, IIconProps, Pivot, PivotItem, ScrollablePane, ScrollbarVisibility, Stack, Text } from '@fluentui/react';
import { useQueryClient } from 'react-query'
import collaboration from '../img/MSC17_collaboration_010_noBG.png'
import { useAuditCommands } from '../hooks/useAudit';
import { CommandAuditEntity } from 'teamcloud';
import { api } from '../API';
import { useParams } from 'react-router-dom';
import JSONPretty from 'react-json-pretty';
import { ContentSearch, Lightbox } from './common';
import { ContentList, ContentProgress } from '.';
import { useOrg } from '../hooks';

export const AuditList: React.FC = () => {

    const theme = getTheme();

    const { orgId } = useParams() as { orgId: string };

    const { data: auditCommands } = useAuditCommands();

    const [itemFilter, setItemFilter] = useState<string>();

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

            const { data } = await api.getAuditEntries(orgId, {
                timeRange: selectedTimeRange,
                commands: selectedCommands,
                onResponse: (raw, flat) => {
                    if (raw.status >= 400)
                        throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
                }
            }).finally(() => setAuditEntriesLoading(false));

            var auditEntriesUpdate = data ?? [];

            console.log(JSON.stringify({
                selectedTimeRange: selectedTimeRange,
                selectedCommands: selectedCommands,
                resultCount: auditEntriesUpdate.length
            }));

            setAuditEntries(auditEntriesUpdate);

        })();
    }, [orgId, selectedTimeRange, selectedCommands, refreshKey]);

    useEffect(() => {
        if (auditEntity) {

            (async () => {
                setAuditEntriesLoading(true);

                const { data } = await api.getAuditEntry(auditEntity.commandId!, auditEntity.organizationId!, {
                    expand: true,
                    onResponse: (raw, flat) => {
                        if (raw.status >= 400)
                            throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
                    }
                }).finally(() => setAuditEntriesLoading(false));;

                auditEntity.commandJson = auditEntity.commandJson ?? data?.commandJson;
                auditEntity.resultJson = auditEntity.resultJson ?? data?.resultJson;
            })();

        }
    }, [auditEntity])

    const onRenderDate = (date?: Date | null) =>
        <Text>{date?.toDateTimeDisplayString(false)}</Text>;

    const onRenderParentId = (parentId: string | undefined) =>
        parentId && parentId !== '00000000-0000-0000-0000-000000000000' ? <Text>{parentId}</Text> : <></>;

    const columns: IColumn[] = [
        { key: 'commandId', name: 'Command ID', minWidth: 300, maxWidth: 300, fieldName: 'commandId' },
        { key: 'parentId', name: 'Parent ID', minWidth: 300, maxWidth: 300, onRender: (a: CommandAuditEntity) => onRenderParentId(a?.parentId) },
        { key: 'command', name: 'Command', minWidth: 300, maxWidth: 10000, fieldName: 'command' },
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

    const { data: org } = useOrg();
    const queryClient = useQueryClient();

    const _onRefresh = () => {
        // setRefreshKey(refreshKey + 1);
        queryClient.invalidateQueries(['org', org!.id, 'audit'])
    };

    const _getCommandOptions = (): IDropdownOption[] => auditCommands
        ? auditCommands.map(ac => ({ key: ac, text: ac } as IDropdownOption))
        : [];

    const _applyFilter = (audit: CommandAuditEntity, filter: string): boolean => {
        if (!filter) return true;
        return JSON.stringify(audit).toUpperCase().includes(filter.toUpperCase())
    }

    return (
        <>
            <ContentProgress progressHidden={!auditEntriesLoading} />
            <Stack tokens={{ childrenGap: '20px' }}>
                <ContentSearch
                    placeholder='Filter audit records'
                    onChange={(_ev, val) => setItemFilter(val)}>
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
                                multiSelect
                                onChange={(e, o) => setSelectedCommands((o?.selected ?? false) ? [...selectedCommands, (o?.key ?? o?.text) as string] : selectedCommands.filter(c => c !== ((o?.key ?? o?.text) as string)))}
                                styles={dropdownStyles}
                            />
                        </Stack>
                        <IconButton
                            theme={theme}
                            iconProps={{ iconName: 'Refresh' }}
                            onClick={_onRefresh} />
                    </Stack>
                </ContentSearch>

                <ContentList
                    columns={columns}
                    items={auditEntries ? itemFilter ? auditEntries.filter(i => _applyFilter(i, itemFilter)) : auditEntries : undefined}
                    noSearch
                    noCheck
                    // applyFilter={_applyFilter}
                    onItemInvoked={setAuditEntity}
                    filterPlaceholder='Filter audit records'
                    noDataTitle='Not audit records match your query'
                    noDataDescription='Try changing your query parameters'
                    noDataImage={collaboration}
                />
            </Stack>
            <Lightbox
                title={`Command: ${auditEntity?.command} (${auditEntity?.commandId})`}
                titleSize='xxLargePlus'
                isOpen={(auditEntity ? true : false)}
                onDismiss={() => setAuditEntity(undefined)}>
                <Pivot selectedKey={pivotKey} onLinkClick={(i, e) => setPivotKey(i?.props.itemKey ?? 'Details')} styles={{ root: { height: '100%', marginBottom: '12px' } }}>
                    <PivotItem headerText='Details' itemKey='Details'>
                        <div style={{ height: 'calc(100vh - 320px)', position: 'relative', maxHeight: 'inherit' }}>
                            <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                                <JSONPretty data={auditEntity ? JSON.stringify(auditEntity) : {}} />
                            </ScrollablePane>
                        </div>
                    </PivotItem>
                    <PivotItem headerText='Command' itemKey='Command'>
                        <div style={{ height: 'calc(100vh - 320px)', position: 'relative', maxHeight: 'inherit' }}>
                            <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                                <JSONPretty data={auditEntity?.commandJson} />
                            </ScrollablePane>
                        </div>
                    </PivotItem>
                    <PivotItem headerText='Result' itemKey='Result'>
                        <div style={{ height: 'calc(100vh - 320px)', position: 'relative', maxHeight: 'inherit' }}>
                            <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                                <JSONPretty data={auditEntity?.resultJson} />
                            </ScrollablePane>
                        </div>
                    </PivotItem>
                </Pivot>
            </Lightbox>
        </>
    );
}
