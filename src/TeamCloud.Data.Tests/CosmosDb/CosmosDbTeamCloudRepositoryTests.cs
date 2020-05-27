/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TeamCloud.Data.Conditional;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using Xunit;

namespace TeamCloud.Data.CosmosDb
{
    [Collection(nameof(CosmosDbRepositoryCollection))]
    public sealed class CosmosDbTeamCloudRepositoryTests : CosmosDbRepositoryTests<CosmosDbTeamCloudRepository>
    {
        private readonly CosmosDbRepositoryFixture fixture;

        public CosmosDbTeamCloudRepositoryTests(CosmosDbRepositoryFixture fixture)
            : base(new CosmosDbTeamCloudRepository(CosmosDbTestOptions.Instance, CosmosDbTestCache.Instance))
        {
            this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task GetInstance()
        {
            var instance = await Repository.GetAsync().ConfigureAwait(false);
            AssertContainerDocumentMetadata(instance);
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task AddProvider()
        {
            var instance1 = await Repository.GetAsync().ConfigureAwait(false);
            var instance1json = JsonConvert.SerializeObject(instance1);
            AssertContainerDocumentMetadata(instance1);

            instance1.Providers.Add(new Provider()
            {
                Id = Guid.NewGuid().ToString()
            });

            var instance2 = await Repository.SetAsync(instance1).ConfigureAwait(false);
            var instance2json = JsonConvert.SerializeObject(instance2);
            AssertContainerDocumentMetadata(instance2);

        }
    }
}
