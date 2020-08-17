/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Commands
{
    public sealed class ProviderCommandLinks : ReferenceLinksContainer<IProviderCommand, ProviderCommandLinks>
    {
        public ProviderCommandLinks() : this(null)
        { }

        public ProviderCommandLinks(IProviderCommand providerCommand = null) : base(providerCommand)
        {
            bool HasProviderId()
                => !string.IsNullOrEmpty(Context?.ProviderId);

            bool HasProjectId()
                => !string.IsNullOrEmpty(Context?.ProjectId);

            SetLink(nameof(SystemData), new ReferenceLink(()
                => HasProviderId() ? GetBaseUri()?.AppendPath($"api/providers/{Context.ProviderId}").ToString() : null));

            SetLink(nameof(ProjectData), new ReferenceLink(()
                => HasProviderId() && HasProjectId() ? GetBaseUri()?.AppendPath($"api/providers/{Context.ProviderId}").ToString() : null));
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
