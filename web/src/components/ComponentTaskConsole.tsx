// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useRef } from 'react';
import { useHistory, useLocation } from 'react-router-dom';
import { FocusZone, FocusZoneDirection, getTheme, IList, Link, List, ScrollToMode, SearchBox, Stack, Text } from '@fluentui/react';
import { ComponentTask } from 'teamcloud';
import { ContentSeparator } from '.';

export interface IComponentTaskConsoleProps {
    task?: ComponentTask;
}

export const ComponentTaskConsole: React.FunctionComponent<IComponentTaskConsoleProps> = (props) => {

    const history = useHistory();
    const location = useLocation();

    const theme = getTheme();

    const [output, setOutput] = useState<{ line: number, text: string, selected: boolean }[]>();
    const [outputFilter, setOutputFilter] = useState<string>();
    const [selectedLine, setSelectedLine] = useState<number>();

    const listRef = useRef<IList>(null);

    const { task } = props;

    useEffect(() => {
        // console.log(`+ setOutput`);
        setOutput(task?.output?.split('\n').map((t, i) => ({ line: i, text: t, selected: selectedLine === i })));
    }, [task, selectedLine]);


    useEffect(() => {
        if (location.hash !== '' && location.hash.startsWith('#')) {
            const index = location.hash.replace('#', '');
            const parsed = parseInt(index, 10);
            if (!isNaN(parsed)) {
                // console.log(`+ setSelectedLine (${parsed})`);
                setSelectedLine(parsed);
            }
        }

    }, [location.hash])


    useEffect(() => {
        if (output && selectedLine && listRef.current) {
            console.log(`+ setScroll`);
            listRef.current.scrollToIndex(selectedLine, i => 20, ScrollToMode.center);
        }
    }, [output, selectedLine, listRef]);


    const _searchBoxStyles = {
        root: {
            border: '1px solid transparent',
            borderRadius: theme.effects.roundedCorner4,
            backgroundColor: 'rgb(47,54,61)',
            color: 'rgb(149,157,165)',
        },
        field: {
            backgroundColor: 'rgb(47,54,61)',
            color: 'rgb(225,228,232)',
        },
        icon: {
            color: 'rgb(149,157,165)',
        }
    };

    const _getTaskName = (t?: ComponentTask) => t ? `Task: ${t.id}` : undefined;

    const _getTaskStatus = (t?: ComponentTask) => {
        if (t?.resourceState) {
            if (t.resourceState.toLowerCase() === 'succeeded' || t.resourceState.toLowerCase() === 'failed') {
                return t.finished ? `${t.resourceState} ${t.finished.toLocaleString()} (ExitCode: ${t.exitCode})` : t.resourceState;
            } else {
                return t.resourceState;
                // return `${d.resourceState}${dots}`;
            }
        } else if (t?.started) {
            return `Started ${t.started.toLocaleString()}`;
        }
        return undefined;
    };

    const textFont = 'SFMono-Regular,Consolas,Liberation Mono,Menlo,monospace!important';
    const textColor = 'rgb(225, 228, 232)';
    const textColorAlt = 'rgb(250, 251, 252)';
    const textColorError = 'rgb(249, 117, 131)';

    const bgColor = 'rgb(36,41,46)';
    const bgColorAlt = 'rgb(47,54,61)';
    const bgColorError = 'rgba(203, 36, 49, 0.15)';
    const bgColorSelected = 'rgba(33, 136, 255, 0.15)';

    const onRenderCell = (item?: { line: number, text: string, selected: boolean }, index?: number): JSX.Element => {
        if (!item) return (<></>);

        const failure = item.text.toLowerCase().includes('failed') || item.text.toLowerCase().includes('error');
        // const bg = item.selected ? bgColorSelected : failure ? bgColorError : undefined;
        //rgb(121, 184, 255)
        return (
            <Stack
                onClick={() => history.push(`#${item.line}`)}
                horizontal
                verticalAlign='start'
                styles={{
                    root: {
                        lineHeight: '20px',
                        backgroundColor: item.selected ? bgColorSelected : failure ? bgColorError : undefined,
                        selectors: {
                            '&:hover': {
                                color: item.selected || failure ? undefined : textColorAlt,
                                background: item.selected || failure ? undefined : bgColorAlt
                            },
                            '&:hover a': {
                                color: failure ? textColorError : textColorAlt,
                                background: item.selected || failure ? undefined : bgColorAlt
                            }
                        }
                    }
                }}>
                <Stack.Item shrink={false} styles={{ root: { width: '48px', textAlign: 'right' } }}>
                    <Link
                        styles={{
                            root: {
                                width: '48px',
                                color: failure ? textColorError : textColor,
                                selectors: {
                                    ':active': { color: theme.palette.blueLight },
                                    ':hover': { color: theme.palette.blueLight }
                                }
                            }
                        }}
                        href={`#${item.line}`}><pre>{item.line}</pre></Link>
                </Stack.Item>
                <Stack.Item styles={{ root: { marginLeft: '16px!important', wordBreak: 'break-all' } }}>
                    <pre>{item.text}</pre>
                </Stack.Item>
            </Stack>
        );
    };

    const lines = output ? outputFilter ? output.filter(o => JSON.stringify(o).toLowerCase().includes(outputFilter.toLowerCase())) : output : [];

    return (
        <Stack styles={{
            root: {
                height: '100%',
                borderRadius: theme.effects.roundedCorner4,
                color: textColor,
                backgroundColor: bgColor,
                fontFamily: textFont,
            }
        }}>
            <Stack.Item>
                <Stack styles={{ root: { padding: '14px 24px 0px 24px' } }} horizontal verticalFill horizontalAlign='space-between' verticalAlign='center'>
                    <Stack.Item>
                        <Stack tokens={{ childrenGap: '4px' }}>
                            <Text styles={{ root: { fontSize: '16px', fontWeight: '600' } }}>{_getTaskName(props.task)}</Text>
                            <Text styles={{ root: { color: 'rgb(149,157,165)', fontSize: '12px', fontWeight: '600' } }}>{_getTaskStatus(props.task)}</Text>
                        </Stack>
                    </Stack.Item>
                    <Stack.Item>
                        <SearchBox onChange={(_ev, val) => setOutputFilter(val)} styles={_searchBoxStyles} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item>
                <ContentSeparator color={theme.palette.neutralPrimary} />
            </Stack.Item>
            <Stack.Item styles={{ root: { overflowY: 'scroll', height: '664px', padding: '0px 16px 16px 16px' } }} data-is-scrollable>
                <FocusZone direction={FocusZoneDirection.vertical} >
                    <List
                        getPageSpecification={(i, r) => ({ height: 20 * 33, itemCount: 33 })}
                        componentRef={listRef} items={lines} onRenderCell={onRenderCell} />
                </FocusZone>
            </Stack.Item>
        </Stack>

    );
}
