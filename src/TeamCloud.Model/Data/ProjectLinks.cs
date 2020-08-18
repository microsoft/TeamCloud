/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Data
{
    public sealed class ProjectLinks : ReferenceLinksContainer<Project, ProjectLinks>
    {
        public ProjectLinks() : this(null)
        { }

        public ProjectLinks(Project project) : base(project)
        {
            SetLink(nameof(Self), new ReferenceLink(()
                => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}").ToString()));

            SetLink(nameof(Identity), new ReferenceLink(()
                => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}/identity").ToString()));

            SetLink(nameof(Users), new ReferenceLink(()
                => GetBaseUri()?.AppendPath($"api/projects/{Context.Id}/users").ToString()));
        }

        [JsonProperty("_self")]
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

    }
}
