using System;
using System.Threading.Tasks;
using TeamCloud.Data.Conditional;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using Xunit;

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

            Assert.NotNull(instance);

            var document = instance as IContainerDocument;

            Assert.NotNull(document);
            Assert.NotNull(document.ETag);
            Assert.NotNull(document.Timestamp);
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task AddProvider()
        {
            var instance1 = await Repository.GetAsync().ConfigureAwait(false);

            instance1.Providers.Add(new Provider()
            {
                Id = Guid.NewGuid().ToString()
            });

            var instance2 = await Repository.SetAsync(instance1).ConfigureAwait(false);

            Assert.NotNull(instance2);

            var document = instance2 as IContainerDocument;

            Assert.NotNull(document);
            Assert.NotNull(document.ETag);
            Assert.NotNull(document.Timestamp);
        }
    }
}
