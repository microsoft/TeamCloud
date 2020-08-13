import React from 'react';
import { UserInfo } from '.';
// import { FontIcon } from 'office-ui-fabric-react/lib/Icon';
import { IBreadcrumbItem, Breadcrumb, Text, ITextStyles, Stack, getTheme, IStackStyles, Separator, SearchBox, ISearchBoxStyles, ICommandBarItemProps } from '@fluentui/react';
import { useParams, useHistory } from 'react-router-dom';
import { Project } from '../model';

export interface IHeaderBarProps {
    project?: Project;
    onSignOut: () => void;
}

export const HeaderBar: React.FunctionComponent<IHeaderBarProps> = (props) => {

    const history = useHistory();
    const { projectId } = useParams();

    const items: IBreadcrumbItem[] = [
        { text: 'Projects', key: 'projects', href: '/', onClick: () => _onBreadcrumbItemClicked }
    ];

    if (projectId && props.project && props.project.id === projectId)
        items.push({ text: props.project.name, key: 'project' });
    else if (!projectId && items.length > 1)
        items.pop();

    const _onBreadcrumbItemClicked = (ev?: React.MouseEvent<HTMLElement, MouseEvent>, item?: IBreadcrumbItem): void => {
        if (item?.href) history.push(item.href!)
    }

    const theme = getTheme();

    const stackStyles: IStackStyles = {
        root: {
            minHeight: '56px',
            background: theme.palette.themePrimary,
        }
    };

    const titleStyles: ITextStyles = {
        root: {
            minHeight: '56px',
            paddingLeft: '12px',
            fontSize: theme.fonts.xxLarge.fontSize,
            color: theme.palette.white
        }
    };

    return (
        <header>
            <Stack horizontal
                verticalFill
                horizontalAlign='space-between'
                verticalAlign='center'
                styles={stackStyles}>
                <Stack.Item>
                    <Text styles={titleStyles}>TeamCloud</Text>
                </Stack.Item>
                <Stack.Item>
                    <UserInfo onSignOut={props.onSignOut} />
                </Stack.Item>
            </Stack>
        </header>
    );
}

