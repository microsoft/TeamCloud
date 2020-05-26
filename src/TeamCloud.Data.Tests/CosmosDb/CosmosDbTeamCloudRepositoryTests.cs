/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Data.Conditional;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    public sealed class CosmosDbTeamCloudRepositoryTests : CosmosDbRepositoryTests<CosmosDbTeamCloudRepository>
    {
        public CosmosDbTeamCloudRepositoryTests()
            : base(new CosmosDbTeamCloudRepository(CosmosDbTestOptions.Default, CosmosDbTestCache.Default))
        { }


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
            AssertContainerDocumentMetadata(instance1);

            instance1.Providers.Add(new Provider()
            {
                Id = Guid.NewGuid().ToString()
            });

            var instance2 = await Repository.SetAsync(instance1).ConfigureAwait(false);
            AssertContainerDocumentMetadata(instance2);
        }
    }
}
