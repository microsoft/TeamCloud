export interface ProjectLink {
    projectId: string;
    providerId: string;

    id: string;
    name: string;
    value: string;
    // value: any;
    location: string;
    isSecret: boolean;
    isShared: boolean;

    dataType: ProjectLinkType;
}

export enum ProjectLinkType {
    Link = 'Link',
    Readme = 'Readme',
    Service = 'Service',
    Resource = 'Resource',
    Repository = 'Repository'
}
