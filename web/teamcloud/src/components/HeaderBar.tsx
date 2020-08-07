import React from 'react';
import { UserInfo } from '.';
// import { FontIcon } from 'office-ui-fabric-react/lib/Icon';
import { IBreadcrumbItem, Breadcrumb } from '@fluentui/react';
import { useParams, useHistory } from 'react-router-dom';

export interface IHeaderBarProps {
    onSignOut: () => void;
}

export const HeaderBar: React.FunctionComponent<IHeaderBarProps> = (props) => {

    console.log('BAM');

    let history = useHistory();
    let { resourceName } = useParams();

    var items: IBreadcrumbItem[] = [
        { text: 'TeamCloud', key: 'app', href: '/', onClick: _onBreadcrumbItemClicked }
    ];

    if (resourceName) {
        items.push({ text: resourceName, key: 'lab' })
    }

    function _onBreadcrumbItemClicked(ev?: React.MouseEvent<HTMLElement, MouseEvent>, item?: IBreadcrumbItem): void {
        if (item?.href) history.push(item.href!)
    }

    return <div className="header">
        <span className="title">
            <Breadcrumb items={items} />
        </span>
        <UserInfo onSignOut={props.onSignOut} />
    </div>;
}

