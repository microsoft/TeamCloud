// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { CSSProperties, useState } from 'react';
import JSONPretty from 'react-json-pretty';
import { Dropdown, getTheme, IColumn, Icon, IconButton, IDropdownOption, Pivot, PivotItem, ScrollablePane, ScrollbarVisibility, Stack, Text } from '@fluentui/react';
import { useQuery, useQueryClient } from 'react-query'
import { CommandAuditEntity } from 'teamcloud';
import { api } from '../API';
import { ContentSearch, Lightbox } from './common';
import { useOrg, useAuditCommands } from '../hooks';
import { ContentList, ContentProgress } from '.';

import collaboration from '../img/MSC17_collaboration_010_noBG.png'

export const AuditList: React.FC = () => {

    const theme = getTheme();

    const timeRangeOptions: IDropdownOption[] = [
        { key: '00:05:00', text: 'last 5 minutes', selected: true },
        { key: '00:30:00', text: 'last 30 minutes' },
        { key: '01:00:00', text: 'last hour' },
        { key: '12:00:00', text: 'last 12 hours' },
        { key: '1.00:00:00', text: 'last 24 hours' },
        { key: '00:00:00', text: 'All time' },
    ];

    const [pivotKey, setPivotKey] = useState<string>('Details');
    const [itemFilter, setItemFilter] = useState<string>();
    const [selectedEntryId, setSelectedEntryId] = useState<string>();
    const [selectedCommands, setSelectedCommands] = useState<string[]>([]);
    const [selectedTimeRange, setSelectedTimeRange] = useState<string>(timeRangeOptions.find(o => (o.selected ?? false))?.key as string);

    const queryClient = useQueryClient();

    const { data: org } = useOrg();

    const { data: auditCommands, isLoading: auditCommandsLoading } = useAuditCommands();
    const { data: auditEntries, isLoading: auditEntriesLoading } = useQuery(['org', org?.id, 'audit', 'entries', selectedTimeRange, ...selectedCommands], async () => {
        const { data } = await api.getAuditEntries(org!.id, {
            timeRange: selectedTimeRange,
            commands: selectedCommands,
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });
        return data;
    }, {
        enabled: !!org?.id,
        cacheTime: 1000 * 30 // cache for 30 secs (opposed to default 5 mins)
    });

    const { data: auditEntry, isLoading: auditEntryLoading } = useQuery(['org', org?.id, 'audit', 'entries', selectedEntryId], async () => {
        const { data } = await api.getAuditEntry(selectedEntryId!, org!.id, {
            expand: true,
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });
        return data;
    }, {
        enabled: !!org?.id && !!selectedEntryId
    });


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

    const _applyFilter = (audit: CommandAuditEntity, filter: string): boolean => {
        if (!filter) return true;
        return JSON.stringify(audit).toUpperCase().includes(filter.toUpperCase())
    }

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

    return (
        <>
            <ContentProgress progressHidden={!auditEntriesLoading && !auditCommandsLoading && !auditEntryLoading} />
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
                                options={timeRangeOptions}
                                onChange={(e, o) => setSelectedTimeRange(o?.key as string)}
                                styles={{ dropdown: { minWidth: 250 } }}
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
                                options={auditCommands?.map(ac => ({ key: ac, text: ac } as IDropdownOption)) ?? []}
                                multiSelect
                                onChange={(e, o) => setSelectedCommands((o?.selected ?? false) ? [...selectedCommands, (o?.key ?? o?.text) as string] : selectedCommands.filter(c => c !== ((o?.key ?? o?.text) as string)))}
                                styles={{ dropdown: { minWidth: 250 } }}
                            />
                        </Stack>
                        <IconButton
                            theme={theme}
                            iconProps={{ iconName: 'Refresh' }}
                            onClick={() => queryClient.invalidateQueries(['org', org?.id, 'audit'])} />
                    </Stack>
                </ContentSearch>

                <ContentList
                    noCheck
                    noSearch
                    columns={columns}
                    items={auditEntries ? itemFilter ? auditEntries.filter(i => _applyFilter(i, itemFilter)) : auditEntries : undefined}
                    onItemInvoked={(entry) => setSelectedEntryId(entry.commandId)}
                    filterPlaceholder='Filter audit records'
                    noDataTitle='Not audit records match your query'
                    noDataDescription='Try changing your query parameters'
                    noDataImage={collaboration}
                />
            </Stack>
            <Lightbox
                title={`Command: ${auditEntry?.command} (${auditEntry?.commandId})`}
                titleSize='xxLargePlus'
                isOpen={(!!selectedEntryId && !!auditEntry)}
                onDismiss={() => setSelectedEntryId(undefined)}>
                <Pivot selectedKey={pivotKey} onLinkClick={(i, e) => setPivotKey(i?.props.itemKey ?? 'Details')} styles={{ root: { height: '100%', marginBottom: '12px' } }}>
                    <PivotItem headerText='Details' itemKey='Details'>
                        <div style={{ height: 'calc(100vh - 320px)', position: 'relative', maxHeight: 'inherit' }}>
                            <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                                <JSONPretty data={auditEntry ? JSON.stringify(auditEntry) : {}} />
                            </ScrollablePane>
                        </div>
                    </PivotItem>
                    <PivotItem headerText='Command' itemKey='Command'>
                        <div style={{ height: 'calc(100vh - 320px)', position: 'relative', maxHeight: 'inherit' }}>
                            <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                                <JSONPretty data={auditEntry?.commandJson} />
                            </ScrollablePane>
                        </div>
                    </PivotItem>
                    <PivotItem headerText='Result' itemKey='Result'>
                        <div style={{ height: 'calc(100vh - 320px)', position: 'relative', maxHeight: 'inherit' }}>
                            <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
                                <JSONPretty data={auditEntry?.resultJson} />
                            </ScrollablePane>
                        </div>
                    </PivotItem>
                </Pivot>
            </Lightbox>
        </>
    );
}
