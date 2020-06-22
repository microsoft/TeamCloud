/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeamCloud.Data.Conditional;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Data.Core;
using Xunit;

namespace TeamCloud.Data.CosmosDb
{
    [Collection(nameof(CosmosDbRepositoryCollection))]
    public class CosmosDbProvidersRepositoryTests : CosmosDbRepositoryTests<CosmosDbProvidersRepository>
    {
        private readonly CosmosDbRepositoryFixture fixture;

        public CosmosDbProvidersRepositoryTests(CosmosDbRepositoryFixture fixture)
            : base(new CosmosDbProvidersRepository(CosmosDbTestOptions.Instance))
        {
            this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        private static string SanitizeName(string name)
        {
            var sanitizedName = new StringBuilder(name.Length);

            foreach (var c in name.ToCharArray())
            {
                if (sanitizedName.Length > 0 && char.IsUpper(c))
                    sanitizedName.Append('.');

                sanitizedName.Append(char.ToLowerInvariant(c));
            }

            return sanitizedName.ToString();
        }

        private IEnumerable<ProviderReference> GetProviderReferences()
        {
            yield return new ProviderReference()
            {
                Id = SanitizeName(nameof(CosmosDbProvidersRepositoryTests))
            };
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task AddProvider()
        {
            var provider = await Repository.AddAsync(new Provider()
            {
                Id = SanitizeName(nameof(AddProvider)),
                Url = "https://www.foo.com",
                AuthCode = "3iexLbySHolb0Tsm5PErwg=="

            }).ConfigureAwait(false);

            AssertContainerDocumentMetadata(provider);
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task UpdateProvider()
        {
            var providerId = SanitizeName(nameof(UpdateProvider));

            var provider = await Repository.AddAsync(new Provider()
            {
                Id = providerId,
                Url = "https://www.foo.com",
                AuthCode = "3iexLbySHolb0Tsm5PErwg=="

            }).ConfigureAwait(false);

            Assert.Equal(providerId, provider.Id);
            AssertContainerDocumentMetadata(provider);

            var registered = DateTime.UtcNow;

            provider.Registered = registered;

            var provider2 = await Repository
                .SetAsync(provider)
                .ConfigureAwait(false);

            Assert.Equal(providerId, provider2.Id);
            AssertContainerDocumentMetadata(provider2);

            Assert.Equal(provider.Registered, provider2.Registered);
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task RemoveProvider()
        {
            var provider = await Repository.AddAsync(new Provider()
            {
                Id = SanitizeName(nameof(RemoveProvider)),
                Url = "https://www.foo.com",
                AuthCode = "3iexLbySHolb0Tsm5PErwg=="

            }).ConfigureAwait(false);

            AssertContainerDocumentMetadata(provider);

            await Repository
                .RemoveAsync(provider)
                .ConfigureAwait(false);
        }
    }
}
