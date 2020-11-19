// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
// import React, { useState, useEffect } from 'react';
// import { Stack, Shimmer, DefaultButton, IButtonStyles, getTheme, Image } from '@fluentui/react';
import { Project } from 'teamcloud';
// import { ProjectDetailCard } from './ProjectDetailCard';
// import AppInsights from '../img/appinsights.svg';
// import DevTestLabs from '../img/devtestlabs.svg';
// import DevOps from '../img/devops.svg';
// import GitHub from '../img/github.svg';
// import { api } from '../API';


export interface IProjectLinksProps {
    project: Project;
}

export const ProjectLinks: React.FunctionComponent<IProjectLinksProps> = (props) => {
    return (<></>);
    // const [projectLinks, setProjectLinks] = useState<ProjectLink[]>();

    // useEffect(() => {
    //     if (props.project) {
    //         const _setLinks = async () => {
    //             const result = await api.getProjectLinks(props.project.id);
    //             setProjectLinks(result.data);
    //         };
    //         _setLinks();
    //     }
    // }, [props.project]);


    // // const _findKnownProviderImage = (link: ProjectLink) => {
    // //     if (link.href) {
    // //         if (link.href.includes('providers/Microsoft.Insights')) return AppInsights;
    // //         if (link.href.includes('dev.azure.com')) return DevOps;
    // //         if (link.href.includes('providers/Microsoft.DevTestLab')) return DevTestLabs;
    // //         if (link.href.includes('github.com')) return GitHub;
    // //     }
    // //     return undefined;
    // // }

    // const _getLinkTypeIcon = (link: ProjectLink) => {
    //     if (link.type)
    //         switch (link.type) { // VisualStudioIDELogo32
    //             case 'Link': return 'Link'; // Link12, FileSymlink, OpenInNewWindow, VSTSLogo
    //             case 'Readme': return 'PageList'; // Preview, Copy, FileHTML, FileCode, MarkDownLanguage, Document
    //             case 'Service': return 'Processing'; // Settings, Globe, Repair
    //             case 'AzureResource': return 'AzureLogo'; // AzureServiceEndpoint
    //             case 'GitRepository': return 'OpenSource';
    //             default: return undefined;
    //         }
    // }

    // const theme = getTheme();

    // const _linkButtonStyles: IButtonStyles = {
    //     root: {
    //         // border: 'none',
    //         width: '100%',
    //         textAlign: 'start',
    //         borderBottom: '1px',
    //         borderStyle: 'none none solid none',
    //         borderRadius: '0',
    //         borderColor: theme.palette.neutralLighter,
    //         padding: '24px 6px'
    //     }
    // }

    // const _linkTypes = ['AzureResource', 'Service', 'GitRepository', 'Readme', 'Link'];

    // const _getLinkStacks = (): JSX.Element[] => projectLinks ?
    //     projectLinks
    //         .sort((a, b) => _linkTypes.indexOf(a.type) - _linkTypes.indexOf(b.type))
    //         .map(l => (
    //             <Stack key={l.id} horizontal tokens={{ childrenGap: '12px' }}>
    //                 <Stack.Item styles={{ root: { width: '100%' } }}>
    //                     <DefaultButton
    //                         iconProps={{ iconName: _getLinkTypeIcon(l) }}
    //                         text={l.title}
    //                         href={l.href}
    //                         target='_blank'
    //                         styles={_linkButtonStyles} >
    //                         {/* <Image
    //                             src={_findKnownProviderImage(l)}
    //                             height={24} width={24} /> */}
    //                     </DefaultButton>
    //                 </Stack.Item>
    //             </Stack>
    //         )) : [];

    // return (
    //     <ProjectDetailCard title='Links' callout={projectLinks?.length?.toString() ?? '0'}>
    //         <Shimmer
    //             // customElementsGroup={_getShimmerElements()}
    //             isDataLoaded={projectLinks !== undefined}
    //             width={152} >
    //             <Stack tokens={{ childrenGap: '0' }} >
    //                 {_getLinkStacks()}
    //             </Stack>
    //         </Shimmer>
    //     </ProjectDetailCard>
    // );
}
