import React from "react";
import { Spinner, CommandBar, ICommandBarItemProps, SearchBox } from '@fluentui/react';
import { GraphUser, getGraphUser } from '../Auth';
import { getProjects } from '../API'
// import { Project, ProjectType, TeamCloudUserRole, UserType, ProjectUserRole } from '../model'
import { Project } from '../model'
import { ProjectList } from "../components";

// const dummyProjectType: ProjectType = {
//     id: 'foo',
//     isDefault: true,
//     region: 'eastus',
//     subscriptions: ['00000000-0000-0000-0000-000000000000'],
//     subscriptionCapacity: 10,
//     providers: [
//         {
//             id: 'foo'
//         }
//     ]
// }

// const dummyProjects: Project[] = [
//     {
//         id: '00000000-0000-0000-0000-000000000000',
//         name: 'ProjectOne',
//         type: dummyProjectType,
//         resourceGroup: {
//             id: '00000000-0000-0000-0000-000000000000',
//             name: 'ProjectOne',
//             subscriptionId: '00000000-0000-0000-0000-000000000000',
//             region: 'eastus'
//         },
//         users: [
//             {
//                 id: '00000000-0000-0000-0000-000000000000',
//                 userType: UserType.User,
//                 role: TeamCloudUserRole.Admin,
//                 projectMemberships: [
//                     {
//                         projectId: '00000000-0000-0000-0000-000000000000',
//                         role: ProjectUserRole.Owner
//                     }
//                 ]
//             }
//         ]
//     }
// ]


export interface IHomeViewProps {
    // subscriptionId?: string;
    // tenantId: string;
}

export interface IHomeViewState {
    operation?: string;
    user?: GraphUser;
    // tenant?: Tenant;
    projects: Project[];
    resourceFilter?: string;
}

export class HomeView extends React.Component<IHomeViewProps, IHomeViewState> {

    constructor(props: IHomeViewProps) {
        super(props);
        this.state = this._getDefaultState();
    }

    private _getDefaultState(): IHomeViewState {
        return {
            operation: undefined,
            user: undefined,
            // tenant: undefined,
            projects: new Array<Project>(),
        }
    };

    componentDidMount() {
        this._refresh();
    }

    private _refresh = async () => {

        var promises: any[] = [
            this.state.user ? Promise.resolve<GraphUser>(this.state.user) : getGraphUser(),
            // this.state.tenant ? Promise.resolve<Tenant>(this.state.tenant) : fetchTenant(this.props.tenantId),
            getProjects()
        ];

        var results = await Promise.all(promises);

        this.setState({
            user: results[0],
            // tenant: results[1],
            projects: results[1].data,
            // projects: dummyProjects
        })
    }

    render() {

        var contentSection = <></>;

        if (this.state.operation || !this.state.user) {

            contentSection = <Spinner label={this.state.operation || ''} />;
        } else {
            contentSection = <ProjectList
                user={this.state.user}
                // tenant={this.state.tenant}
                projects={this.state.projects}
                resourceNameFilter={this.state.resourceFilter} />
        }

        return (
            <>
                <CommandBar
                    className="commands"
                    items={this._getCommandBarItems(false)}
                    farItems={this._getCommandBarItems(true)}
                />
                <div className="content">
                    {contentSection}
                </div>
            </>
        );
    }

    private _getCommandBarItems = (farItems: boolean): ICommandBarItemProps[] => {
        if (farItems) {

            return [
                {
                    key: 'search',
                    onRender: () => <SearchBox className="searchBox" iconProps={{ iconName: 'Filter' }} placeholder="Filter" onChange={(_, resourceFilter) => this.setState({ resourceFilter })} />
                },
                // {
                //     key: 'my',
                //     text: 'My',
                //     iconProps: { iconName: 'contact' },
                //     disabled: true
                // },
                // {
                //     key: 'all',
                //     text: 'All',
                //     iconProps: { iconName: 'people' },
                //     disabled: true
                // },
                // {
                //     key: 'pool',
                //     text: 'Pool',
                //     iconProps: { iconName: 'buildqueue' },
                //     disabled: true
                // }
            ];

        } else {

            return [
                // {
                //     key: 'machines',
                //     text: 'Machines',
                //     iconProps: { iconName: 'TVMonitor' },
                //     disabled: true
                // },
                // {
                //     key: 'environments',
                //     text: 'Environments',
                //     iconProps: { iconName: 'WebAppBuilderFragment' },
                //     disabled: true
                // },
                // {
                //     key: 'seperator',
                //     onRender: () => <div className="seperator" />
                // },
                {
                    key: 'refresh',
                    text: 'Refresh',
                    iconProps: { iconName: 'refresh' },
                    onClick: () => { this._refresh() }
                },
                {
                    key: 'create',
                    text: 'Create',
                    iconProps: { iconName: 'CirclePlus' },
                    disabled: true
                }
            ];
        }
    }
}
