import React from 'react';
import { Project } from '../model';

import {
    DocumentCard,
    DocumentCardTitle,
    IDocumentCardStyles,
    // DocumentCardImage,
    DocumentCardDetails,
    IDocumentCardTitleStyles,
    DocumentCardType,
    // ImageFit
} from '@fluentui/react';
import { GraphUser } from '../Auth';

// import { ImageFit } from "office-ui-fabric-react/lib/Image";
// import { Tenant } from "../models/Tenant";

export interface IProjectListProps {
    user: GraphUser,
    // tenant: Tenant,
    projects: Project[],
    resourceNameFilter?: string
}

export interface IProjectListState {

}

export class ProjectList extends React.Component<IProjectListProps, IProjectListState> {

    constructor(props: IProjectListProps) {
        super(props);
        this.state = this._getDefaultState();
    }

    private _getDefaultState(): IProjectListState {
        return {}
    }

    async componentDidMount() {
    }

    render() {

        const cardStyles: IDocumentCardStyles = {
            root: { display: 'inline-block', marginRight: 20, marginBottom: 20, width: 320 }
        };

        const titleStylesPrimary: IDocumentCardTitleStyles = {
            root: { paddingTop: 30 }
        }

        const titleStylesSecondary: IDocumentCardTitleStyles = {
            root: { paddingTop: 0 }
        }

        var projectCards: any[] = this.props.projects
            .filter(this._applyLabFilter)
            .map(project => {

                var link = window.origin + '/projects/' + project.id;

                return <DocumentCard
                    key={project.id}
                    styles={cardStyles}
                    type={DocumentCardType.normal}
                    onClickHref={link}
                >
                    {/* <DocumentCardImage height={100} imageFit={ImageFit.contain} imageSrc={LabLogo} /> */}
                    <DocumentCardDetails>
                        <DocumentCardTitle
                            title={project.name}
                            shouldTruncate={true}
                            styles={titleStylesPrimary} />
                        <DocumentCardTitle
                            title={project.id}
                            shouldTruncate={true}
                            showAsSecondaryTitle={true}
                            styles={titleStylesSecondary} />
                    </DocumentCardDetails>
                </DocumentCard>
            });

        return <div>{projectCards}</div>
    }

    private _applyLabFilter = (project: Project): boolean => {

        var match = true;

        if (this.props.resourceNameFilter) {
            match = project.name.toUpperCase().includes(this.props.resourceNameFilter.toUpperCase());
        }

        return match;
    }
}
