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
    }
}
