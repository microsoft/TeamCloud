/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Commands
{
    public sealed class ProviderCommandLinks : ReferenceLinksContainer<IProviderCommand, ProviderCommandLinks>
    {
        public ProviderCommandLinks() : this(null)
        { }

        public ProviderCommandLinks(IProviderCommand providerCommand = null) : base(providerCommand)
        {
            SetLink(nameof(SystemData), new ReferenceLink(()
                => GetBaseUri()?.AppendPath($"api/providers/{Context.ProviderId}").ToString()));

            SetLink(nameof(ProjectData), new ReferenceLink(()
                => string.IsNullOrEmpty(Context.ProjectId) ? default : GetBaseUri()?.AppendPath($"api/providers/{Context.ProviderId}").ToString()));
        }

        public ReferenceLink SystemData
        {
            get => GetLink();
            private set => SetLink(link: value);
        }

        public ReferenceLink ProjectData
        {
            get => GetLink();
            private set => SetLink(link: value);
        }
    }
}
