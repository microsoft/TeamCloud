/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Data.CosmosDb.Core;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbUsersRepositoryTests : CosmosDbRepositoryTests<CosmosDbUsersRepository>
    {
        public CosmosDbUsersRepositoryTests()
            : base(new CosmosDbUsersRepository(CosmosDbTestOptions.Default))
        { }
    }
}
