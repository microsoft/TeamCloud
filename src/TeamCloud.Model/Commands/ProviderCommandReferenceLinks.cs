/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Commands
{
    public sealed class ProviderCommandReferenceLinks : ReferenceLinksContainer<IProviderCommand, ProviderCommandReferenceLinks>
    {
        public ProviderCommandReferenceLinks() : this(null)
        { }

        public ProviderCommandReferenceLinks(IProviderCommand providerCommand = null) : base(providerCommand)
        {
            bool HasProviderId()
                => !string.IsNullOrEmpty(Context?.ProviderId);

            bool HasProjectId()
                => !string.IsNullOrEmpty(Context?.ProjectId);

            SetLink(nameof(SystemData), new ReferenceLink(()
                => HasProviderId() ? GetBaseUri()?.AppendPath($"api/providers/{Context.ProviderId}/data").ToString() : null));

            SetLink(nameof(ProjectData), new ReferenceLink(()
                => HasProviderId() && HasProjectId() ? GetBaseUri()?.AppendPath($"api/projects/{Context.ProjectId}/providers/{Context.ProviderId}").ToString() : null));

            SetLink(nameof(Offers), new ReferenceLink(()
                => HasProviderId() ? GetBaseUri()?.AppendPath($"api/providers/{Context.ProviderId}/offers").ToString() : null));

            SetLink(nameof(ProjectComponents), new ReferenceLink(()
                => HasProviderId() && HasProjectId() ? GetBaseUri()?.AppendPath($"api/projects/{Context.ProjectId}/providers/{Context.ProviderId}/components").ToString() : null));
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

        public ReferenceLink Offers
        {
            get => GetLink();
            private set => SetLink(link: value);
        }

        public ReferenceLink ProjectComponents
        {
            get => GetLink();
            private set => SetLink(link: value);
        }
    }
}
