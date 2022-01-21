﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Data;

public sealed class ProjectReferenceLinks : ReferenceLinksContainer<Project, ProjectReferenceLinks>
{
    public ProjectReferenceLinks() : this(null)
    { }

    public ProjectReferenceLinks(Project project) : base(project)
    {
        SetLink(nameof(Self), new ReferenceLink(()
            => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}").ToString()));

        SetLink(nameof(Identity), new ReferenceLink(()
            => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}/identity").ToString()));

        SetLink(nameof(Users), new ReferenceLink(()
            => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}/users").ToString()));

        SetLink(nameof(Links), new ReferenceLink(()
            => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}/links").ToString()));

        SetLink(nameof(Offers), new ReferenceLink(()
            => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}/offers").ToString()));

        SetLink(nameof(Components), new ReferenceLink(()
            => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}/components").ToString()));
    }

    [JsonProperty("_self", Order = int.MinValue)]
    public ReferenceLink Self
    {
        get => GetLink();
        private set => SetLink(link: value);
    }

    public ReferenceLink Identity
    {
        get => GetLink();
        private set => SetLink(link: value);
    }

    public ReferenceLink Users
    {
        get => GetLink();
        private set => SetLink(link: value);
    }

    public ReferenceLink Links
    {
        get => GetLink();
        private set => SetLink(link: value);
    }

    public ReferenceLink Offers
    {
        get => GetLink();
        private set => SetLink(link: value);
    }

    public ReferenceLink Components
    {
        get => GetLink();
        private set => SetLink(link: value);
    }
}
