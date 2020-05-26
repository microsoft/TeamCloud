/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Data.CosmosDb.Core;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProjectsRepositoryTests : CosmosDbRepositoryTests<CosmosDbProjectsRepository>
    {
        public CosmosDbProjectsRepositoryTests()
            : base(new CosmosDbProjectsRepository(CosmosDbTestOptions.Default, new CosmosDbUsersRepository(CosmosDbTestOptions.Default)))
        { }
    }
}
