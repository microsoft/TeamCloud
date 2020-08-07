import React from 'react';
// import { useLocation } from 'react-router-dom';
import { GraphUser, getGraphUser } from '../Auth';
import { Project } from '../model';
import { getProject } from '../API';
import { Stack, Text, Spinner } from '@fluentui/react';

export interface IProjectViewProps {
    projectId: string;
}

export interface IProjectViewState {

    operation?: string;
    user?: GraphUser;
    // tenant?: Tenant;
    project: Project;

    // resourceFilter?: string;
    // resourceType: ResourceTypeFilter,
    // resourceOwner: ResourceOwnerFilter,
    // resourceTrash: ResourceIdentifier[],

    // virtualMachines: VirtualMachine[],
    // environments: Environment[],

    // announcementVisible: boolean,
    // cleanupVisible: boolean
}
export class ProjectView extends React.Component<IProjectViewProps, IProjectViewState> {

    constructor(props: IProjectViewProps) {
        super(props);
        this.state = this._getDefaultState();
    }


    private _getDefaultState(): IProjectViewState {
        return {
            operation: undefined,
            user: undefined,
            // tenant: undefined,
            project: {} as Project,
            // resourceFilter: undefined,
            // resourceType: ResourceTypeFilter.machine,
            // resourceOwner: ResourceOwnerFilter.my,
            // resourceTrash: new Array<ResourceIdentifier>(),
            // virtualMachines: new Array<VirtualMachine>(),
            // environments: new Array<Environment>(),
            // announcementVisible: false,
            // cleanupVisible: false
        }
    };

    componentDidMount() {
        this._refresh();
    }

    private _refresh = async () => {

        let promises: any[] = [
            this.state.user ? Promise.resolve<GraphUser>(this.state.user) : getGraphUser(),
            // this.state.tenant ? Promise.resolve<Tenant>(this.state.tenant) : fetchTenant(this.props.tenantId),
            getProject(this.props.projectId)
        ];

        let results = await Promise.all(promises);

        this.setState({
            user: results[0],
            // tenant: results[1],
            project: results[1].data,
            // project: dummyProjects
        })
    }

    render() {
        if (this.state.project.id)
            return (
                <Stack verticalFill verticalAlign='center' horizontalAlign='center'>
                    <Text>{this.state.project.id}</Text>
                    <Text>{this.state.project.name}</Text>
                    <Text>{this.state.project.resourceGroup.id}</Text>
                    <Text>{this.state.project.resourceGroup.name}</Text>
                    <Text>{this.state.project.resourceGroup.region}</Text>
                    <Text>{this.state.project.resourceGroup.subscriptionId}</Text>
                    <Text>{this.state.project.type.id}</Text>
                    <Text>{this.state.project.type.isDefault}</Text>
                    <Text>{this.state.project.type.providers[0].id}</Text>
                    <Text>{this.state.project.type.region}</Text>
                    <Text>{this.state.project.users[0].id}</Text>
                    <Text>{this.state.project.users[0].role}</Text>
                    <Text>{this.state.project.users[0].userType}</Text>
                    {/* <Text>{this.state.project.users[0].projectMemberships[0].projectId}</Text>
                    <Text>{this.state.project.users[0].projectMemberships[0].role}</Text> */}
                </Stack>
            );
        return (<Stack verticalFill verticalAlign='center' horizontalAlign='center'><Spinner /></Stack>);
    }
}
