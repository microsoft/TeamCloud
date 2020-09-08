// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { ProjectLink, ProjectLinkType, Project, DataResult } from '../model';
import { Stack, Shimmer, Image, DefaultButton, IButtonStyles, getTheme } from '@fluentui/react';
import { ProjectDetailCard } from './ProjectDetailCard';
import { ExampleProjectLinks } from '../data/ExampleData';
import AppInsights from '../img/appinsights.svg';
import DevOps from '../img/devops.svg';
import DevTestLabs from '../img/devtestlabs.svg';
import GitHub from '../img/github.svg';
import { getProjectLinks } from '../API';


export interface IProjectLinksProps {
    project: Project;
}

export const ProjectLinks: React.FunctionComponent<IProjectLinksProps> = (props) => {

    const [links, setLinks] = useState<ProjectLink[]>();

    // registerIcons({ icons: { AppInsights, DevOps, DevTestLabs, GitHub } });

    useEffect(() => {
        if (props.project) {
            const _setLinks = async () => {
                const result = await getProjectLinks(props.project.id);
                const data = (result as DataResult<ProjectLink[]>).data
                // let _links = ExampleProjectLinks;
                setLinks(data);
            };
            _setLinks();
        }
    }, [props.project]);


    // const _findKnownProviderImage = (link: ProjectLink) => {
    //     if (link.providerId) {
    //         if (link.providerId.startsWith('azure.appinsights')) return AppInsights;
    //         if (link.providerId.startsWith('azure.devops')) return DevOps;
    //         if (link.providerId.startsWith('azure.devtestlabs')) return DevTestLabs;
    //         if (link.providerId.startsWith('github')) return GitHub;
    //     }
    //     return undefined;
    // }

    const _getLinkTypeIcon = (link: ProjectLink) => {
        switch (link.type) { // VisualStudioIDELogo32
            case ProjectLinkType.Link: return 'Link'; // Link12, FileSymlink, OpenInNewWindow, VSTSLogo
            case ProjectLinkType.Readme: return 'PageList'; // Preview, Copy, FileHTML, FileCode, MarkDownLanguage, Document
            case ProjectLinkType.Service: return 'Processing'; // Settings, Globe, Repair
            case ProjectLinkType.AzureResource: return 'AzureLogo'; // AzureServiceEndpoint
            case ProjectLinkType.GitRepository: return 'OpenSource';
            default: return undefined;
        }
    }

    const theme = getTheme();

    const _linkButtonStyles: IButtonStyles = {
        root: {
            // border: 'none',
            width: '100%',
            textAlign: 'start',
            borderBottom: '1px',
            borderStyle: 'none none solid none',
            borderRadius: '0',
            borderColor: theme.palette.neutralLighter,
            padding: '24px 6px'
        }
    }

    const _linkTypes = [ProjectLinkType.AzureResource, ProjectLinkType.Service, ProjectLinkType.GitRepository, ProjectLinkType.Readme, ProjectLinkType.Link];

    const _getLinkStacks = () => links?.sort((a, b) => _linkTypes.indexOf(a.type) - _linkTypes.indexOf(b.type)).map(l => (
        <Stack horizontal tokens={{ childrenGap: '12px' }}>
            < Stack.Item styles={{ root: { width: '100%' } }}>
                <DefaultButton
                    iconProps={{ iconName: _getLinkTypeIcon(l) }}
                    text={l.title}
                    href={l.href}
                    target='_blank'
                    styles={_linkButtonStyles} >
                    {/* <Image
                        src={_findKnownProviderImage(l)}
                        height={24} width={24} /> */}
                </DefaultButton>
            </Stack.Item >
        </Stack >
    ));

    return (
        <ProjectDetailCard title='Resources' callout={links?.length.toString()}>
            <Shimmer
                // customElementsGroup={_getShimmerElements()}
                isDataLoaded={links ? links.length > 0 : false}
                width={152} >
                <Stack tokens={{ childrenGap: '0' }} >
                    {_getLinkStacks()}
                </Stack>
            </Shimmer>
        </ProjectDetailCard>
    );
}
