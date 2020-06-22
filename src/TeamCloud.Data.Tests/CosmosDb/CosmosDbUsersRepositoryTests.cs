/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Data.Conditional;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal.Data;
using Xunit;

namespace TeamCloud.Data.CosmosDb
{
    [Collection(nameof(CosmosDbRepositoryCollection))]
    public class CosmosDbUsersRepositoryTests : CosmosDbRepositoryTests<CosmosDbUsersRepository>
    {
        private readonly CosmosDbRepositoryFixture fixture;

        public CosmosDbUsersRepositoryTests(CosmosDbRepositoryFixture fixture)
            : base(new CosmosDbUsersRepository(CosmosDbTestOptions.Instance))
        {
            this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task AddUser()
        {
            var user = await Repository.AddAsync(new User()
            {
                Id = Guid.NewGuid().ToString(),
                Role = TeamCloudUserRole.Admin

            }).ConfigureAwait(false);

            AssertContainerDocumentMetadata(user);
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task UpdateUser()
        {
            var user = await Repository.AddAsync(new User()
            {
                Id = Guid.NewGuid().ToString(),
                Role = TeamCloudUserRole.Admin

            }).ConfigureAwait(false);

            AssertContainerDocumentMetadata(user);

            user.Role = TeamCloudUserRole.Creator;

            var user2 = await Repository
                .SetAsync(user)
                .ConfigureAwait(false);

            Assert.Equal(user.Id, user2.Id);
            AssertContainerDocumentMetadata(user2);
        }

        [ConditionalFact(ConditionalFactPlatforms.Windows)]
        public async Task RemoveUser()
        {
            var user = await Repository.AddAsync(new User()
            {
                Id = Guid.NewGuid().ToString(),
                Role = TeamCloudUserRole.Admin

            }).ConfigureAwait(false);

            AssertContainerDocumentMetadata(user);

            await Repository
                .RemoveAsync(user)
                .ConfigureAwait(false);
        }
    }
}
