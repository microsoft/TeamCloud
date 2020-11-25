// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { PropsWithChildren, useState } from 'react';
import { CheckboxVisibility, DetailsList, DetailsListLayoutMode, FontWeights, getTheme, IColumn, IDetailsHeaderProps, IDetailsRowProps, IRenderFunction, ITextStyles, PrimaryButton, SearchBox, Stack, Text } from '@fluentui/react';

export interface IContentListProps<T> {
    items?: T[];
    columns?: IColumn[];
    filterPlaceholder?: string;
    applyFilter?: (item: T, filter: string) => boolean;
    onItemInvoked?: (item: T) => void;
    buttonText?: string;
    buttonIcon?: string;
    onButtonClick?: () => void;
    secondaryButtonText?: string;
    secondaryButtonIcon?: string;
    onSecondaryButtonClick?: () => void;
}

export const ContentList = <T,>(props: PropsWithChildren<IContentListProps<T>>) => {

    const [itemFilter, setItemFilter] = useState<string>();

    const theme = getTheme();

    const _onRenderRow: IRenderFunction<IDetailsRowProps> = (props?: IDetailsRowProps, defaultRender?: (props?: IDetailsRowProps) => JSX.Element | null): JSX.Element | null => {
        if (props) props.styles = { fields: { alignItems: 'center' }, check: { minHeight: '62px' }, cell: { fontSize: '14px' } }
        return defaultRender ? defaultRender(props) : null;
    };

    const _onRenderDetailsHeader: IRenderFunction<IDetailsHeaderProps> = (props?: IDetailsHeaderProps, defaultRender?: (props?: IDetailsHeaderProps) => JSX.Element | null): JSX.Element | null => {
        if (props) props.styles = { root: { paddingTop: '8px' } }
        return defaultRender ? defaultRender(props) : null;
    };

    const items: T[] = props.items ? (itemFilter && props.applyFilter !== undefined) ? props.items.filter(i => props.applyFilter!(i, itemFilter)) : props.items : [];

    const _titleStyles: ITextStyles = {
        root: {
            fontSize: '14px',
            fontWeight: FontWeights.regular,
        }
    }

    const _calloutStyles: ITextStyles = {
        root: {
            fontSize: '11px',
            fontWeight: FontWeights.regular,
            color: 'rgb(102, 102, 102)',
            backgroundColor: theme.palette.neutralLighter,
            padding: '2px 9px',
            borderRadius: '14px',
        }
    }


    if (props.items === undefined)
        return (<></>);

    if (props.items.length === 0)
        return (<Text styles={{ root: { width: '100%', paddingLeft: '8px' } }}>No projects</Text>)

    return (
        <Stack tokens={{ childrenGap: '20px' }}>
            { props.applyFilter && (<Stack styles={{
                root: {
                    padding: '10px 16px 10px 6px',
                    borderRadius: theme.effects.roundedCorner4,
                    boxShadow: theme.effects.elevation4,
                    backgroundColor: theme.palette.white
                }
            }} >
                <SearchBox
                    placeholder={props.filterPlaceholder ?? 'Filter members'}
                    iconProps={{ iconName: 'Filter' }}
                    onChange={(_ev, val) => setItemFilter(val)}
                    styles={{
                        root: {
                            border: 'none !important', selectors: {
                                '::after': { border: 'none !important' },
                                ':hover .ms-SearchBox-iconContainer': { color: theme.palette.neutralTertiary }
                            }
                        },
                        iconContainer: { color: theme.palette.neutralTertiary, },
                        field: { border: 'none !important' }
                    }} />
            </Stack>)}
            <Stack styles={{
                root: {
                    borderRadius: theme.effects.roundedCorner4,
                    boxShadow: theme.effects.elevation4,
                    backgroundColor: theme.palette.white
                }
            }} >
                <Stack horizontal verticalFill verticalAlign='baseline' horizontalAlign='space-between'
                    styles={{ root: { padding: '16px 16px 0px 16px', } }}>
                    <Stack.Item>
                        <Stack horizontal verticalFill verticalAlign='baseline' tokens={{ childrenGap: '5px' }}>
                            <Stack.Item>
                                <Text styles={_titleStyles}>Total</Text>
                            </Stack.Item>
                            <Stack.Item>
                                <Text styles={_calloutStyles}>{props.items.length}</Text>
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
                    {props.buttonText && (
                        <Stack.Item>
                            <PrimaryButton
                                text={props.buttonText}
                                iconProps={props.buttonIcon ? { iconName: props.buttonIcon } : undefined}
                                onClick={props.onButtonClick} />
                        </Stack.Item>
                    )}
                </Stack>

                <DetailsList
                    items={items}
                    columns={props.columns}
                    // isHeaderVisible={false}
                    onRenderRow={_onRenderRow}
                    onRenderDetailsHeader={_onRenderDetailsHeader}
                    // selectionMode={SelectionMode.none}
                    layoutMode={DetailsListLayoutMode.justified}
                    checkboxVisibility={CheckboxVisibility.always}
                    selectionPreservedOnEmptyClick={true}
                    onItemInvoked={props.onItemInvoked} />
            </Stack>
        </Stack>
    );
}
